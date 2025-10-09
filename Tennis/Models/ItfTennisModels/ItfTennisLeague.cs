using Newtonsoft.Json;

namespace Tennis.Models.ItfTennisModels;
public class TournamentResponse
{
    [JsonProperty("url")]
    public string Url { get; set; } = string.Empty;

    [JsonProperty("items")]
    public List<TournamentItem> Items { get; set; } = [];

    [JsonProperty("totalItems")]
    public int TotalItems { get; set; }
}

public class TournamentItem
{
    [JsonProperty("tournamentName")]
    public string TournamentName { get; set; } = string.Empty;

    [JsonProperty("tournamentKey")]
    public string TournamentKey { get; set; } = string.Empty;

    [JsonProperty("dates")]
    public string Dates { get; set; } = string.Empty;

    [JsonProperty("location")]
    public string Location { get; set; } = string.Empty;

    [JsonProperty("hostNation")]
    public string HostNation { get; set; } = string.Empty;

    [JsonProperty("tournamentLink")]
    public string TournamentLink { get; set; } = string.Empty;

    [JsonProperty("tourStatusDesc")]
    public string TourStatusDesc { get; set; } = string.Empty;
}

public class OrderOfPlayDay
{
    [JsonProperty("orderOfPlayDayId")]
    public long OrderOfPlayDayId { get; set; }

    [JsonProperty("playDate")]
    public DateTime PlayDate { get; set; }

    [JsonProperty("playDateString")]
    public string PlayDateString { get; set; } = string.Empty;
}
