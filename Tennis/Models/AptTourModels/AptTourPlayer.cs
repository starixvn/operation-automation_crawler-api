using Newtonsoft.Json;

namespace Tennis.Models.AptTourModels; public class HandInfo
{
    [JsonProperty("Id")]
    public string Id { get; set; }

    [JsonProperty("Description")]
    public string Description { get; set; }
}

public class SocialLink
{
    [JsonProperty("SocialId")]
    public string SocialId { get; set; }

    [JsonProperty("SocialLink")]
    public string SocialUrl { get; set; }
}

public class PlayerResponse
{
    [JsonProperty("LastName")]
    public string LastName { get; set; }

    [JsonProperty("FirstName")]
    public string FirstName { get; set; }

    [JsonProperty("BirthCity")]
    public string BirthCity { get; set; }

    [JsonProperty("BirthDate")]
    public DateTime BirthDate { get; set; }

    [JsonProperty("Nationality")]
    public string Nationality { get; set; }
}
