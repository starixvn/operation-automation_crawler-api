using Newtonsoft.Json;

namespace Tennis.Models.AptTourModels; public class TournamentResponse
{
    [JsonProperty("TournamentDates")]
    public List<TournamentDate> TournamentDates { get; set; }
}

public class TournamentDate
{
    [JsonProperty("DisplayDate")]
    public string DisplayDate { get; set; }

    [JsonProperty("Tournaments")]
    public List<Tournament> Tournaments { get; set; }
}

public class Tournament
{
    [JsonProperty("Id")]
    public string Id { get; set; }

    [JsonProperty("Name")]
    public string Name { get; set; }

    [JsonProperty("FormattedDate")]
    public string FormattedDate { get; set; }
}