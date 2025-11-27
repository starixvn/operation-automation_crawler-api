using ItfCrawler.Models;
using ItfCrawler.Storage;
using Newtonsoft.Json;

namespace ItfCrawler.Crawler;

public class LeagueCrawler(IHttpClientFactory httpClientFactory, ItfStorage storage)
{
    private readonly HttpClient client = httpClientFactory.CreateClient("ItfHttpClient");

    public async Task<List<ItfLeague>> CrawlAndSaveLeagues()
    {
        var tours = await CrawlTours();
        var itfLeagues = GetItfLeagueList(tours);
        await storage.SaveLeaguesAsync(itfLeagues);
        return itfLeagues;
    }

    private static List<ItfLeague> GetItfLeagueList(List<Tournament> tours)
    {
        var itfLeagues = tours.Select(t => new ItfLeague
        {
            Id = t.TournamentKey,
            Name = t.TournamentName,
            OriginalUrl = $"https://www.itftennis.com{t.TournamentLink}",
            UpdatedDate = DateTime.Now
        }).ToList();

        return itfLeagues;
    }

    private async Task<List<Tournament>> CrawlTours()
    {
        var startDate = DateTime.Today.AddDays(-1);
        var endDate = DateTime.Today.AddDays(15);

        var dateFrom = startDate.ToString("yyyy-MM-dd");
        var dateTo = endDate.ToString("yyyy-MM-dd");

        // Fetch men's tournaments
        var menUrl = $"https://www.itftennis.com/tennis/api/TournamentApi/GetCalendar?circuitCode=MT&searchString=&skip=0&take=100&nationCodes=&zoneCodes=&dateFrom={dateFrom}&dateTo={dateTo}&indoorOutdoor=&categories=&isOrderAscending=true&orderField=startDate&surfaceCodes=";
        var menResponse = await client.GetStringAsync(menUrl);
        var menData = JsonConvert.DeserializeObject<TournamentCalendarResponse>(menResponse);
        var menTours = menData?.Items ?? new List<Tournament>();

        // Fetch women's tournaments
        var womenUrl = $"https://www.itftennis.com/tennis/api/TournamentApi/GetCalendar?circuitCode=WT&searchString=&skip=0&take=100&nationCodes=&zoneCodes=&dateFrom={dateFrom}&dateTo={dateTo}&indoorOutdoor=&categories=&isOrderAscending=true&orderField=startDate&surfaceCodes=";
        var womenResponse = await client.GetStringAsync(womenUrl); 
        var womenData = JsonConvert.DeserializeObject<TournamentCalendarResponse>(womenResponse);
        var womenTours = womenData?.Items ?? new List<Tournament>();

        // Combine and sort tournaments
        var allTours = menTours.Concat(womenTours).OrderBy(t => t.StartDate).ToList();

        return allTours;
    }
}