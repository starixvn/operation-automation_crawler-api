using HtmlAgilityPack;
using Fizzler.Systems.HtmlAgilityPack;
using Newtonsoft.Json;
using Tennis.Models;
using Tennis.Models.AptTourModels;

namespace Tennis;

public class AtpTourCrawler(IHttpClientFactory httpClientFactory)
{
    private const string HttpClientName = "AtpTour";

    

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
                var (dateFrom, dateTo) = ParseDateRange(tournament.FormattedDate);
                leagues.Add(new League
                {
                    Source = url,
                    Name = tournament.Name,
                    Id = tournament.Id,
                    Date = tournament.FormattedDate,
                    DateFrom = dateFrom,
                    DateTo = dateTo
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

        DateTime dateFrom, dateTo;

        // Try to parse end date first as it always has the year
        if (!DateTime.TryParseExact(parts[1], "d MMMM, yyyy",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out dateTo))
        {
            throw new ArgumentException($"Invalid end date format: {parts[1]}. Value={value}");
        }

        // Try to parse start date as full date first (with year)
        if (DateTime.TryParseExact(parts[0], "d MMMM, yyyy",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out dateFrom))
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
            // If the start day is greater than the end day and they're in different months,
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