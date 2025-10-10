namespace Tennis.Models;

public record League
{
    public string Source { get; init; }

    public string Id { get; init; }

    public string Name { get; init; }

    public string Location { get; init; }

    public string Country { get; set; }

    public string Date { get; init; }

    public DateTime DateFrom { get; init; }

    public DateTime DateTo { get; init; }

    public virtual bool Equals(League other)
    {
        return other != null && Source == other.Source;
    }

    public override int GetHashCode()
    {
        return Source.GetHashCode();
    }
}