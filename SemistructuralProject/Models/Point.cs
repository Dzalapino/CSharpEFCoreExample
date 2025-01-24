namespace SemistructuralProject.Models;

using System.Collections.Generic;

public class Point
{
    public int Id { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public List<Country> Countries { get; set; }
}