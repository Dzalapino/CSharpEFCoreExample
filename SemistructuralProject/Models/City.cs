namespace SemistructuralProject.Models;

public class City
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CountryId { get; set; }
    public required Country Country { get; set; }
    public int CenterLocationId { get; set; }
    public required Point CenterLocation { get; set; }
}