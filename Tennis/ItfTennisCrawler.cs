using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Tennis.Extensions;
using Tennis.Models;
using Tennis.Models.ItfTennisModels;

namespace Tennis;

public class ItfTennisCrawler
{
    private readonly HttpClient httpClient;
    private const int DelayInSeconds = 5;

    public ItfTennisCrawler(HttpClient httpClient)
    {
        this.httpClient = httpClient;
        // Configure default headers
        this.httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/141.0.0.0 Safari/537.36 Edg/141.0.0.0");
        this.httpClient.DefaultRequestHeaders.Add("Accept", "ext/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
        this.httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9,fr;q=0.8,vi;q=0.7,af;q=0.6");
        this.httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
        this.httpClient.DefaultRequestHeaders.Add("Cookie", "_fbp=fb.1.1759725939384.572113403145088191; _gid=GA1.2.2101266261.1759725940; csrf_token=39650779-1ea8-4c89-ae42-4a3efdea8dd2; _tt_enable_cookie=1; _ttp=01K6VXJEHB05MXPS9H3C9G630Q_.tt.1; ttcsid=1759726090797::iZDZEKDbtNxXvPKtym4t.1.1759726139738.0; ttcsid_D0LHM03C77UFC0GSO0QG=1759726090797::LIcloSqCnQGF4VjfiimL.1.1759726139738.0; nlbi_178373=qEGnTnoOYS6hvm96toSRdQAAAABOGoJFcwWt/7wKj6ITcGj4; visid_incap_178373=GwhIHIUnTI+5af66AVg/fTnU5GgAAAAAQUIPAAAAAAAzB6lLdRowfoA2uShT+bG5; incap_ses_299_178373=IF4SYVxOxWcQG2t6TEMmBDzU5GgAAAAAKiFpr6NqdWBRZw+Ft14bKg==; ARRAffinity=cdf24dd0d44a5b177cc7702aae06bbdbf5f77196a1ebe37cfbdd11ebf8c8a283; ARRAffinitySameSite=cdf24dd0d44a5b177cc7702aae06bbdbf5f77196a1ebe37cfbdd11ebf8c8a283; _ga_ZRMHKRE9CL=GS2.1.s1759830202$o8$g1$t1759830317$j51$l0$h0; _ga=GA1.1.28378336.1759725940; _ga_DH6MMHJPYQ=GS2.2.s1759830203$o7$g1$t1759830317$j47$l0$h0");
        this.httpClient.DefaultRequestHeaders.Add("Pragma", "no-cache");
        this.httpClient.DefaultRequestHeaders.Add("Priority", "u=0, i");
        this.httpClient.DefaultRequestHeaders.Add("Sec-CH-UA", "\"Microsoft Edge\";v=\"141\", \"Not?A_Brand\";v=\"8\", \"Chromium\";v=\"141\"");
        this.httpClient.DefaultRequestHeaders.Add("Sec-CH-UA-Mobile", "?0");
        this.httpClient.DefaultRequestHeaders.Add("Sec-CH-UA-Platform", "\"Windows\"");
        this.httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "document");
        this.httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "navigate");
        this.httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Site", "none");
        this.httpClient.DefaultRequestHeaders.Add("Sec-Fetch-User", "?1");
        this.httpClient.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
        this.httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
    }

    /// <summary>
    /// https://www.itftennis.com/tennis/api/TournamentApi/GetCalendar?circuitCode=JT
    /// https://www.itftennis.com/tennis/api/TournamentApi/GetCalendar?circuitCode=MT
    /// https://www.itftennis.com/tennis/api/TournamentApi/GetCalendar?circuitCode=WT
    /// https://www.itftennis.com/tennis/api/TournamentApi/GetCalendar?circuitCode=WT&take=100&dateFrom=2025-10-01&dateTo=2025-10-31
    /// </summary>
    /// <returns></returns>
    public async Task<List<League>> GetLeagues()
    {
        var rootUrl = "https://www.itftennis.com/tennis/api/TournamentApi/GetCalendar?circuitCode={0}&searchString=&skip=0&take=100&nationCodes=&zoneCodes=&dateFrom={1:yyyy-MM-dd}&dateTo={2:yyyy-MM-dd}&indoorOutdoor=&categories=&isOrderAscending=true&orderField=startDate&surfaceCodes=";
        var dateFrom = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var dateTo = dateFrom.AddMonths(1).AddDays(-1);

        var menLeagues = await GetLeagues(string.Format(rootUrl, "MT", dateFrom, dateTo));
        var womenLeagues = await GetLeagues(string.Format(rootUrl, "WT", dateFrom, dateTo));
        var juniorLeagues = await GetLeagues(string.Format(rootUrl, "JT", dateFrom, dateTo));

        // Combine all leagues into one list
        var allLeagues = menLeagues.Concat(womenLeagues).Concat(juniorLeagues).ToList();
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
                Source = url,
                Date = tournament.Dates,
                DateFrom = dateFrom,
                DateTo = dateTo
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

    public async Task<List<Match>> GetMatches(List<string> keys)
    {
        var result = new List<Match>();
        foreach (var key in keys)
        {
            var matches = await GetMatches(key);
            result.AddRange(matches);

        }

        return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key">w-itf-usa-2025-045</param>
    /// <returns></returns>
    public async Task<List<Match>> GetMatches(string key)
    {
        var (id, date) = await GetSchedule(key);
        if (id <= 0)
        {
            return [];
        }

        var matchUrl = $"https://www.itftennis.com/tennis/api/TournamentApi/GetOrderOfPlay?orderOfPlayDayId={id}";
        return await GetMatches(matchUrl, date);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="url">Example: https://www.itftennis.com/tennis/api/TournamentApi/GetOrderOfPlay?orderOfPlayDayId=2581308</param>
    /// <param name="date"></param>
    /// <returns></returns>
    private async Task<List<Match>> GetMatches(string url, DateTime date)
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

                schedules.Add(new Match
                {
                    Date = matchTime,
                    Location = court.CourtName,
                    Home = homePlayers,
                    Away = awayPlayers,
                    Source = url
                });
            }
        }

        return schedules;
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="url">Example: https://www.itftennis.com/en/players/gabriele-crivellaro/800590928/ita/mt/s/overview/#pprofile-info-tabs</param>
    /// <returns></returns>
    public async Task<Player> GetPlayer(string url)
    {
        var response = await GetStringAndDelayAsync(url);

        var doc = new HtmlDocument();
        doc.LoadHtml(response);

        // Extract ID from URL
        var id = url.Split('/').FirstOrDefault(s => s.All(char.IsDigit)) ?? string.Empty;

        // Extract Age and Calculate Birth Year
        var ageNode = doc.DocumentNode.QuerySelector("div.player-hero__details-item span.player-hero__value");
        var age = ageNode != null ? int.Parse(ageNode.InnerText.Trim()) : 0;
        var birthYear = DateTime.Now.Year - age;

        // Extract data from JSON-LD
        var jsonLdScript = doc.DocumentNode.SelectNodes("//script[@type='application/ld+json']")
            ?.FirstOrDefault()?.InnerText;

        var name = string.Empty;
        var nationality = string.Empty;
        var gender = "unknown";

        if (!string.IsNullOrEmpty(jsonLdScript))
        {
            var jsonLdData = System.Text.Json.JsonDocument.Parse(jsonLdScript);
            var root = jsonLdData.RootElement;

            name = root.GetProperty("name").GetString() ?? string.Empty;
            nationality = root.GetProperty("nationality").GetString() ?? string.Empty;
            var genderValue = root.GetProperty("gender").GetString();
            gender = genderValue == "M" ? "male" : (genderValue == "F" ? "female" : "unknown");
        }

        return new Player
        {
            Id = id,
            Name = name,
            BirthDay = new DateTime(birthYear, 1, 1), // Setting to January 1st since we only have the year
            Nationality = nationality,
            Birthplace = string.Empty,
            Gender = gender,
            Source = url
        };
    }


    private async Task<string> GetStringAndDelayAsync(string url)
    {
        var response = await httpClient.GetAndUnzipStringAsync(url);
        await Task.Delay(TimeSpan.FromSeconds(DelayInSeconds));
        return response;
    }
}