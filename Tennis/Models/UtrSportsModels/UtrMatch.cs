using System.Text.Json.Serialization;

namespace Tennis.Models.UtrSportsModels;

public class UtrMatch
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("drawId")]
    public string DrawId { get; set; }

    [JsonPropertyName("roundNumber")]
    public int RoundNumber { get; set; }

    [JsonPropertyName("roundId")]
    public string RoundId { get; set; }

    [JsonPropertyName("matchPosition")]
    public int MatchPosition { get; set; }

    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("scoringTypeId")]
    public int ScoringTypeId { get; set; }

    [JsonPropertyName("position1")]
    public UtrMatchPosition Position1 { get; set; }

    [JsonPropertyName("position2")]
    public UtrMatchPosition Position2 { get; set; }
}

public class UtrMatchPosition
{
    [JsonPropertyName("memberId")]
    public int MemberId { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("typeId")]
    public int TypeId { get; set; }
}

public class UtrMatchesResponse
{
    [JsonPropertyName("matches")]
    public List<UtrMatch> Matches { get; set; }
}

