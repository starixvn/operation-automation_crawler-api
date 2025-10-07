using HtmlAgilityPack;
using Fizzler.Systems.HtmlAgilityPack;
using Newtonsoft.Json;
using Tennis.Models;

namespace Tennis;

public class AtpTourCrawler(IHttpClientFactory httpClientFactory)
{
    public class HandInfo
    {
        [JsonProperty("Id")]
        public string Id { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }
    }

    public class SocialLink
    {
        [JsonProperty("SocialId")]
        public string SocialId { get; set; }

        [JsonProperty("SocialLink")]
        public string SocialUrl { get; set; }
    }

    public class PlayerResponse
    {
        [JsonProperty("LastName")]
        public string LastName { get; set; }

        [JsonProperty("FirstName")]
        public string FirstName { get; set; }

        [JsonProperty("BirthCity")]
        public string BirthCity { get; set; }

        [JsonProperty("BirthDate")]
        public DateTime BirthDate { get; set; }

        [JsonProperty("Nationality")]
        public string Nationality { get; set; }
    }


    private const string HttpClientName = "AtpTour";

    public class TournamentResponse
    {
        [JsonProperty("TournamentDates")]
        public List<TournamentDate> TournamentDates { get; set; }
    }

    public class TournamentDate
    {
        [JsonProperty("DisplayDate")]
        public string DisplayDate { get; set; }

        [JsonProperty("Tournaments")]
        public List<Tournament> Tournaments { get; set; }
    }

    public class Tournament
    {
        [JsonProperty("Id")]
        public string Id { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("FormattedDate")]
        public string FormattedDate { get; set; }
    }

    public async Task<List<League>> GetLeagues()
    {
        const string url = "https://www.atptour.com/en/-/tournaments/calendar/tour";
        using var client = CreateHttpClient();

        var response = await client.GetStringAsync(url);
        var tournamentResponse = JsonConvert.DeserializeObject<TournamentResponse>(response);

        var leagues = new List<League>();
        foreach (var tournamentDate in tournamentResponse.TournamentDates)
        {
            foreach (var tournament in tournamentDate.Tournaments)
            {
                leagues.Add(new League
                {
                    Source = url,
                    Name = tournament.Name,
                    Id = tournament.Id,
                    Date = tournament.FormattedDate
                });
            }
        }

        return leagues;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key">5014</param>
    /// <returns></returns>
    public async Task<List<Match>> GetMatches(string key)
    {
        using var client = CreateHttpClient();

        var url = $"https://www.atptour.com/en/scores/current/leaguename/{key}/daily-schedule";
        var response = await client.GetStringAsync(url);
        var doc = new HtmlDocument();
        doc.LoadHtml(response);

        var schedules = new List<Match>();
        var matchElements = doc.DocumentNode.QuerySelectorAll(".content-group .schedule");


        foreach (var matchElement in matchElements)
        {
            var dateStr = matchElement.GetAttributeValue("data-matchdate", "");
            var timeStr = matchElement.GetAttributeValue("data-datetime", "");

            var matchDate =
                // Parse date and time
                DateTime.Parse(!string.IsNullOrEmpty(timeStr) ? timeStr : dateStr);

            // Get location
            var locationElement = matchElement.QuerySelector(".schedule-location-timestamp strong");
            var location = locationElement?.InnerText.Trim() ?? string.Empty;

            // Get home and away players
            var homeElement = matchElement.QuerySelector(".schedule-players .player");
            var awayElement = matchElement.QuerySelector(".schedule-players .opponent");

            var homePlayers = await GetPlayersFromElement(homeElement);
            var awayPlayers = await GetPlayersFromElement(awayElement);

            schedules.Add(new Match
            {
                Date = matchDate,
                Location = location,
                Home = homePlayers,
                Away = awayPlayers
            });
        }

        return schedules;
    }

    public async Task<Player> GetPlayer(string url)
    {
        // Extract player ID from URL
        var id = url.Replace("/overview","").TrimEnd('/').Split('/').Last().Split('/')[0];

        using var client = CreateHttpClient();
        var apiUrl = $"https://www.atptour.com/en/-/www/players/hero/{id}?v=1";
        var response = await client.GetStringAsync(apiUrl);
        
        var playerResponse = JsonConvert.DeserializeObject<PlayerResponse>(response);

        return new Player
        {
            Id = id,
            Name = $"{playerResponse.FirstName} {playerResponse.LastName}",
            BirthDay = playerResponse.BirthDate,
            Nationality = playerResponse.Nationality,
            Birthplace = playerResponse.BirthCity
        };
    }

    private async Task<List<Player>> GetPlayersFromElement(HtmlNode element)
    {
        var players = new List<Player>();
        var playerLinks = element.QuerySelectorAll(".name a");
        
        foreach (var link in playerLinks)
        {
            var playerUrl = "https://www.atptour.com" + link.GetAttributeValue("href", "");
            var player = await GetPlayer(playerUrl);
            players.Add(player);
        }

        return players;
    }

    private HttpClient CreateHttpClient()
    {
        HttpClient client = null;
        try
        {
            client = httpClientFactory.CreateClient(HttpClientName);
            client.DefaultRequestHeaders.Add("Accept", "text/html");
            client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en");
            return client;
        }
        catch
        {
            client?.Dispose();
            throw;
        }
    }
}