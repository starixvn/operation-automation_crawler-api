namespace Tennis.Models;

public record League
{
    public string Source { get; init; }

    public string Id { get; init; }

    public string Name { get; init; }

    public string Date { get; init; }
}