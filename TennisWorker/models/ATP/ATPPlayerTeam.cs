namespace TennisWorker.Models.ATP;
public class ATPPlayerTeam
{
    public ATPPlayerDetail Player { get; set; }

    public ATPPlayerDetail Partner { get; set; }
    public List<ATPScore> SetScores { get; set; }
}