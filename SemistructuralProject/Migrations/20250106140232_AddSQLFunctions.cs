using Microsoft.EntityFrameworkCore.Migrations;

namespace SemistructuralProject.Migrations
{
    public partial class AddSQLFunctions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE TYPE dbo.PointsTableType AS TABLE
                (
                    X FLOAT,
                    Y FLOAT
                );
            ");

            migrationBuilder.Sql(@"
                CREATE FUNCTION dbo.GetDistanceByCoordinates(@x1 FLOAT, @y1 FLOAT, @x2 FLOAT, @y2 FLOAT)
                RETURNS FLOAT
                AS
                BEGIN
                    RETURN SQRT(POWER(@x2 - @x1, 2) + POWER(@y2 - @y1, 2));
                END;
            ");

            migrationBuilder.Sql(@"
                CREATE FUNCTION dbo.GetDistanceByPointsId(@id1 INT, @id2 INT)
                RETURNS FLOAT
                AS
                BEGIN
                    DECLARE @x1 FLOAT , @y1 FLOAT, @x2 FLOAT, @y2 FLOAT;

                    SELECT @x1 = X, @y1 = Y FROM Points WHERE Id = @id1;
                    SELECT @x2 = X, @y2 = Y FROM Points WHERE Id = @id2;

                    RETURN SQRT(POWER(@x2 - @x1, 2) + POWER(@y2 - @y1, 2));
                END;
            ");

            migrationBuilder.Sql(@"
                -- Function for Ray Casting Algorithm
                CREATE FUNCTION dbo.IsPointInPolygon_RayCasting(
                    @X FLOAT, -- X-coordinate of the point to check
                    @Y FLOAT, -- Y-coordinate of the point to check
                    @Points dbo.PointsTableType READONLY -- Table of points with columns X and Y
                )
                RETURNS BIT
                AS
                BEGIN
                    DECLARE @CrossCount INT = 0;
                    DECLARE @PrevX FLOAT;
                    DECLARE @PrevY FLOAT;
                    DECLARE @CurrX FLOAT;
                    DECLARE @CurrY FLOAT;

                    -- Close the polygon by connecting the last point to the first
                    DECLARE @PointTable TABLE (PointOrder INT, X FLOAT, Y FLOAT);
                    INSERT INTO @PointTable (PointOrder, X, Y)
                    SELECT ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS PointOrder, X, Y
                    FROM @Points;

                    INSERT INTO @PointTable (PointOrder, X, Y)
                    SELECT 0, X, Y FROM @PointTable WHERE PointOrder = 1;

                    -- Iterate through each edge of the polygon
                    DECLARE PointCursor CURSOR FOR
                        SELECT X, Y FROM @PointTable ORDER BY PointOrder;

                    OPEN PointCursor;
                    FETCH NEXT FROM PointCursor INTO @PrevX, @PrevY;
                    FETCH NEXT FROM PointCursor INTO @CurrX, @CurrY;

                    WHILE @@FETCH_STATUS = 0
                    BEGIN
                        IF (@PrevY <= @Y AND @CurrY > @Y) OR (@PrevY > @Y AND @CurrY <= @Y)
                        BEGIN
                            DECLARE @IntersectX FLOAT = @PrevX + (@Y - @PrevY) * (@CurrX - @PrevX) / (@CurrY - @PrevY);
                            IF @IntersectX > @X
                                SET @CrossCount = @CrossCount + 1;
                        END

                        SET @PrevX = @CurrX;
                        SET @PrevY = @CurrY;
                        FETCH NEXT FROM PointCursor INTO @CurrX, @CurrY;
                    END

                    CLOSE PointCursor;
                    DEALLOCATE PointCursor;

                    -- Return 1 if odd number of crossings, 0 otherwise
                    RETURN CASE WHEN @CrossCount % 2 = 1 THEN 1 ELSE 0 END;
                END;
                GO
            ");

            migrationBuilder.Sql(@"
                -- Function for Winding Number Algorithm
                CREATE FUNCTION dbo.IsPointInPolygon_WindingNumber(
                    @X FLOAT, -- X-coordinate of the point to check
                    @Y FLOAT, -- Y-coordinate of the point to check
                    @Points dbo.PointsTableType READONLY -- Table of points with columns X and Y
                )
                RETURNS BIT
                AS
                BEGIN
                    DECLARE @WindingNumber INT = 0;
                    DECLARE @PrevX FLOAT;
                    DECLARE @PrevY FLOAT;
                    DECLARE @CurrX FLOAT;
                    DECLARE @CurrY FLOAT;

                    -- Close the polygon by connecting the last point to the first
                    DECLARE @PointTable TABLE (PointOrder INT, X FLOAT, Y FLOAT);
                    INSERT INTO @PointTable (PointOrder, X, Y)
                    SELECT ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS PointOrder, X, Y
                    FROM @Points;

                    INSERT INTO @PointTable (PointOrder, X, Y)
                    SELECT 0, X, Y FROM @PointTable WHERE PointOrder = 1;

                    -- Iterate through each edge of the polygon
                    DECLARE PointCursor CURSOR FOR
                        SELECT X, Y FROM @PointTable ORDER BY PointOrder;

                    OPEN PointCursor;
                    FETCH NEXT FROM PointCursor INTO @PrevX, @PrevY;
                    FETCH NEXT FROM PointCursor INTO @CurrX, @CurrY;

                    WHILE @@FETCH_STATUS = 0
                    BEGIN
                        IF @PrevY <= @Y
                        BEGIN
                            IF @CurrY > @Y AND (@CurrX - @PrevX) * (@Y - @PrevY) - (@CurrY - @PrevY) * (@X - @PrevX) > 0
                                SET @WindingNumber = @WindingNumber + 1;
                        END
                        ELSE
                        BEGIN
                            IF @CurrY <= @Y AND (@CurrX - @PrevX) * (@Y - @PrevY) - (@CurrY - @PrevY) * (@X - @PrevX) < 0
                                SET @WindingNumber = @WindingNumber - 1;
                        END

                        SET @PrevX = @CurrX;
                        SET @PrevY = @CurrY;
                        FETCH NEXT FROM PointCursor INTO @CurrX, @CurrY;
                    END

                    CLOSE PointCursor;
                    DEALLOCATE PointCursor;

                    -- Return 1 if winding number is non-zero, 0 otherwise
                    RETURN CASE WHEN @WindingNumber <> 0 THEN 1 ELSE 0 END;
                END;
                GO
            ");

            migrationBuilder.Sql(@"
                CREATE FUNCTION dbo.IsPointInCountry(
                    @X FLOAT, -- X-coordinate of the point to check
                    @Y FLOAT, -- Y-coordinate of the point to check
                    @CountryId INT -- ID of the country to check
                )
                RETURNS BIT
                AS
                BEGIN
                    DECLARE @BorderPoints dbo.PointsTableType;

                    INSERT INTO @BorderPoints (X, Y)
                    SELECT P.X, P.Y
                    FROM CountryBorderPoints CBP
                    INNER JOIN Points P ON CBP.PointId = P.Id
                    WHERE CBP.CountryId = @CountryId;

                    RETURN dbo.IsPointInPolygon_RayCasting(@X, @Y, @BorderPoints);
                END;
                GO
            ");

            migrationBuilder.Sql(@"
                CREATE PROCEDURE dbo.CreateCountryWithBorderPoints
                    @countryName NVARCHAR(MAX),
                    @borderPoints dbo.PointsTableType READONLY
                AS
                BEGIN
                    DECLARE @countryId INT;

                    INSERT INTO Countries (Name)
                    VALUES (@countryName);

                    SET @countryId = SCOPE_IDENTITY();

                    DECLARE @tempBorderPoints TABLE (Id INT);

                    INSERT INTO Points (X, Y)
                    OUTPUT INSERTED.Id INTO @tempBorderPoints
                    SELECT X, Y FROM @borderPoints;

                    INSERT INTO CountryBorderPoints (CountryId, PointId)
                    SELECT @countryId, Id FROM @tempBorderPoints;
                END;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS dbo.GetDistanceByCoordinates;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS dbo.GetDistanceByPointsId;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS dbo.IsPointInPolygon_RayCasting;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS dbo.IsPointInPolygon_WindingNumber;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS dbo.IsPointInCountry;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.CreateCountryWithBorderPoints;");
            migrationBuilder.Sql("DROP TYPE IF EXISTS dbo.PointsTableType;");
        }
    }
}