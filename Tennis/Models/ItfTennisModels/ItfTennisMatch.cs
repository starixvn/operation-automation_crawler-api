using Newtonsoft.Json;

namespace Tennis.Models.ItfTennisModels;
public class ItfResponse
{
    [JsonProperty("courtName")]
    public string CourtName { get; set; } = string.Empty;

    [JsonProperty("matches")]
    public List<ItfMatch> Matches { get; set; } = [];
}

public class ItfMatch
{
    [JsonProperty("schedule")]
    public string Schedule { get; set; } = string.Empty;

    [JsonProperty("teams")]
    public List<ItfTeam> Teams { get; set; } = [];
}

public class ItfTeam
{
    [JsonProperty("players")]
    public List<ItfPlayer> Players { get; set; } = [];
}

public class ItfPlayer
{
    [JsonProperty("playerId")]
    public long PlayerId { get; set; }

    [JsonProperty("nationality")]
    public string Nationality { get; set; } = string.Empty;

    [JsonProperty("givenName")]
    public string GivenName { get; set; } = string.Empty;

    [JsonProperty("familyName")]
    public string FamilyName { get; set; } = string.Empty;

    [JsonProperty("profileLink")]
    public string ProfileLink { get; set; } = string.Empty;
}
