namespace Tennis.Models;


public class Player(string id)
{
    public string Id { get; } = id;

    public string Name { get; set; }

    public DateTime BirthDay { get; set; }

    public string Nationality { get; set; }

    public string Birthplace { get; set; }

    public string Gender { get; set; }

    public string Source { get; set; }

    public override bool Equals(object obj)
    {
        if (obj is Player other)
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
