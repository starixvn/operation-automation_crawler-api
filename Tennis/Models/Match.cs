namespace Tennis.Models;

public class Match(string id)
{
    public DateTime Date { get; set; }

    public DateTime OriginalDate { get; set; }

    public string City { get; set; }

    public string Country { get; set; } = string.Empty;

    public string UtcCity { get; set; }

    public double UtcOffset { get; set; } = 0;

    public string Location { get; set; }

    public string Home { get; set; }

    public string Away { get; set; }

    public string Source { get; set; }

    public string Group { get; set; }

    public string Id { get; } = id;

    public string MatchType { get; set; }

    public override bool Equals(object obj)
    {
        if (obj is Match other)
        {
            return Id == other.Id;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}