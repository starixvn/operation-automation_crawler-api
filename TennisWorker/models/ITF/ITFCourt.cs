using System.Text.Json;
using System.Text.Json.Serialization;

namespace TennisWorker.Models.ITF;

public class ITFCourt
{
    [JsonPropertyName("_admin_name")]
    public string AdminName { get; set; }

    [JsonPropertyName("_name")]
    public string Name { get; set; }

    public Dictionary<string, JsonElement> AdditionalData { get; set; } = new();
}