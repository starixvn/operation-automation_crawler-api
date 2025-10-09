namespace Tennis.Models;

public class CountryUtcConfig
{
    public Dictionary<string, Dictionary<string, int>> CountryUtcOffsets { get; set; } = new();
}
