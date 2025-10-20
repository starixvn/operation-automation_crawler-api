using System.Globalization;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Helper;
using Tennis.Extensions;
using Tennis.Models;
using Tennis.Models.AptTourModels;

namespace Tennis;

public class AtpTourCrawler(DynamicCaching cache)
{
    private const string ActiveFlagsCacheKey = "ActiveFlags";
    private static readonly HttpClient Client = HttpClientExtensions.CreateAtpTourHttpClient();

    public async Task<List<League>> GetLeagues()
    {
        const string tourUrl = "https://www.atptour.com/en/-/tournaments/calendar/tour";
        const string challengerUrl = "https://www.atptour.com/en/-/tournaments/calendar/challenger";
        var tours = await GetLeagues(tourUrl, "Tournament");
        var challengers = await GetLeagues(challengerUrl, "Challengers");

        var leagues = tours.Concat(challengers).ToList();
        leagues = leagues
            .Where(l => l.DateFrom <= DateTime.Today.AddDays(10))
            .Where(l => l.DateTo >= DateTime.Today.AddDays(-1))
            .Distinct()
            .ToList();
        Console.WriteLine($"Found {leagues.Count} leagues");
        return leagues;
    }

    private async Task<List<League>> GetLeagues(string url, string group)
    {
        var response = await Client.GetStringAsync(url);
        var tournamentResponse = JsonConvert.DeserializeObject<TournamentResponse>(response);

        var leagues = new List<League>();
        foreach (var tournamentDate in tournamentResponse.TournamentDates)
        {
            foreach (var tournament in tournamentDate.Tournaments)
            {
                var (dateFrom, dateTo) = DateTimeExtensions.ParseAtpTourDateRange(tournament.FormattedDate);
                if (dateFrom > DateTime.Today.AddDays(10) || dateTo < DateTime.Today.AddDays(-1))
                {
                    continue;
                }

                var leagueUrl = $"https://www.atptour.com{tournament.TournamentOverviewUrl}";
                var players = await GetPlayersFromTournament(leagueUrl);
                var playerNames = players.Select(p => p.Name).ToList();
                Console.WriteLine($"Found {players.Count} players for league {leagueUrl}");
                leagues.Add(new League
                {
                    Source = leagueUrl,
                    Name = tournament.Name,
                    Id = tournament.Id,
                    Date = tournament.FormattedDate,
                    DateFrom = dateFrom,
                    DateTo = dateTo,
                    Location = tournament.Location,
                    ScheduleUrl = $"https://www.atptour.com{tournament.ScheduleUrl}",
                    Group = group,
                    Players = playerNames
                });
            }
        }

        return leagues;
    }

    public async Task<List<Match>> GetMatches(League league)
    {
        if (string.IsNullOrEmpty(league.ScheduleUrl))
        {
            return [];
        }

        var days = await GetDaysOfLeague(league.ScheduleUrl);
        if (days.Count == 0)
        {
            return await GetMatches(league.ScheduleUrl, string.Empty, league.Group);
        }
        else
        {
            var result = new List<Match>();
            foreach (var day in days)
            {
                var matches = await GetMatches(league.ScheduleUrl, day, league.Group);
                result.AddRange(matches);
            }

            return result;
        }
    }

    private async Task<List<Match>> GetMatches(string leagueUrl, string day, string group)
    {
        if (!string.IsNullOrEmpty(day))
        {
            leagueUrl += $"?day={day}";
        }

        var response = await Client.GetStringAsync(leagueUrl);
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
                Source = leagueUrl,
                Group = group
            });
        }

        return schedules;
    }

    private async Task<List<string>> GetDaysOfLeague(string leagueUrl)
    {
        var response = await Client.GetStringAsync(leagueUrl);
        var doc = new HtmlDocument();
        doc.LoadHtml(response);

        var dropdown = doc.DocumentNode.QuerySelector("#matchDate-filter");
        var options = dropdown?.SelectNodes(".//option");
        var days = options?.Select(option => option.GetAttributeValue("value", ""))
            .Select(d=>d.Trim())
            .Where(d=>!string.IsNullOrEmpty(d))
            .Select(d =>
            {
                var date = DateTime.TryParseExact(d, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate)
                    ? parsedDate
                    : DateTime.MinValue;
                return date;
            })
            .Where(d=>d>=DateTime.Today)
            .Select(d=>d.ToString("yyyy-MM-dd"))
            .ToList();

        return days ?? [];
    }

    public async Task<Player> GetPlayer(string url, string country)
    {
        // Extract player ID from URL
        var id = url.Replace("/overview", "").TrimEnd('/').Split('/').Last().Split('/')[0];
        var playerCacheKey = $"AtpPlayer-{id}";
        // Try to get player from cache
        if (cache.TryGetValue(playerCacheKey, out Player cachedPlayer))
        {
            return cachedPlayer;
        }

        var apiUrl = $"https://www.atptour.com/en/-/www/players/hero/{id}?v=1";
        var response = await Client.GetStringAsync(apiUrl);

        var playerResponse = JsonConvert.DeserializeObject<PlayerResponse>(response);
        var activeFlags = await GetActiveFlags();
        var nation = activeFlags.Contains(country) ? $" ({playerResponse.NatlId})" : string.Empty;
        var player = new Player(id)
        {
            Name = $"{playerResponse.FirstName} {playerResponse.LastName}{nation}",
            BirthDay = playerResponse.BirthDate,
            Nationality = playerResponse.Nationality,
            Birthplace = playerResponse.BirthCity
        };

        cache.SetValue(playerCacheKey, player, TimeSpan.FromDays(30));

        return player;
    }

    private async Task<List<Player>> GetPlayersFromElement(HtmlNode element)
    {
        var players = new List<Player>();
        var playerLinks = element.QuerySelectorAll(".name a").ToList();
        var flagLinks = element.QuerySelectorAll(".atp-flag use").ToList();

        if (playerLinks.Count != flagLinks.Count)
        {
            return [];
        }
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
        if (cache.TryGetValue(ActiveFlagsCacheKey, out List<string> cachedFlags))
        {
            return cachedFlags;
        }

        const string url = "https://www.atptour.com/assets/atptour/assets/flags.svg";
        var response = await Client.GetStringAsync(url);

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

        cache.SetValue(ActiveFlagsCacheKey, activeFlags, TimeSpan.FromDays(1));

        return activeFlags;
    }

    public async Task<List<Player>> GetPlayersFromTournament(string leagueUrl)
    {
        var cacheKey ="AtpTournamentPlayers-" + leagueUrl;
        if (cache.TryGetValue(cacheKey, out List<Player> cachedPlayers))
        {
            if (cachedPlayers.Count > 0)
            {
                return cachedPlayers;
            }
        }

        var drawUrls = GetDrawLink(leagueUrl);
        var singlePlayers = await GetPlayersFromTournament(drawUrls, "singles");
        var doublePlayers = await GetPlayersFromTournament(drawUrls, "doubles");
        var qualifierPlayers = await GetPlayersFromTournament(drawUrls, "qualifiersingles");

        var players= singlePlayers.Concat(doublePlayers).Concat(qualifierPlayers).Distinct().ToList();
        cache.SetValue(cacheKey, players, TimeSpan.FromDays(30));

        return players;
    }

    private async Task<List<Player>> GetPlayersFromTournament(string drawUrls, string matchtype)
    {
        var result = new List<Player>();
        var url = drawUrls + $"?matchtype={matchtype}";
        var response = await Client.GetStringAsync(url);

        var doc = new HtmlDocument();
        doc.LoadHtml(response);

        var playerNodes = doc.DocumentNode.QuerySelectorAll(".player-info");
        foreach (var playerNode in playerNodes)
        {
            var players = await GetPlayersFromElement(playerNode);
            result.AddRange(players);
        }

        return result;
    }

    private string GetDrawLink(string leagueUrl)
    {
        return leagueUrl.Replace("https://www.atptour.com/en/tournaments/", "https://www.atptour.com/en/scores/current/")
            .Replace("/overview", "/draws");
    }
}