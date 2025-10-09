namespace Tennis.Models;

public class Match
{
    public DateTime Date { get; set; }

    public DateTime OriginalDate { get; set; }

    public string City { get; set; }

    public string Country { get; set; } = string.Empty;

    public string UtcCity { get; set; }

    public int UtcOffset { get; set; } = 0;

    public string Location { get; set; }

    public string Home { get; set; }

    public string Away { get; set; }

    public string Source { get; set; }
}