using Helper;
using TennisWorker.Models;

namespace TennisWorker.Services;

public interface IScoreProcessing
{
    Task ProcessScoreAsync(List<LiveScore> liveScores, CancellationToken cancellationToken = default);
}

public class ScoreProcessing(ILiveScoreRepository liveScoreRepository, DynamicCaching cache) : IScoreProcessing
{
    private const string CacheKeyPrefix = "tennis-atp:livescore";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public async Task ProcessScoreAsync(List<LiveScore> liveScores, CancellationToken cancellationToken)
    {
        var groups = liveScores.GroupBy(score => score.MatchId);

        foreach (var group in groups)
        {
            var matchId = group.Key;
            var currentScore = await GetCurrentScore(matchId, cancellationToken);
            var newScores = GetNewScores(currentScore, group.ToList());
            if (newScores.Count > 0)
            {
                await liveScoreRepository.InsertAsync(newScores, cancellationToken);
                cache.SetValue<List<LiveScore>>(BuildCacheKey(matchId), group.ToList(), CacheDuration);
            }
        }
    }

    private List<LiveScore> GetNewScores(List<LiveScore> currentScore, List<LiveScore> liveScores)
    {
        return liveScores
            .Where(score => !currentScore.Any(c =>
                c.SetNumber == score.SetNumber &&
                c.HomeValue == score.HomeValue &&
                c.AwayValue == score.AwayValue))
            .ToList();
    }

    private async Task<List<LiveScore>> GetCurrentScore(string matchId, CancellationToken cancellationToken)
    {
        var cacheKey = BuildCacheKey(matchId);
        if (cache.TryGetValue(cacheKey, out List<LiveScore> cachedScore))
        {
            return cachedScore;
        }
        var scores = await liveScoreRepository.GetByMatchAsync(matchId, cancellationToken);
        return scores.ToList();
    }

    private static string BuildCacheKey(string matchId) => $"{CacheKeyPrefix}:{matchId}";
}