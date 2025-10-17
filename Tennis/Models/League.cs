namespace Tennis.Models;

public class League
{
    public string Source { get; init; }

    public string Id { get; init; }

    public string Name { get; init; }

    public string Location { get; init; }

    public string Country { get; set; }

    public string Date { get; init; }

    public DateTime DateFrom { get; init; }

    public DateTime DateTo { get; init; }

    public string Group { get; init; }

    public string ScheduleUrl { get; set; } = string.Empty;

    public List<Match> Matches { get; set; } = new();

    public List<Player> Players { get; set; } = new();

    public virtual bool Equals(League other)
    {
        return other != null && Source == other.Source;
    }

    public override int GetHashCode()
    {
        return Source.GetHashCode();
    }
}