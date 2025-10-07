using System.Text.Json.Serialization;

namespace Tennis.Models.UtrSportsModels;

public class UtrLeagueResponse
{
    [JsonPropertyName("hits")]
    public List<UtrLeagueHit> Hits { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }
}

public class UtrLeagueHit
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("source")]
    public UtrLeagueSource Source { get; set; }
}

public class UtrLeagueSource
{
    [JsonPropertyName("club")]
    public UtrLeagueClub Club { get; set; }

    [JsonPropertyName("eventSchedule")]
    public UtrLeagueSchedule EventSchedule { get; set; }
}

public class UtrLeagueClub
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
}

public class UtrLeagueSchedule
{
    [JsonPropertyName("eventDatesLong")]
    public string EventDatesLong { get; set; }

    [JsonPropertyName("eventStartUtc")]
    public DateTime EventStartUtc { get; set; }

    [JsonPropertyName("eventEndUtc")]
    public DateTime EventEndUtc { get; set; }
}
