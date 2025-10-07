using System.Text.Json.Serialization;

namespace Tennis.Models.UtrSportsModels;


public class UtrPlayer
{
    [JsonPropertyName("playerId")]
    public int PlayerId { get; set; }

    [JsonPropertyName("memberId")]
    public int MemberId { get; set; }

    [JsonPropertyName("firstName")]
    public string FirstName { get; set; }

    [JsonPropertyName("lastName")]
    public string LastName { get; set; }

    [JsonPropertyName("gender")]
    public string Gender { get; set; }

    [JsonPropertyName("isPro")]
    public bool IsPro { get; set; }

    [JsonPropertyName("singlesUtr")]
    public double? SinglesUtr { get; set; }

    [JsonPropertyName("doublesUtr")]
    public double? DoublesUtr { get; set; }

    [JsonPropertyName("nationality")]
    public string Nationality { get; set; }

    [JsonPropertyName("location")]
    public UtrPlayerLocation Location { get; set; }
}

public class UtrPlayerLocation
{
    [JsonPropertyName("latLng")]
    public double[] LatLng { get; set; }

    [JsonPropertyName("display")]
    public string Display { get; set; }
}

public class UtrPlayersResponse
{
    [JsonPropertyName("players")]
    public List<UtrPlayer> Players { get; set; }
}
