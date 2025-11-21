using System.Text.Json;
using TennisWorker.Models.ATP;

namespace TennisWorker.Services;
public interface IFetchScoreService
{
    Task<List<AtpEvent>> FetchATPTennisScoreAsync(CancellationToken cancellationToken = default);
}

public class FetchScoreService(HttpClient httpClient) : IFetchScoreService
{
    private const string BaseUrl = "https://app.atptour.com/api/v2/gateway/livematches/website";
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
}
