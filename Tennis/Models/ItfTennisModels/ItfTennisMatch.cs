namespace Tennis.Models.ItfTennisModels;

public class ItfTennisPlayer
{
    public string familyName { get; set; }
    public string givenName { get; set; }
    public bool hiddenPlayer { get; set; }
    public string nationality { get; set; }
    public int playerId { get; set; }
    public string profileLink { get; set; }
}


public class ItfTennisMatch
{
    public string tournamentKey { get; set; }
    public string ageCategory { get; set; }
    public List<ItfTennisPlayer> awayPlayers { get; set; }
    public string eventClassificationCode { get; set; }
    public string eventClassificationDesc { get; set; }
    public string eventDesc { get; set; }
    public string h2hLink { get; set; }
    public List<ItfTennisPlayer> homePlayers { get; set; }
    public string liveScoresLink { get; set; }
    public string matchDescription { get; set; }
    public int matchId { get; set; }
    public int matchNumber { get; set; }
    public string playStatusCode { get; set; }
    public string playStatusDesc { get; set; }
    public string roundCode { get; set; }
    public string roundGroupDesc { get; set; }
    public string schedule { get; set; }

    public DateTime playDate { get; set; }
}