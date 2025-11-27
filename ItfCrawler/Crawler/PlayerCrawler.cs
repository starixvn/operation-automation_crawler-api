using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using ItfCrawler.Models;
using ItfCrawler.Storage;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace ItfCrawler.Crawler;

public class PlayerCrawler(IHttpClientFactory httpClientFactory, ItfStorage storage)
{
    private readonly HttpClient client = httpClientFactory.CreateClient("ItfHttpClient");

    public async Task<List<ItfPlayer>> CrawlAndSavePlayers(List<ItfLeague> leagues)
    {
        var players = await CrawlPlayers(leagues);
        await storage.SavePlayersAsync(players);

        return players;
    }

    private async Task<List<ItfPlayer>> CrawlPlayers(List<ItfLeague> leagues)
    {
        var allPlayers = new List<ItfPlayer>();
        foreach (var league in leagues)
        {
            var acceptancePlayers = await CrawlPlayers(league);
            var players = acceptancePlayers.Select(a => new ItfPlayer
            {
                CountryCode = a.NationalityCode,
                Id = a.PlayerId,
                OriginalUrl = $"https://www.itftennis.com{a.ProfileLink}",
                Name = $"{a.GivenName} {a.FamilyName}",
                UpdatedDate = DateTime.Now
            });
            allPlayers.AddRange(players);
        }

        return allPlayers;
    }

    /// <summary>
    /// Crawl players for a specific tournament
    /// </summary>
    private async Task<List<AcceptancePlayer>> CrawlPlayers(ItfLeague league)
    {
        var (circuitCode, matchTypeCodes) = await GetTournamentAcceptanceFilter(league.OriginalUrl);

        if (string.IsNullOrEmpty(circuitCode))
        {
            return [];
        }

        var playerLists = new List<List<AcceptancePlayer>>();
        foreach (var matchTypeCode in matchTypeCodes)
        {
            var players = await CrawlPlayers(league.Id, circuitCode, matchTypeCode);
            playerLists.Add(players);
        }

        return playerLists.SelectMany(p => p).ToList();
    }

    private async Task<List<AcceptancePlayer>> CrawlPlayers(string key, string circuitCode, string matchType)
    {
        var url =
            $"https://www.itftennis.com/tennis/api/TournamentApi/GetAcceptanceList?tournamentKey={key}&circuitCode={circuitCode}&matchTypeCode={matchType}";
        var response = await client.GetStringAsync(url);
        var acceptanceList = JsonConvert.DeserializeObject<List<Acceptance>>(response);

        var entryClassifications = acceptanceList.SelectMany(a => a.EntryClassifications)
            .ToList();
        var entries = entryClassifications.SelectMany(e => e.Entries)
            .ToList();
        var players = entries.Where(e=>e.Players!=null)
            .SelectMany(e => e.Players).ToList();

        return players.ToList();
    }

    private async Task<(string circuitCode, List<string> matchTypeCodes)> GetTournamentAcceptanceFilter(string originalUrl)
    {
        var url = $"{originalUrl}acceptance-list";
        var response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            return (string.Empty, []);
        }

        var html = await response.Content.ReadAsStringAsync();
        var document = new HtmlDocument();
        document.LoadHtml(html);
        var script = document.DocumentNode.QuerySelector("section.acceptance-list-container script");
        var scriptContent = script.InnerText;

        return ExtractTournamentAcceptanceFilter(scriptContent);
    }

    public (string CircuitCode, List<string> MatchTypeCodes) ExtractTournamentAcceptanceFilter(string text)
    {
        // Support both formats:
        // 1. circuitCode: 'MT' (object property)
        // 2. var circuitCode = 'MT'; (variable assignment)
        var circuitCodePattern = @"(?:circuitCode\s*[:=]\s*['""']([^'""']*)['""'])";

        // Support both formats:
        // 1. filters: [...] (object property)
        // 2. var filters = [...]; (variable assignment)
        var filtersPattern = @"filters\s*[:=]\s*(\[[\s\S]*?\]);?";

        var circuitCode = string.Empty;
        var matchTypeCodes = new List<string>();

        var circuitMatch = Regex.Match(text, circuitCodePattern);
        if (circuitMatch.Success && circuitMatch.Groups.Count > 1)
        {
            circuitCode = circuitMatch.Groups[1].Value;
        }

        var filtersMatch = Regex.Match(text, filtersPattern);
        if (filtersMatch.Success && filtersMatch.Groups.Count > 1)
        {
            var filtersJson = filtersMatch.Groups[1].Value.Trim().TrimEnd(';');

            var filters = JsonConvert.DeserializeObject<List<AcceptanceFilterItem>>(filtersJson);
            matchTypeCodes = filters.Select(f => f.ValueCode).ToList();
        }

        return (circuitCode, matchTypeCodes);
    }
}