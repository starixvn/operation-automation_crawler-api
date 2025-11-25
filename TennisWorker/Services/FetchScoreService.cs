using System.Text.Json;
using TennisWorker.Models.ATP;
using TennisWorker.Models.ITF;

namespace TennisWorker.Services;
public interface IFetchScoreService
{
    Task<List<AtpEvent>> FetchATPTennisScoreAsync(CancellationToken cancellationToken = default);
    Task<List<ItfEventInfo>> FetchITFTennisScoreAsync(CancellationToken cancellationToken = default);
}

public class FetchScoreService(HttpClient httpClient) : IFetchScoreService
{
    private const string BaseUrl = "https://app.atptour.com/api/v2/gateway/livematches/website";

    private const string ItfBaseUrl = "https://api.itf-production.sports-data.stadion.io/custom/wttCompleteMatchList/";
    private static readonly string[] TournamentLevels = ["challenger", "tour"];

    public async Task<List<AtpEvent>> FetchATPTennisScoreAsync(CancellationToken cancellationToken = default)
    {
        var events = new List<AtpEvent>();
        foreach (var level in TournamentLevels)
        {
            events.AddRange(await FetchEventsForLevelAsync(level, cancellationToken));
            events.ForEach(e => e.EventTitle = $"ATP-{level}");
        }
        return events;
    }

    public async Task<List<ItfEventInfo>> FetchITFTennisScoreAsync(CancellationToken cancellationToken = default)
    {
        var requestUri = $"{ItfBaseUrl}{DateTimeOffset.Now:yyyy-MM-dd}";
        using var response = await httpClient.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var apiResponse = await JsonSerializer.DeserializeAsync<EventDataResponse>(
            responseStream,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            },
            cancellationToken);

        return apiResponse?.Data is not { Count: > 0 } data ? [] : data.Values.ToList();
    }

    private async Task<List<AtpEvent>> FetchEventsForLevelAsync(string level, CancellationToken cancellationToken)
    {
        var requestUri = $"{BaseUrl}?scoringTournamentLevel={level}";
        using var response = await httpClient.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var apiResponse = await JsonSerializer.DeserializeAsync<AtpLiveMatchesResponse>(responseStream, cancellationToken: cancellationToken);

        if (apiResponse?.Data?.LiveMatchesTournamentsOrdered is not { Count: > 0 } tournaments)
        {
            return [];
        }

        return tournaments.ToList();
    }

    //private List<ItfEventInfo> FilterCourtDetails(List<ItfEventInfo> eventInfos)
    //{
    //    var filteredEventInfos = new List<ItfEventInfo>();
    //    eventInfos.ForEach(e =>
    //    {
    //        foreach (var court in e.Courts)
    //        {
    //            foreach (var detail in court.Value)
    //            {
    //                if (detail.ActualEndDate > DateTime.Now)
    //                {
    //                    filteredEventInfos.Add(e);
    //                }
    //            }
    //        }
    //    });
    //}
}
