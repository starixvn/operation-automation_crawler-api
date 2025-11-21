namespace TennisWorker.Models;

public class LiveScore
{
    public string Id { get; set; }

    public string MatchId { get; set; }

    public string HomeValue { get; set; }

    public string AwayValue { get; set; }

    public DateTime CreatedDate { get; set; }

    public string Source { get; set; }

    public int SetNumber { get; set; }
}