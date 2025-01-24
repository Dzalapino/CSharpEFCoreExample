namespace SemistructuralProject.Models;

using System.Collections.Generic;

public class Country
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public required List<Point> BorderPoints { get; set; }
}