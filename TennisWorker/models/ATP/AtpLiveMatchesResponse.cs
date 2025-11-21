namespace TennisWorker.Models.ATP;

public class AtpLiveMatchesResponse
{
    public AtpResponseData Data { get; set; }
}

public class AtpResponseData
{
    public List<AtpEvent> LiveMatchesTournamentsOrdered { get; set; } = [];

    public string EventLevel { get; set; }
}

