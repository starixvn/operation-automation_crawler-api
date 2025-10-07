namespace Tennis.Models;

public record Match
{
    public DateTime Date { get; init; }

    public string Location { get; init; }

    public List<Player> Home { get; init; }

    public List<Player> Away { get; init; }

    public string Source { get; set; }
}