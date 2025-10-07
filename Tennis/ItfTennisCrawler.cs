using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Tennis.Extensions;
using Tennis.Models;

namespace Tennis;


public class ItfTennisCrawler(IHttpClientFactory httpClientFactory)
{

    public class ItfResponse
    {
        [JsonProperty("courtName")]
        public string CourtName { get; set; } = string.Empty;

        [JsonProperty("matches")]
        public List<ItfMatch> Matches { get; set; } = new();
    }

    public class ItfMatch
    {
        [JsonProperty("schedule")]
        public string Schedule { get; set; } = string.Empty;

        [JsonProperty("teams")]
        public List<ItfTeam> Teams { get; set; } = new();
    }

    public class ItfTeam
    {
        [JsonProperty("players")]
        public List<ItfPlayer?> Players { get; set; } = new();
    }

    public class ItfPlayer
    {
        [JsonProperty("playerId")]
        public long PlayerId { get; set; }

        [JsonProperty("nationality")]
        public string Nationality { get; set; } = string.Empty;

        [JsonProperty("givenName")]
        public string GivenName { get; set; } = string.Empty;

        [JsonProperty("familyName")]
        public string FamilyName { get; set; } = string.Empty;

        [JsonProperty("profileLink")]
        public string ProfileLink { get; set; } = string.Empty;
    }


    private const string HttpClientName = "ItfTennis";

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
        var rootUrl = $"https://www.itftennis.com/tennis/api/TournamentApi/GetCalendar?circuitCode=";
        var parameters = $"&dateFrom={DateTime.Today.AddDays(-14):yyyy-MM-dd}&dateTo={DateTime.Today.AddDays(14):yyyy-MM-dd}";
        
        var menLeagues = await GetLeagues(rootUrl + "MT" + parameters);
        var womenLeagues = await GetLeagues(rootUrl + "WT" + parameters);
        var juniorLeagues = await GetLeagues(rootUrl + "JT" + parameters);
        
        // Combine all leagues into one list
        var allLeagues = menLeagues.Concat(womenLeagues).Concat(juniorLeagues).ToList();
        return allLeagues;
    }

    private async Task<List<League>> GetLeagues(string url)
    {
        using var client = CreateHttpClient();
        var response = await GetStringAndDelayAsync(url);

        var data = JsonConvert.DeserializeObject<TournamentResponse>(response);
        if (data?.Items == null)
        {
            return [];
        }

        return data.Items.Select(tournament => new League
        {
            Name = tournament.TournamentName,
            Id = tournament.TournamentKey,
            Source = url,
            Date = tournament.Dates
        }).ToList();
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
        using var client = CreateHttpClient();
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
        using var client = CreateHttpClient();
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

    public class TournamentResponse
    {
        [JsonProperty("url")]
        public string Url { get; set; } = string.Empty;

        [JsonProperty("items")]
        public List<TournamentItem> Items { get; set; } = new();

        [JsonProperty("totalItems")]
        public int TotalItems { get; set; }
    }

    public class TournamentItem
    {
        [JsonProperty("tournamentName")]
        public string TournamentName { get; set; } = string.Empty;

        [JsonProperty("tournamentKey")]
        public string TournamentKey { get; set; } = string.Empty;

        [JsonProperty("dates")]
        public string Dates { get; set; } = string.Empty;
    }

    public class OrderOfPlayDay
    {
        [JsonProperty("orderOfPlayDayId")]
        public long OrderOfPlayDayId { get; set; }

        [JsonProperty("playDate")]
        public DateTime PlayDate { get; set; }

        [JsonProperty("playDateString")]
        public string PlayDateString { get; set; } = string.Empty;
    }

    private async Task <string> GetStringAndDelayAsync(string url)
    {
        using var client = CreateHttpClient();
        var response = await client.GetAndUnzipStringAsync(url);
        await Task.Delay(TimeSpan.FromSeconds(DelayInSeconds));

        return response;
    }

    private HttpClient CreateHttpClient()
    {
        HttpClient client = null;
        try
        {
            client = httpClientFactory.CreateClient(HttpClientName);
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36");
            client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
            client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9,fr;q=0.8,vi;q=0.7,af;q=0.6");
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
            return client;
        }
        catch
        {
            client?.Dispose();
            throw;
        }
    }
}