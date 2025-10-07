namespace Tennis.Models;


public record Player
{
    public string Id { get; set; }

    public string Name { get; init; }

    public DateTime BirthDay { get; init; }

    public string Nationality { get; init; }

    public string Birthplace { get; init; }

    public string Gender { get; init; }

    public string Source { get; init; }
}
