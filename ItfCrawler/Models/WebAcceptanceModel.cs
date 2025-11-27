using Newtonsoft.Json;

namespace ItfCrawler.Models;

public class Acceptance
{
    [JsonProperty("playerTypeDesc")]
    public string PlayerTypeDesc { get; set; }

    [JsonProperty("entryClassifications")]
    public List<AcceptanceEntryClassification> EntryClassifications { get; set; }
}

public class AcceptanceEntryClassification
{
    [JsonProperty("entries")]
    public List<AcceptanceEntry> Entries { get; set; }
}

public class AcceptanceEntry
{
    [JsonProperty("players")]
    public List<AcceptancePlayer> Players { get; set; }
}

public class AcceptancePlayer
{
    [JsonProperty("nationalityCode")]
    public string NationalityCode { get; set; }

    [JsonProperty("playerId")]
    public long PlayerId { get; set; } 

    [JsonProperty("givenName")]
    public string GivenName { get; set; }

    [JsonProperty("familyName")]
    public string FamilyName { get; set; }

    [JsonProperty("profileLink")]
    public string ProfileLink { get; set; }
}

public class AcceptanceFilterItem
{
    [JsonProperty("valueCode")]
    public string ValueCode { get; set; }
}