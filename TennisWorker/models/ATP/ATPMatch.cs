namespace TennisWorker.Models.ATP;

public class ATPMatch
{
    public string MatchId { get; set; }

    public int LeagueId { get; set; }

    public string Source { get; set; }

    public string RoundName { get; set; }
    public string MatchStatus { get; set; }
    public DateTime LastUpdated { get; set; }
    public ATPPlayerTeam PlayerTeam { get; set; }
    public ATPPlayerTeam OpponentTeam { get; set; }
}
