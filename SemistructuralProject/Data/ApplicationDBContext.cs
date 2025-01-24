using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SemistructuralProject.Models;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Storage;

namespace SemistructuralProject.Data;

public class ApplicationDBContext() : DbContext
{
    public DbSet<Point> Points { get; set; }
    public DbSet<City> Cities { get; set; }
    public DbSet<Country> Countries { get; set; }


    [DbFunction("GetDistanceByCoordinates", "dbo")]
    public static double GetDistanceByCoordinatesLINQ(double x1, double y1, double x2, double y2)
        => throw new NotSupportedException("This method can only be used in LINQ queries.");
    
    [DbFunction("GetDistanceById", "dbo")]
    public static double GetDistanceByPointsIdLINQ(int id1, int id2)
        => throw new NotSupportedException("This method can only be used in LINQ queries.");

    [DbFunction("IsPointInCountry", "dbo")]
    public static double IsPointInCountryLINQ(double x, double y, int countryId)
        => throw new NotSupportedException("This method can only be used in LINQ queries.");

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(
            @"Server=localhost,1433;Database=ApplicationDB;User Id=sa;Password=P@ssw0rd;Encrypt=False;TrustServerCertificate=True;");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<City>()
            .HasOne(c => c.Country)
            .WithMany()
            .HasForeignKey(c => c.CountryId);

        modelBuilder.Entity<City>()
            .HasOne(c => c.CenterLocation)
            .WithMany()
            .HasForeignKey(c => c.CenterLocationId);

        modelBuilder.Entity<Country>()
            .HasMany(c => c.BorderPoints)
            .WithMany(p => p.Countries)
            .UsingEntity<Dictionary<string, object>>(
                "CountryBorderPoints",
                j => j.HasOne<Point>().WithMany().HasForeignKey("PointId"),
                j => j.HasOne<Country>().WithMany().HasForeignKey("CountryId"));

        modelBuilder.HasDbFunction(() => GetDistanceByCoordinatesLINQ(0, 0, 0, 0))
            .HasName("GetDistanceByCoordinates")
            .HasSchema("dbo");

        modelBuilder.HasDbFunction(() => GetDistanceByPointsIdLINQ(0, 0))
            .HasName("GetDistanceById")
            .HasSchema("dbo");

        modelBuilder.HasDbFunction(() => IsPointInCountryLINQ(0, 0, 0))
            .HasName("IsPointInCountry")
            .HasSchema("dbo");
    }

    public double GetDistanceByCoordinates(float x1, float y1, float x2, float y2)
    {
        var distance = Database
            .SqlQuery<double>($"SELECT dbo.GetDistanceByCoordinates({x1},{y1},{x2},{y2})")
            .ToList();
        return distance.FirstOrDefault();
    }

    public double GetDistanceByPointsId(int id1, int id2)
    {
        var distance = Database
            .SqlQuery<double>($"SELECT dbo.GetDistanceByPointsId({id1},{id2})")
            .ToList();
        return distance.FirstOrDefault();
    }

    public bool IsPointInPolygon_RayCasting(float x, float y, List<Point> polygonPoints)
    {
        var pointsTable = new DataTable();
        pointsTable.Columns.Add("X", typeof(double));
        pointsTable.Columns.Add("Y", typeof(double));

        foreach (var point in polygonPoints)
        {
            pointsTable.Rows.Add(point.X, point.Y);
        }

        var xParam = new SqlParameter("@X", x);
        var yParam = new SqlParameter("@Y", y);
        var pointsParam = new SqlParameter("@Points", pointsTable)
        {
            TypeName = "dbo.PointsTableType"
        };

        var isInside = Database
            .SqlQueryRaw<bool>("SELECT dbo.IsPointInPolygon_RayCasting(@X, @Y, @Points)", xParam, yParam, pointsParam)
            .ToList();
        return isInside.FirstOrDefault();
    }

    public bool IsPointInPolygon_WindingNumber(float x, float y, List<Point> polygonPoints)
    {
        var pointsTable = new DataTable();
        pointsTable.Columns.Add("X", typeof(double));
        pointsTable.Columns.Add("Y", typeof(double));

        foreach (var point in polygonPoints)
        {
            pointsTable.Rows.Add(point.X, point.Y);
        }

        var xParam = new SqlParameter("@X", x);
        var yParam = new SqlParameter("@Y", y);
        var pointsParam = new SqlParameter("@Points", pointsTable)
        {
            TypeName = "dbo.PointsTableType"
        };

        var isInside = Database
            .SqlQueryRaw<bool>("SELECT dbo.IsPointInPolygon_WindingNumber(@X, @Y, @Points)", xParam, yParam, pointsParam)
            .ToList();
        return isInside.FirstOrDefault();
    }

    public bool IsPointInCountry(float x, float y, int countryId)
    {
        var isInside = Database
            .SqlQuery<bool>($"SELECT dbo.IsPointInCountry({x},{y},{countryId})")
            .ToList();
        return isInside.FirstOrDefault();
    }

    public void CreateCountryWithBorderPoints(string countryName, List<Point> borderPoints)
    {
        var borderPointsTable = new DataTable();
        borderPointsTable.Columns.Add("X", typeof(double));
        borderPointsTable.Columns.Add("Y", typeof(double));

        foreach (var point in borderPoints)
        {
            borderPointsTable.Rows.Add(point.X, point.Y);
        }

        var countryNameParam = new SqlParameter("@countryName", countryName);
        var borderPointsParam = new SqlParameter("@borderPoints", borderPointsTable)
        {
            TypeName = "dbo.PointsTableType"
        };

        Database.ExecuteSqlRaw("EXEC dbo.CreateCountryWithBorderPoints @countryName, @borderPoints", countryNameParam, borderPointsParam);
    }

    public void CreateCityWithCenterLocation(string cityName, int countryId, float centerLocationX, float centerLocationY)
    {
        var cityNameParam = new SqlParameter("@cityName", cityName);
        var countryIdParam = new SqlParameter("@countryId", countryId);
        var centerLocationXParam = new SqlParameter("@centerLocationX", centerLocationX);
        var centerLocationYParam = new SqlParameter("@centerLocationY", centerLocationY);

        Database.ExecuteSqlRaw(
            "EXEC dbo.CreateCityWithCenterLocation @cityName, @countryId, @centerLocationX, @centerLocationY",
            cityNameParam,
            countryIdParam,
            centerLocationXParam,
            centerLocationYParam
        );
    }
}