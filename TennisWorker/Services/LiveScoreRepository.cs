using Microsoft.EntityFrameworkCore;
using Tennis.Models.LiveTennis;
using Tennis.Storage;
using TennisWorker.Models;

namespace TennisWorker.Services;

public interface ILiveScoreRepository
{
    Task<IReadOnlyCollection<LiveScore>> GetByMatchAsync(string matchId, CancellationToken cancellationToken = default);

    Task InsertAsync(IEnumerable<LiveScore> liveScores, CancellationToken cancellationToken = default);
}

public class LiveScoreRepository(OperationCrawlerDbContext dbContext) : ILiveScoreRepository
{

    public async Task<IReadOnlyCollection<LiveScore>> GetByMatchAsync(string matchId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(matchId))
        {
            return Array.Empty<LiveScore>();
        }

        var entities = await dbContext.TennisLiveScores
            .AsNoTracking()
            .Where(score => score.match_id == matchId)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToModel).ToList();
    }

    public async Task InsertAsync(IEnumerable<LiveScore> liveScores, CancellationToken cancellationToken = default)
    {
        var scores = liveScores.ToList();
        if (scores.Count == 0)
        {
            return;
        }

        var entities = scores.Select(MapToEntity).ToList();

        await dbContext.TennisLiveScores.AddRangeAsync(entities, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static LiveScore MapToModel(TennisLiveScore entity) => new()
    {
        Id = entity.Id.ToString(),
        MatchId = entity.match_id,
        HomeValue = entity.home_value,
        AwayValue = entity.away_value,
        CreatedDate = entity.created_date,
        SetNumber = entity.set_number
    };

    private static TennisLiveScore MapToEntity(LiveScore score) => new()
    {
        match_id = score.MatchId ?? string.Empty,
        home_value = score.HomeValue,
        away_value = score.AwayValue,
        set_number = score.SetNumber,
        source = score.Source
    };
}

