using System.Text.Json.Serialization;

namespace TennisWorker.Models.ITF;

public class ItfEventInfo
{
    [JsonPropertyName("_name")]
    public string Name { get; set; }

    [JsonPropertyName("_admin_name")]
    public string AdminName { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("courts")]
    public Dictionary<string, List<ITFCourt>> Courts { get; set; } = new();
}