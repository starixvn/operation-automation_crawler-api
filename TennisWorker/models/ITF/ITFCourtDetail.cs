using System.Text.Json.Serialization;

namespace TennisWorker.Models.ITF;

public class ITFCourtDetail
{

    [JsonPropertyName("actualEndDate")]
    public DateTime ActualEndDate { get; set; }

    [JsonPropertyName("actualStartDate")]
    public DateTime ActualStartDate { get; set; }
}