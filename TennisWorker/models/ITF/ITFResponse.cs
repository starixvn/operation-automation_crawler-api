using System.Text.Json.Serialization;

namespace TennisWorker.Models.ITF;

public class EventDataResponse
{
    [JsonPropertyName("data")]
    public Dictionary<string, ItfEventInfo> Data { get; set; } = new();
}