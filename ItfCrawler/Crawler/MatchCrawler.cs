using ItfCrawler.Models;
using ItfCrawler.Storage;
using Newtonsoft.Json;

namespace ItfCrawler.Crawler;

public class MatchCrawler(IHttpClientFactory httpClientFactory, ItfStorage storage)
{
    private readonly HttpClient client = httpClientFactory.CreateClient("ItfHttpClient");

    public async Task<List<ItfMatch>> CrawlAndSaveMatches(List<string> keys)
    {
        var scheduleMatches = await CrawlScheduleMatches(keys);
        var itfMatches = ToItfMatches(scheduleMatches);

        await storage.SaveMatchesAsync(itfMatches);

        return itfMatches;
    }

    private List<ItfMatch> ToItfMatches(List<ScheduleMatch> matches)
    {
        return matches.Select(m =>
        {
            var homePlayers = m.HomePlayers.Where(p => p != null).ToList();
            var awayPlayers = m.AwayPlayers.Where(p => p != null).ToList();
            var homeId1 = homePlayers.Count > 0 ? homePlayers[0].PlayerId : 0;
            var homeId2 = homePlayers.Count > 1 ? homePlayers[1].PlayerId : 0;
            var awayId1 = awayPlayers.Count > 0 ? awayPlayers[0].PlayerId : 0;
            var awayId2 = awayPlayers.Count > 1 ? awayPlayers[1].PlayerId : 0;

            var itfMatch = new ItfMatch
            {
                Id = m.MatchId,
                HomeId1 = homeId1,
                HomeId2 = homeId2,
                AwayId1 = awayId1,
                AwayId2 = awayId2,
                UpdatedDate = DateTime.Now
            };

            return itfMatch;
        }).ToList();
    }

    private async Task<List<ScheduleMatch>> CrawlScheduleMatches(List<string> keys)
    {
        var allMatches = new List<ScheduleMatch>();

        foreach (var key in keys)
        {
            var matches = await CrawlScheduleMatches(key);
            if (matches != null && matches.Any())
            {
                allMatches.AddRange(matches);
            }
        }

        return allMatches;
    }

    private async Task<List<ScheduleMatch>> CrawlScheduleMatches(string key)
    {
        var dayUrl = $"https://www.itftennis.com/tennis/api/TournamentApi/GetOrderOfPlayDays?tournamentKey={key}";
        var daysResponse = await client.GetStringAsync(dayUrl);
        var daysData = JsonConvert.DeserializeObject<List<ScheduleOrderOfPlayDay>>(daysResponse);

        // Check if array is empty
        if (daysData == null || !daysData.Any())
        {
            return [];
        }

        // Get first item
        var orderOfPlay = daysData[0];

        // Get matches data
        var url = $"https://www.itftennis.com/tennis/api/TournamentApi/GetOrderOfPlay?orderOfPlayDayId={orderOfPlay.OrderOfPlayDayId}";
        var matchesResponse = await client.GetStringAsync(url);
        var matchJson = JsonConvert.DeserializeObject<List<ScheduleMatchGroup>>(matchesResponse);
        var matches = matchJson?.SelectMany(item => item.Matches).ToList() ?? new List<ScheduleMatch>();

        // Restructure players and add tournament info
        foreach (var match in matches)
        {
            if (match.Teams is { Count: >= 2 })
            {
                match.HomePlayers = match.Teams[0].Players ?? [];
                match.AwayPlayers = match.Teams[1].Players ?? [];
            }
            match.TournamentKey = key;
            match.PlayDate = orderOfPlay.PlayDate;
        }

        return matches;
    }
}