using System.Text.Json.Serialization;

namespace ItfCrawler.Models;

public class TournamentCalendarResponse
{
    [JsonPropertyName("items")]
    public List<Tournament> Items { get; set; } = new();
}

public class Tournament
{
    [JsonPropertyName("tournamentKey")]
    public string TournamentKey { get; set; } = string.Empty;

    [JsonPropertyName("tournamentLink")]
    public string TournamentLink { get; set; } = string.Empty;

    [JsonPropertyName("startDate")]
    public DateTime StartDate { get; set; }

    [JsonPropertyName("endDate")]
    public DateTime EndDate { get; set; }

    [JsonPropertyName("tournamentName")]
    public string TournamentName { get; set; }

    [JsonPropertyName("hostNation")]
    public string HostNation { get; set; }
}
