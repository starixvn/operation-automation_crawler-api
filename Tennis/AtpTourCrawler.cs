using HtmlAgilityPack;
using Fizzler.Systems.HtmlAgilityPack;
using Newtonsoft.Json;
using Tennis.Models;
using Tennis.Models.AptTourModels;
using Microsoft.Extensions.Caching.Memory;

namespace Tennis;

public class AtpTourCrawler(HttpClient client, IMemoryCache memoryCache)
{
    private const string ACTIVE_FLAGS_CACHE_KEY = "ActiveFlags";
    private readonly IMemoryCache _memoryCache = memoryCache;

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
                var (dateFrom, dateTo) = ParseDateRange(tournament.FormattedDate);
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

    private (DateTime dateFrom, DateTime dateTo) ParseDateRange(string value)
    {
        var parts = value.Split(" - ", StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
        {
            throw new ArgumentException($"Invalid date range format: {value}");
        }

        // Try to parse end date first as it always has the year
        if (!DateTime.TryParseExact(parts[1], "d MMMM, yyyy",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out var dateTo))
        {
            throw new ArgumentException($"Invalid end date format: {parts[1]}. Value={value}");
        }

        // Try to parse start date as full date first (with year)
        if (DateTime.TryParseExact(parts[0], "d MMMM, yyyy",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out var dateFrom))
        {
            return (dateFrom, dateTo);
        }

        // Try to parse start date with month but no year
        var startDateWithYear = $"{parts[0]}, {dateTo.Year}";
        if (DateTime.TryParseExact(startDateWithYear, "d MMMM, yyyy",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out dateFrom))
        {
            // If the parsed date is after the end date, it means we need to use the previous year
            if (dateFrom > dateTo)
            {
                startDateWithYear = $"{parts[0]}, {dateTo.Year - 1}";
                if (!DateTime.TryParseExact(startDateWithYear, "d MMMM, yyyy",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None,
                        out dateFrom))
                {
                    throw new ArgumentException($"Invalid start date format: {startDateWithYear}. Value={value}");
                }
            }
            return (dateFrom, dateTo);
        }

        // If start date is just a day, combine it with month and year from end date
        if (int.TryParse(parts[0], out int startDay))
        {
            // If the start day is greater than the end day, and they're in different months,
            // we need to use the previous month
            var startMonth = dateTo.Month;
            var startYear = dateTo.Year;

            if (startDay > dateTo.Day)
            {
                startMonth = dateTo.Month == 1 ? 12 : dateTo.Month - 1;
                startYear = startMonth == 12 ? startYear - 1 : startYear;
            }

            var startDateStr = $"{startDay} {new DateTime(startYear, startMonth, 1):MMMM}, {startYear}";

            if (!DateTime.TryParseExact(startDateStr, "d MMMM, yyyy",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out dateFrom))
            {
                throw new ArgumentException($"Invalid start date format: {startDateStr}. Value={value}");
            }
        }
        else
        {
            throw new ArgumentException($"Invalid start date format: {parts[0]}. Value={value}");
        }

        return (dateFrom, dateTo);
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

        var apiUrl = $"https://www.atptour.com/en/-/www/players/hero/{id}?v=1";
        var response = await client.GetStringAsync(apiUrl);

        var playerResponse = JsonConvert.DeserializeObject<PlayerResponse>(response);
        var activeFlags = await GetActiveFlags();
        var nation = activeFlags.Contains(country) ? $" ({playerResponse.NatlId})" : string.Empty;
        return new Player
        {
            Id = id,
            Name = $"{playerResponse.FirstName} {playerResponse.LastName}{nation}",
            BirthDay = playerResponse.BirthDate,
            Nationality = playerResponse.Nationality,
            Birthplace = playerResponse.BirthCity
        };
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
        if (_memoryCache.TryGetValue(ACTIVE_FLAGS_CACHE_KEY, out List<string> cachedFlags))
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

        _memoryCache.Set(ACTIVE_FLAGS_CACHE_KEY, activeFlags, cacheOptions);

        return activeFlags;
    }
}