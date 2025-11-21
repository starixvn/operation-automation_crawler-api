namespace TennisWorker.Models.ATP;

public class AtpEvent
{
    public string EventTitle { get; set; }

    public int EventId { get; set; }
    public string EventLevel { get; set; }
    public bool IsLive { get; set; }
    public List<ATPMatch> LiveMatches { get; set; } = [];
}

