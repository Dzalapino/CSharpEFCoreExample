using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SemistructuralProject.Data;
using SemistructuralProject.Models;

class Program
{
    static void Main(string[] args)
    {
        using (var context = new ApplicationDBContext())
        {
            context.Database.EnsureCreated();

            // // Create points and country and place it in the database.
            // // Uncomment if you didn't add the data to the database.
            // var borderPoints = new List<Point>
            // {
            //     new Point { X = -1, Y = -1 },
            //     new Point { X = -1, Y = 1 },
            //     new Point { X = 1, Y = 1 },
            //     new Point { X = 1, Y = -1 }
            // };
            // context.CreateCountryWithBorderPoints("Poland", borderPoints);

            // Show countries and points after adding them to the database.
            Console.WriteLine("Countries and their border points:");
            foreach (var country in context.Countries.Include(c => c.BorderPoints))
            {
                Console.WriteLine(country.Name);
                foreach (var point in country.BorderPoints)
                {
                    Console.WriteLine($"X: {point.X}, Y: {point.Y}");
                }
                Console.WriteLine();
            }

            // Check functions
            Console.WriteLine(
                "distance between points (0,0) and (3,4) is: " + 
                context.GetDistanceByCoordinates(0, 0, 3, 4).ToString()
            );

            var point1 = context.Points.Where(p => p.Id == 1).FirstOrDefault();
            var point2 = context.Points.Where(p => p.Id == 2).FirstOrDefault();
            Console.WriteLine(
                "Point with id 1 is: " + point1!.X.ToString() + ", " + point1.Y.ToString()
            );
            Console.WriteLine(
                "Point with id 2 is: " + point2!.X.ToString() + ", " + point2.Y.ToString()
            );

            Console.WriteLine(
                "distance between points with id 1 and 2 is: " + context.GetDistanceByPointsId(1, 2).ToString()
            );

            Console.WriteLine(
                "Is point (0, 0) in {(-1, -1), (0, 1), (1, -1)} polygon? (Using Ray Casting Alghoritm) " + 
                (
                    context.IsPointInPolygon_RayCasting(
                        0f, 0f, 
                        new List<Point>
                        {
                            new Point { X = -1, Y = -1 },
                            new Point { X = 0, Y = 1 },
                            new Point { X = 1, Y = -1 }
                        }
                    ) ? "Yes" : "No"
                )
            );

            Console.WriteLine(
                "Is point (0, 0) in {(-1, -1), (0, 1), (1, -1)} polygon? (Using Winding Number Alghoritm) " + 
                (
                    context.IsPointInPolygon_WindingNumber(
                        0f, 0f, 
                        new List<Point>
                        {
                            new Point { X = -1, Y = -1 },
                            new Point { X = 0, Y = 1 },
                            new Point { X = 1, Y = -1 }
                        }
                    ) ? "Yes" : "No"
                )
            );

            Console.WriteLine(
                "Is point (0,0) in Poland? " + (context.IsPointInCountry(0, 0, 1) ? "Yes" : "No" )
            );

            Console.WriteLine(
                "Is point (10.5, 10.0) in Poland? " + (context.IsPointInCountry(10.5f, 10.0f, 1) ? "Yes" : "No" )
            );

            try
            {
                Console.WriteLine(
                    "\nNow we will try to add new city to our country in the database." +
                    "\nIt will be done using C# function that uses SQL procedure"
                );
                // // Uncomment if you didn't add the city to the database yet. 
                // context.CreateCityWithCenterLocation(
                //     "Warsaw",
                //     context.Countries.Where(c => c.Name == "Poland").FirstOrDefault()!.Id,
                //     0.2f,
                //     0.0f
                // );

                Console.WriteLine("City added successfully, printing cities in the DB:");
                foreach (var city in context.Cities.Include(c => c.CenterLocation))
                {
                    Console.WriteLine(city.Name);
                    Console.WriteLine($"Center location: X: {city.CenterLocation.X}, Y: {city.CenterLocation.Y}");
                    Console.WriteLine();
                }

                Console.WriteLine(
                    "Now we will try to add Berlin to Poland using SQL procedure." +
                    "\nIt will fail because it isn't within borders of the country.");
                context.CreateCityWithCenterLocation(
                    "Berlin",
                    context.Countries.Where(c => c.Name == "Poland").FirstOrDefault()!.Id,
                    -1.5f,
                    0.2f
                );
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}