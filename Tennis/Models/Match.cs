namespace Tennis.Models;

public record Match
{
    public DateTime Date { get; init; }

    public string Location { get; init; }

    public string Home { get; init; }

    public string Away { get; init; }

    public string Source { get; set; }
}