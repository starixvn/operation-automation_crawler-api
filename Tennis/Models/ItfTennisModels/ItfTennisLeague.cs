using Newtonsoft.Json;

namespace Tennis.Models.ItfTennisModels;

public class ItfTennisLeague
{
    public string tournamentName { get; set; }

    public string dates { get; set; }
    public string location { get; set; }
    public string category { get; set; }
    public string prizeMoney { get; set; }
    public string liveLink { get; set; }
    public string surfaceDesc { get; set; }
    public string surfaceCode { get; set; }
    public string tourStatusCode { get; set; }
    public string tourStatusDesc { get; set; }
    public string indoorOrOutDoor { get; set; }
    public bool isRecognised { get; set; }
    public string recognisedTournamentLink { get; set; }
    public int id { get; set; }
    public string name { get; set; }
    public string promotionalName { get; set; }
    public string hostNation { get; set; }
    public string hostNationCode { get; set; }
    public string venue { get; set; }
    public DateTime startDate { get; set; }
    public DateTime endDate { get; set; }
    public string tournamentKey { get; set; }
    public string tennisCategoryCode { get; set; }
    public string tournamentLink { get; set; }
    public int year { get; set; }
    public string liveStreamingUrl { get; set; }

    public List<ItfTennisMatch> matches { get; set; }
}
