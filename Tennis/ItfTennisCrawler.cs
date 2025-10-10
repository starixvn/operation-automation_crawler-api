using Tennis.Extensions;
using Tennis.Models;
using Tennis.Models.ItfTennisModels;
using Microsoft.Extensions.Caching.Memory;

namespace Tennis;

public class ItfTennisCrawler(TimeIsCrawler timeIsCrawler, HttpClient httpClient, IMemoryCache memoryCache)
{
    private const int DelayInSeconds = 1;
    private const string CacheKeyPrefix = "ItfTennisLeague_";

    public async Task<List<League>> GetLeagues()
    {
        // Example: Ex: https://www.itftennis.com/tennis/api/TournamentApi/GetCalendar?circuitCode=WT&take=100&dateFrom=2025-10-01&dateTo=2025-10-31
        var rootUrl = "https://www.itftennis.com/tennis/api/TournamentApi/GetCalendar?circuitCode={0}"
                      + "&searchString=&skip=0&take=100&nationCodes=&zoneCodes="
                      + "&dateFrom={1:yyyy-MM-dd}&dateTo={2:yyyy-MM-dd}&indoorOutdoor="
                      + "&categories=&isOrderAscending=true&orderField=startDate&surfaceCodes=";
        var dateFrom = DateTime.Today.AddDays(-7);
        var dateTo = DateTime.Today.AddDays(7);

        var menLeagues = await GetLeagues(string.Format(rootUrl, "MT", dateFrom, dateTo));
        var womenLeagues = await GetLeagues(string.Format(rootUrl, "WT", dateFrom, dateTo));

        // Combine all leagues into one list
        var allLeagues = menLeagues.Concat(womenLeagues).ToList();
        return allLeagues;
    }

    private async Task<List<League>> GetLeagues(string url)
    {
        var response = await GetStringAndDelayAsync(url);

        var data = JsonExtensions.TryDeserializeObject<TournamentResponse>(response);
        if (data.Items == null)
        {
            return [];
        }

        var leagues= data.Items
            .Where(i => i.TourStatusDesc != "CANCELLED")
            .Select(tournament =>
        {
            var (dateFrom, dateTo) = DateTimeExtensions.ParseItfDateRange(tournament.Dates);
            return new League
            {
                Name = tournament.TournamentName,
                Id = tournament.TournamentKey,
                Source = $"https://www.itftennis.com{tournament.TournamentLink}",
                Date = tournament.Dates,
                DateFrom = dateFrom,
                DateTo = dateTo,
                Location = tournament.Location,
                Country = tournament.HostNation
            };
        }).ToList();

        leagues.ForEach(l => 
        { 
            var cacheKey = CacheKeyPrefix + l.Source.TrimEnd('/');
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromDays(7));
            memoryCache.Set(cacheKey, l, cacheOptions);
        });
        return leagues;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="leagueUrl">https://www.itftennis.com/en/tournament/w15-sharm-elsheikh/egy/2025/w-itf-egy-2025-018/</param>
    /// <param name="activeFlags"></param>
    /// <returns></returns>
    public async Task<List<Match>> GetMatches(string leagueUrl, List<string> activeFlags)
    {
        var key = leagueUrl.TrimEnd('/').Split('/').LastOrDefault() ?? string.Empty;
        var (id, date) = await GetSchedule(key);
        if (id <= 0)
        {
            return [];
        }

        var matchUrl = $"https://www.itftennis.com/tennis/api/TournamentApi/GetOrderOfPlay?orderOfPlayDayId={id}";
        return await GetMatches(matchUrl, leagueUrl, date, activeFlags);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="matchUrl">https://www.itftennis.com/tennis/api/TournamentApi/GetOrderOfPlay?orderOfPlayDayId=2581308</param>
    /// <param name="leagueUrl"></param>
    /// <param name="date"></param>
    /// <param name="activeFlags"></param>
    /// <returns></returns>
    private async Task<List<Match>> GetMatches(string matchUrl, string leagueUrl, DateTime date, List<string> activeFlags)
    {
        var response = await GetStringAndDelayAsync(matchUrl);

        var courts = JsonExtensions.TryDeserializeObject<List<ItfResponse>>(response);

        var matches = new List<Match>();

        foreach (var court in courts)
        {
            foreach (var m in court.Matches)
            {
                var match = await CreateMatch(m, leagueUrl, court.CourtName, date, activeFlags);
                matches.Add(match);
            }
        }

        return matches;
    }

    private async Task<Match> CreateMatch(ItfMatch match, string leagueUrl, string courtName, DateTime date, List<string> activeFlags)
    {
        var matchTime = ParseMatchTime(match.Schedule, date);
        
        var homePlayers = MapPlayersFromTeam(match.Teams[0].Players);
        var awayPlayers = MapPlayersFromTeam(match.Teams[1].Players);

        var result= new Match(match.MatchId)
        {
            OriginalDate = matchTime,
            Date = matchTime,
            Location = courtName,
            Home = FormatPlayerNames(homePlayers, activeFlags),
            Away = FormatPlayerNames(awayPlayers, activeFlags),
            Source = leagueUrl
        };

        var cacheKey = CacheKeyPrefix + leagueUrl;
        if (memoryCache.TryGetValue(cacheKey, out League league))
        {
            var timeIsModel = await timeIsCrawler.GetTimeIsModel(league.Location);
            result.Country = league.Country;
            result.City = league.Location;

            if (timeIsModel != null)
            {
                result.UtcOffset = timeIsModel.UtcOffset;
                result.Date = matchTime.AddHours(-result.UtcOffset + 8);
                result.UtcCity = timeIsModel.Location;
            }
        }

        return result;
    }

    private DateTime ParseMatchTime(string schedule, DateTime date)
    {
        var matchTime = date;
        if (schedule.StartsWith("Starting at "))
        {
            var timeStr = schedule.Replace("Starting at ", "");
            if (TimeSpan.TryParse(timeStr, out var time))
            {
                matchTime = date.Add(time);
            }
        }
        else if (schedule.StartsWith("Not Before "))
        {
            var timeStr = schedule.Replace("Not Before ", "");
            if (TimeSpan.TryParse(timeStr, out var time))
            {
                matchTime = date.Add(time);
            }
        }
        return matchTime;
    }

    private List<Player> MapPlayersFromTeam(List<ItfPlayer> teamPlayers)
    {
        return teamPlayers
            .Where(p => p != null)
            .Select(p => new Player
            {
                Id = p!.PlayerId.ToString(),
                Name = $"{p.GivenName} {p.FamilyName}".Trim(),
                Nationality = p.Nationality,
                Source = p.ProfileLink
            })
            .ToList();
    }

    private string FormatPlayerNames(List<Player> players, List<string> activeFlags)
    {
        return string.Join(" / ", players.Select(p =>
        {
            var flag = "flag-" + p.Nationality.ToLower();
            return activeFlags.Contains(flag) ? $"{p.Name} ({p.Nationality})" : p.Name;
        }));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key">w-itf-usa-2025-045</param>
    /// <returns></returns>
    private async Task<(long, DateTime)> GetSchedule(string key)
    {
        var url = $"https://www.itftennis.com/tennis/api/TournamentApi/GetOrderOfPlayDays?tournamentKey={key}";
        var response = await GetStringAndDelayAsync(url);

        var data = JsonExtensions.TryDeserializeObject<List<OrderOfPlayDay>>(response);
        var firstDay = data.FirstOrDefault();
        var id = firstDay?.OrderOfPlayDayId ?? 0;
        var date = firstDay?.PlayDate ?? DateTime.Today;
        return (id, date);
    }

    private async Task<string> GetStringAndDelayAsync(string url)
    {
        var response = await httpClient.GetAndUnzipStringAsync(url);
        await Task.Delay(TimeSpan.FromSeconds(DelayInSeconds));
        return response;
    }
}