namespace TennisWorker.Models;

public class Match
{
    public string Id { get; set; }
    public string Home { get; set; }

    public string Away { get; set; }
    public DateTime KickOffTime { get; set; }

    public DateTime CreatedDate { get; set; }

    public string LeagueId { get; set; }


}