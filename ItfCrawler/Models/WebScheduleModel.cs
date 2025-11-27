using System.Text.Json.Serialization;

namespace ItfCrawler.Models;

public class ScheduleOrderOfPlayDay
{
    [JsonPropertyName("orderOfPlayDayId")]
    public int OrderOfPlayDayId { get; set; } = 0;

    [JsonPropertyName("playDate")]
    public DateTime PlayDate { get; set; }
}

public class ScheduleMatchGroup
{
    [JsonPropertyName("matches")]
    public List<ScheduleMatch> Matches { get; set; } = new();
}

public class ScheduleMatch
{
    [JsonPropertyName("matchId")]
    public int MatchId { get; set; }

    [JsonPropertyName("teams")]
    public List<ScheduleTeam> Teams { get; set; }

    [JsonPropertyName("homePlayers")]
    public List<SchedulePlayer> HomePlayers { get; set; } = new();

    [JsonPropertyName("awayPlayers")]
    public List<SchedulePlayer> AwayPlayers { get; set; } = new();

    [JsonPropertyName("TournamentKey")]
    public string TournamentKey { get; set; }

    [JsonPropertyName("playDate")]
    public DateTime? PlayDate { get; set; }

    [JsonPropertyName("matchStatus")]
    public string MatchStatus { get; set; }
}

public class ScheduleTeam
{
    [JsonPropertyName("players")]
    public List<SchedulePlayer> Players { get; set; } = new();
}

public class SchedulePlayer
{
    [JsonPropertyName("playerId")]
    public int PlayerId { get; set; } = 0;

    [JsonPropertyName("playerName")]
    public string PlayerName { get; set; }

    [JsonPropertyName("familyName")]
    public string FamilyName { get; set; }

    [JsonPropertyName("givenName")]
    public string GivenName { get; set; }

    [JsonPropertyName("nationalityCode")]
    public string NationalityCode { get; set; }
}
