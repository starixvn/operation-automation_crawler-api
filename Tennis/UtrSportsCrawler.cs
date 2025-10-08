/*
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Tennis.Models;
using Tennis.Models.UtrSportsModels;

namespace Tennis;

public class UtrSportsCrawler(IConfiguration configuration, HttpClient httpClient)
{
    public async Task<string> Login()
    {
        var username = configuration.GetValue<string>("UtrSportsUsername");
        var password = configuration.GetValue<string>("UtrSportsPassword");
        const string url = "https://app.utrsports.net/api/v1/auth/login";

        var loginData = new { email = username, password = password };

        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
        

        request.Content = JsonContent.Create(loginData);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }

    public async Task<List<League>> GetAllLeagues()
    {
        const string inProgressUrl = "https://api.utrsports.net/v2/search/proTennisTour?top=40&skip=0&utrType=verified&utrTeamType=doubles&sort=eventSchedule.eventStartUtc%3Adesc&showInProgressEvents=true&searchOrigin=searchPage";
        const string upComingUrl = "https://api.utrsports.net/v2/search/proTennisTour?top=40&skip=0&utrType=verified&utrTeamType=doubles&searchOrigin=searchPage";

        var inProgress = await GetAllLeagues(inProgressUrl);
        var upComing = await GetAllLeagues(upComingUrl);

        return inProgress.Concat(upComing).ToList();
    }

    private async Task<List<League>> GetAllLeagues(string url)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
        

        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<UtrLeagueResponse>();

        return result?.Hits
            .Select(hit => new League
            {
                Id = hit.Id,
                Name = hit.Source.Club.Name,
                Source = url,
                Date = hit.Source.EventSchedule.EventDatesLong,
                DateFrom = hit.Source.EventSchedule.EventStartUtc,
                DateTo = hit.Source.EventSchedule.EventEndUtc
            }).ToList() ?? [];
    }

    public async Task<List<Match>> GetAllMatches(long eventId)
    {
        var utrMatches = await GetAllUtrMatches(eventId);
        var utrPlayers = await GetAllPlayers(eventId);

        var utrPlayersDict = utrPlayers.ToDictionary(p => p.MemberId, p => p);

        return utrMatches.Select(m =>
        {
            utrPlayersDict.TryGetValue(m.Position1.MemberId, out UtrPlayer player1);
            utrPlayersDict.TryGetValue(m.Position2.MemberId, out UtrPlayer player2);
            var home = new Player
            {
                Name = player1.FirstName + " " + player1.LastName,
                Gender = player1.Gender,
                Nationality = player1.Nationality
            };

            var away = new Player
            {
                Name = player2.FirstName + " " + player2.LastName,
                Gender = player2.Gender,
                Nationality = player2.Nationality,
                Source = $"https://api.utrsports.net/v2/tms/events/{eventId}/all-players"
            };
            return new Match
            {
                Date = m.Date,
                Location = "TBD",
                Home = [home],
                Away = [away],
                Source = $"https://api.utrsports.net/v1/tms/events/{eventId}/order-of-play/matches"

            };
        }).ToList();
    }

    private async Task<List<UtrPlayer>> GetAllPlayers(long eventId)
    {
        var url = $"https://api.utrsports.net/v2/tms/events/{eventId}/all-players";

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
        

        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<UtrPlayersResponse>();

        return result?.Players ?? [];
    }

    private async Task<List<UtrMatch>> GetAllUtrMatches(long eventId)
    {
        var url = $"https://api.utrsports.net/v1/tms/events/{eventId}/order-of-play/matches";

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));        

        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<UtrMatchesResponse>();
        return result?.Matches ?? new List<UtrMatch>();
    }
}

*/