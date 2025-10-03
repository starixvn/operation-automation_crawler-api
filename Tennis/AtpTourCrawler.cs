using HtmlAgilityPack;
using System.Globalization;
using Fizzler.Systems.HtmlAgilityPack;
using System.Text.Json.Serialization;

namespace Tennis;

public class HandInfo
{
    [JsonPropertyName("Id")]
    public string Id { get; set; }

    [JsonPropertyName("Description")]
    public string Description { get; set; }
}

public class SocialLink
{
    [JsonPropertyName("SocialId")]
    public string SocialId { get; set; }

    [JsonPropertyName("SocialLink")]
    public string SocialUrl { get; set; }
}

public class PlayerResponse
{
    [JsonPropertyName("LastName")]
    public string LastName { get; set; }

    [JsonPropertyName("FirstName")]
    public string FirstName { get; set; }

    [JsonPropertyName("BirthCity")]
    public string BirthCity { get; set; }

    [JsonPropertyName("BirthDate")]
    public DateTime BirthDate { get; set; }

    [JsonPropertyName("Nationality")]
    public string Nationality { get; set; }
}

public record Player
{
    public string Id { get; set; }
    
    public string Name { get; init; }

    public DateTime BirthDay { get; init; }

    public string Nationality { get; init; }

    public string Birthplace { get; init; }
}

public record Schedule
{
    public DateTime Date { get; init; }

    public string Location { get; init; }

    public List<Player> Home { get; init; }

    public List<Player> Away { get; init; }

}

public class AtpTourCrawler(IHttpClientFactory httpClientFactory)
{
    private const string HttpClientName = "AtpTour";

    public async Task<Player> GetPlayer(string url)
    {
        // Extract player ID from URL
        var id = url.Replace("/overview","").TrimEnd('/').Split('/').Last().Split('/')[0];

        using var client = CreateHttpClient();
        var apiUrl = $"https://www.atptour.com/en/-/www/players/hero/{id}?v=1";
        var response = await client.GetStringAsync(apiUrl);
        
        var playerResponse = System.Text.Json.JsonSerializer.Deserialize<PlayerResponse>(response);

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

    public async Task<List<Schedule>> GetSchedule(string url)
    {
        using var client = CreateHttpClient();

        var response = await client.GetStringAsync(url);
        var doc = new HtmlDocument();
        doc.LoadHtml(response);

        var schedules = new List<Schedule>();
        var matchElements = doc.DocumentNode.QuerySelectorAll(".content-group .schedule");


        foreach (var matchElement in matchElements)
        {
            var dateStr = matchElement.GetAttributeValue("data-matchdate", "");
            var timeStr = matchElement.GetAttributeValue("data-datetime", "");
            DateTime matchDate;

            // Parse date and time
            if (!string.IsNullOrEmpty(timeStr))
            {
                matchDate = DateTime.Parse(timeStr);
            }
            else
            {
                matchDate = DateTime.Parse(dateStr);
            }

            // Get location
            var locationElement = matchElement.QuerySelector(".schedule-location-timestamp strong");
            var location = locationElement?.InnerText.Trim() ?? string.Empty;

            // Get home and away players
            var homeElement = matchElement.QuerySelector(".schedule-players .player");
            var awayElement = matchElement.QuerySelector(".schedule-players .opponent");

            var homePlayers = await GetPlayersFromElement(homeElement);
            var awayPlayers = await GetPlayersFromElement(awayElement);

            schedules.Add(new Schedule
            {
                Date = matchDate,
                Location = location,
                Home = homePlayers,
                Away = awayPlayers
            });
        }

        return schedules;
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