using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Tennis.Extensions;
using Tennis.Models;
using Tennis.Models.ItfTennisModels;

namespace Tennis;

public class ItfTennisCrawler(HttpClient httpClient, IConfiguration configuration)
{
    private readonly CountryUtcConfig countryUtcConfig = configuration.GetSection(nameof(CountryUtcConfig)).Get<CountryUtcConfig>() ?? new CountryUtcConfig();

    private const int DelayInSeconds = 1;

    /// <summary>
    /// https://www.itftennis.com/tennis/api/TournamentApi/GetCalendar?circuitCode=JT
    /// https://www.itftennis.com/tennis/api/TournamentApi/GetCalendar?circuitCode=MT
    /// https://www.itftennis.com/tennis/api/TournamentApi/GetCalendar?circuitCode=WT
    /// https://www.itftennis.com/tennis/api/TournamentApi/GetCalendar?circuitCode=WT&take=100&dateFrom=2025-10-01&dateTo=2025-10-31
    /// </summary>
    /// <returns></returns>
    public async Task<List<League>> GetLeagues()
    {
        var rootUrl =
            "https://www.itftennis.com/tennis/api/TournamentApi/GetCalendar?circuitCode={0}&searchString=&skip=0&take=100&nationCodes=&zoneCodes=&dateFrom={1:yyyy-MM-dd}&dateTo={2:yyyy-MM-dd}&indoorOutdoor=&categories=&isOrderAscending=true&orderField=startDate&surfaceCodes=";
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

        var data = JsonConvert.DeserializeObject<TournamentResponse>(response);
        if (data?.Items == null)
        {
            return [];
        }

        return data.Items.Select(tournament =>
        {
            var (dateFrom, dateTo) = ParseDateRange(tournament.Dates);
            return new League
            {
                Name = tournament.TournamentName,
                Id = tournament.TournamentKey,
                Source =
                    $"https://www.itftennis.com/en/tournament/w100-edmond-ok/usa/2025/{tournament.TournamentKey}/order-of-play/",
                Date = tournament.Dates,
                DateFrom = dateFrom,
                DateTo = dateTo,
                Location = tournament.Location
            };
        }).ToList();
    }

    private (DateTime dateFrom, DateTime dateTo) ParseDateRange(string value)
    {
        // Split the string by "to" and trim whitespace
        var parts = value.Split("to", StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
        {
            throw new ArgumentException($"Invalid date range format: {value}");
        }

        // Parse the first date (e.g. "27 Oct")
        var fromParts = parts[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var day1 = int.Parse(fromParts[0]);
        var month1 = DateTime.ParseExact(fromParts[1], "MMM", System.Globalization.CultureInfo.InvariantCulture).Month;

        // Parse the second date (e.g. "02 Nov 2025")
        var toParts = parts[1].Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var day2 = int.Parse(toParts[0]);
        var month2 = DateTime.ParseExact(toParts[1], "MMM", System.Globalization.CultureInfo.InvariantCulture).Month;
        var year = int.Parse(toParts[2]);

        // Create DateTime objects
        var dateFrom = new DateTime(year, month1, day1);
        var dateTo = new DateTime(year, month2, day2);

        return (dateFrom, dateTo);
    }

    public async Task<List<Match>> GetMatches(List<string> keys, List<string> activeFlags)
    {
        var result = new List<Match>();
        foreach (var key in keys)
        {
            var matches = await GetMatches(key, activeFlags);
            result.AddRange(matches);

        }

        return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key">w-itf-usa-2025-045</param>
    /// <param name="activeFlags"></param>
    /// <returns></returns>
    public async Task<List<Match>> GetMatches(string key, List<string> activeFlags)
    {
        var (id, date) = await GetSchedule(key);
        if (id <= 0)
        {
            return [];
        }

        var matchUrl = $"https://www.itftennis.com/tennis/api/TournamentApi/GetOrderOfPlay?orderOfPlayDayId={id}";
        return await GetMatches(matchUrl, date, key, activeFlags);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="url">Example: https://www.itftennis.com/tennis/api/TournamentApi/GetOrderOfPlay?orderOfPlayDayId=2581308</param>
    /// <param name="date"></param>
    /// <param name="key"></param>
    /// <param name="activeFlags"></param>
    /// <returns></returns>
    private async Task<List<Match>> GetMatches(string url, DateTime date, string key, List<string> activeFlags)
    {
        var response = await GetStringAndDelayAsync(url);

        var courts = JsonConvert.DeserializeObject<List<ItfResponse>>(response);
        if (courts == null)
        {
            return [];
        }

        var schedules = new List<Match>();

        foreach (var court in courts)
        {
            foreach (var match in court.Matches)
            {
                // Parse time from schedule
                var matchTime = date;
                if (match.Schedule.StartsWith("Starting at "))
                {
                    var timeStr = match.Schedule.Replace("Starting at ", "");
                    if (TimeSpan.TryParse(timeStr, out var time))
                    {
                        matchTime = date.Add(time);
                    }
                }
                else if (match.Schedule.StartsWith("Not Before "))
                {
                    var timeStr = match.Schedule.Replace("Not Before ", "");
                    if (TimeSpan.TryParse(timeStr, out var time))
                    {
                        matchTime = date.Add(time);
                    }
                }

                // Convert players to our Player model
                var homePlayers = match.Teams[0].Players
                    .Where(p => p != null)
                    .Select(p => new Player
                    {
                        Id = p!.PlayerId.ToString(),
                        Name = $"{p.GivenName} {p.FamilyName}".Trim(),
                        Nationality = p.Nationality,
                        Source = p.ProfileLink
                    })
                    .ToList();

                var awayPlayers = match.Teams[1].Players
                    .Where(p => p != null)
                    .Select(p => new Player
                    {
                        Id = p!.PlayerId.ToString(),
                        Name = $"{p.GivenName} {p.FamilyName}".Trim(),
                        Nationality = p.Nationality,
                        Source = p.ProfileLink
                    })
                    .ToList();

                var home = string.Join(" / ", homePlayers.Select(p =>
                {
                    var flag = "flag-" + p.Nationality.ToLower();
                    return activeFlags.Contains(flag) ? $"{p.Name} ({p.Nationality})" : p.Name;
                }));
                var away = string.Join(" / ", awayPlayers.Select(p =>
                {
                    var flag = "flag-" + p.Nationality.ToLower();
                    return activeFlags.Contains(flag) ? $"{p.Name} ({p.Nationality})" : p.Name;
                }));

                var gmt8MatchTime = GetGmt8DateTime(matchTime, key);
                schedules.Add(new Match
                {
                    Date = gmt8MatchTime,
                    Location = court.CourtName,
                    Home = home,
                    Away = away,
                    Source = $"https://www.itftennis.com/en/tournament/w100-edmond-ok/usa/2025/{key}/order-of-play/"
                });
            }
        }

        return schedules;
    }

    private DateTime GetGmt8DateTime(DateTime value, string key)
    {
        var offset = GetUtcOffsetFromKey(key);
        var gmt8Time = value.AddHours(8 - offset);
        return gmt8Time;
    }

    private int GetUtcOffsetFromKey(string key)
    {
        var countryCode = GetCountryCode(key);
        return GetUtcOffset(countryCode);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="key">w-itf-can-2025-006</param>
    /// <returns></returns>
    private string GetCountryCode(string key)
    {
        // Split the key by '-' and get the country code part (e.g., "w-itf-can-2025-006" -> ["w", "itf", "can", "2025", "006"])
        var parts = key.Split('-');
        if (parts.Length >= 3)
        {
            // Return the country code part in uppercase (e.g., "can" -> "CAN")
            return parts[2].ToUpper();
        }
        return string.Empty;
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

        var data = JsonConvert.DeserializeObject<List<OrderOfPlayDay>>(response);
        var id = data?.FirstOrDefault()?.OrderOfPlayDayId ?? 0;
        var date = data?.FirstOrDefault()?.PlayDate ?? DateTime.Today;
        return (id, date);
    }

    private async Task<string> GetStringAndDelayAsync(string url)
    {
        var response = await httpClient.GetAndUnzipStringAsync(url);
        await Task.Delay(TimeSpan.FromSeconds(DelayInSeconds));
        return response;
    }

    private int GetUtcOffset(string countryCode)
    {
        // Convert to uppercase for consistency
        countryCode = countryCode.ToUpper();

        // Search through all regions for the country code
        foreach (var region in countryUtcConfig.CountryUtcOffsets)
        {
            if (region.Value.TryGetValue(countryCode, out int offset))
            {
                return offset;
            }
        }

        // Return UTC (0) if country code is not found
        return 0;
    }

}