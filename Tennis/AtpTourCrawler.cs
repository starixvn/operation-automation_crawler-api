using HtmlAgilityPack;
using Fizzler.Systems.HtmlAgilityPack;
using Newtonsoft.Json;
using Tennis.Models;
using Tennis.Models.AptTourModels;
using Microsoft.Extensions.Caching.Memory;
using Tennis.Extensions;

namespace Tennis;

public class AtpTourCrawler(HttpClient client, IMemoryCache memoryCache)
{
    private const string ActiveFlagsCacheKey = "ActiveFlags";
    private readonly IMemoryCache memoryCache = memoryCache;

    public async Task<List<League>> GetLeagues()
    {
        const string url = "https://www.atptour.com/en/-/tournaments/calendar/tour";
        var response = await client.GetStringAsync(url);
        var tournamentResponse = JsonConvert.DeserializeObject<TournamentResponse>(response);

        var leagues = new List<League>();
        foreach (var tournamentDate in tournamentResponse.TournamentDates)
        {
            foreach (var tournament in tournamentDate.Tournaments)
            {
                var (dateFrom, dateTo) = DateTimeExtensions.ParseAtpTourDateRange(tournament.FormattedDate);
                leagues.Add(new League
                {
                    Source = $"https://www.atptour.com/en/scores/current/shanghai/{tournament.Id}/daily-schedule",
                    Name = tournament.Name,
                    Id = tournament.Id,
                    Date = tournament.FormattedDate,
                    DateFrom = dateFrom,
                    DateTo = dateTo,
                    Location = tournament.Location
                });
            }
        }

        return leagues;
    }


    public async Task<List<Match>> GetMatches(string leagueUrl)
    {
        var days = await GetDaysOfLeague(leagueUrl);
        if (days.Count == 0)
        {
            return await GetMatches(leagueUrl, string.Empty);
        }
        else
        {
            var result = new List<Match>();
            foreach (var day in days)
            {
                var matches = await GetMatches(leagueUrl, day);
                result.AddRange(matches);
            }

            return result;
        }
    }

    private async Task<List<Match>> GetMatches(string leagueUrl, string day)
    {
        if (!string.IsNullOrEmpty(day))
        {
            leagueUrl += $"?day={day}";
        }

        var response = await client.GetStringAsync(leagueUrl);
        var doc = new HtmlDocument();
        doc.LoadHtml(response);

        var schedules = new List<Match>();
        var matchElements = doc.DocumentNode.QuerySelectorAll(".content-group .schedule");


        foreach (var matchElement in matchElements)
        {
            var dateStr = matchElement.GetAttributeValue("data-matchdate", "");
            var timeStr = matchElement.GetAttributeValue("data-datetime", "");
            var dateTimeStr = string.IsNullOrEmpty(timeStr) ? dateStr : timeStr;
            var matchDate = DateTime.Parse(dateTimeStr);

            // Get location
            var locationElement = matchElement.QuerySelector(".schedule-location-timestamp strong");
            var location = locationElement?.InnerText.Trim() ?? string.Empty;

            if (locationElement == null)
            {
                continue;
            }

            // Get home and away players

            var homeElement = matchElement.QuerySelector(".schedule-players .player");
            var awayElement = matchElement.QuerySelector(".schedule-players .opponent");

            var homePlayers = await GetPlayersFromElement(homeElement);
            var awayPlayers = await GetPlayersFromElement(awayElement);

            var matchId = $"{dateTimeStr}-{string.Join('-', homePlayers.Select(p => p.Id))}"
                          + $"-{string.Join('-', awayPlayers.Select(p => p.Id))}";
            schedules.Add(new Match(matchId)
            {
                OriginalDate = matchDate.Date,
                UtcOffset = 0,
                City = "ATP Tour",
                Country = "ATP Tour",
                Date = matchDate.AddHours(8),
                Location = location,
                Home = string.Join(" / ", homePlayers.Select(p => p.Name)),
                Away = string.Join(" / ", awayPlayers.Select(p => p.Name)),
                Source = leagueUrl
            });
        }

        return schedules;
    }

    private async Task<List<string>> GetDaysOfLeague(string key)
    {
        var url = $"https://www.atptour.com/en/scores/current/leaguename/{key}/daily-schedule";
        var response = await client.GetStringAsync(url);
        var doc = new HtmlDocument();
        doc.LoadHtml(response);

        var dropdown = doc.DocumentNode.QuerySelector("#matchDate-filter");
        var options = dropdown?.SelectNodes(".//option");
        var days = options?.Select(option => option.GetAttributeValue("value", "")).ToList();

        return days ?? [];
    }

    public async Task<Player> GetPlayer(string url, string country)
    {
        // Extract player ID from URL
        var id = url.Replace("/overview", "").TrimEnd('/').Split('/').Last().Split('/')[0];

        // Try to get player from cache
        if (memoryCache.TryGetValue(id, out Player cachedPlayer))
        {
            return cachedPlayer;
        }

        var apiUrl = $"https://www.atptour.com/en/-/www/players/hero/{id}?v=1";
        var response = await client.GetStringAsync(apiUrl);

        var playerResponse = JsonConvert.DeserializeObject<PlayerResponse>(response);
        var activeFlags = await GetActiveFlags();
        var nation = activeFlags.Contains(country) ? $" ({playerResponse.NatlId})" : string.Empty;
        var player = new Player
        {
            Id = id,
            Name = $"{playerResponse.FirstName} {playerResponse.LastName}{nation}",
            BirthDay = playerResponse.BirthDate,
            Nationality = playerResponse.Nationality,
            Birthplace = playerResponse.BirthCity
        };

        // Cache the player data for 30 days
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromDays(30));

        memoryCache.Set(id, player, cacheOptions);

        return player;
    }

    private async Task<List<Player>> GetPlayersFromElement(HtmlNode element)
    {
        var players = new List<Player>();
        var playerLinks = element.QuerySelectorAll(".name a").ToList();
        var flagLinks = element.QuerySelectorAll(".atp-flag use").ToList();

        for (var i = 0; i < playerLinks.Count; i++)
        {
            var flagLink = flagLinks[i];
            var flag = flagLink.GetAttributeValue("href", "/assets/atptour/assets/flags.svg#flag-invalid");
            var country = flag.Split('#').Last();
            var link = playerLinks[i];
            var playerUrl = "https://www.atptour.com" + link.GetAttributeValue("href", "");
            var player = await GetPlayer(playerUrl, country);
            players.Add(player);
        }

        return players;
    }

    public async Task<List<string>> GetActiveFlags()
    {
        if (memoryCache.TryGetValue(ActiveFlagsCacheKey, out List<string> cachedFlags))
        {
            return cachedFlags;
        }

        const string url = "https://www.atptour.com/assets/atptour/assets/flags.svg";
        var response = await client.GetStringAsync(url);

        var doc = new HtmlDocument();
        doc.LoadHtml(response);

        var symbolNodes = doc.DocumentNode.SelectNodes("//symbol");
        var activeFlags = new List<string>();

        foreach (var symbol in symbolNodes)
        {
            var id = symbol.GetAttributeValue("id", "");
            if (!string.IsNullOrEmpty(id) && !id.Contains("deactive"))
            {
                activeFlags.Add(id);
            }
        }

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromDays(1));

        memoryCache.Set(ActiveFlagsCacheKey, activeFlags, cacheOptions);

        return activeFlags;
    }
}