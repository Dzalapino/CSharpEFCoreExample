using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SemistructuralProject.Migrations
{
    /// <inheritdoc />
    public partial class AddTriggersAndProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE TRIGGER trg_PreventPointDeletion
                ON Points
                INSTEAD OF DELETE
                AS
                BEGIN
                    IF EXISTS (SELECT 1 FROM Cities WHERE CenterLocationId IN (SELECT Id FROM DELETED))
                    OR EXISTS (SELECT 1 FROM CountryBorderPoints WHERE PointId IN (SELECT Id FROM DELETED))
                    BEGIN
                        RAISERROR ('Cannot delete point because it is used in cities or country border points tables.', 16, 1);
                        ROLLBACK TRANSACTION;
                    END
                    ELSE
                    BEGIN
                        DELETE FROM Points WHERE Id IN (SELECT Id FROM DELETED);
                    END
                END;
            ");

            migrationBuilder.Sql(@"
                CREATE TRIGGER trg_CheckCityWithinCountry
                ON Cities
                INSTEAD OF INSERT
                AS
                BEGIN
                    DECLARE @centerX FLOAT, @centerY FLOAT, @countryId INT;

                    SELECT @centerX = p.X, @centerY = p.Y, @countryId = i.CountryId
                    FROM INSERTED i
                    INNER JOIN Points p ON i.CenterLocationId = p.Id;

                    IF dbo.IsPointInCountry(@centerX, @centerY, @countryId) = 0
                    BEGIN
                        RAISERROR ('Cannot insert city because its center point is outside the borders of the specified country.', 16, 1);
                        ROLLBACK TRANSACTION;
                    END
                    ELSE
                    BEGIN
                        INSERT INTO Cities (Name, CountryId, CenterLocationId)
                        SELECT Name, CountryId, CenterLocationId
                        FROM INSERTED;
                    END
                END;
            ");

            migrationBuilder.Sql(@"
                CREATE PROCEDURE dbo.CreateCityWithCenterLocation
                    @cityName NVARCHAR(MAX),
                    @countryId INT,
                    @centerX FLOAT,
                    @centerY FLOAT
                AS
                BEGIN
                    DECLARE @centerLocationId INT;
                    DECLARE @isInside BIT;

                    -- Check if the center point is within the country borders
                    SET @isInside = dbo.IsPointInCountry(@centerX, @centerY, @countryId);

                    IF @isInside = 1
                    BEGIN
                        INSERT INTO Points (X, Y) VALUES (@centerX, @centerY);

                        SET @centerLocationId = SCOPE_IDENTITY();

                        INSERT INTO Cities (Name, CountryId, CenterLocationId) VALUES (@cityName, @countryId, @centerLocationId);
                    END
                    ELSE
                    BEGIN
                        RAISERROR ('Cannot create city because its center point is outside the borders of the specified country.', 16, 1);
                    END
                END;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER trg_PreventPointDeletion");
            migrationBuilder.Sql("DROP TRIGGER trg_CheckCityWithinCountry");
            migrationBuilder.Sql("DROP PROCEDURE dbo.CreateCityWithCenterLocation");
        }
    }
}
