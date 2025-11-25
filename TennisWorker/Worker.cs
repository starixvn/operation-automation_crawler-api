using Fanex.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TennisWorker.Services;

namespace TennisWorker;

public class TennisWorker(IFetchScoreService fetchScoreService, IScoreProcessing scoreProcessing, ILogger<TennisWorker> logger) : BackgroundService
{
    private readonly TimeSpan refreshInterval = TimeSpan.FromSeconds(5);


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var events = await fetchScoreService.FetchATPTennisScoreAsync(stoppingToken);
                //var itfEvents = await fetchScoreService.FetchITFTennisScoreAsync(stoppingToken);
                var liveScores = events
                    .SelectMany(e =>
                    {
                        e.LiveMatches.ForEach(r =>
                        {
                            r.LeagueId = e.EventId;
                            r.Source = e.EventTitle;
                        });

                        return e.LiveMatches;
                    })
                    .SelectMany(ScoreConverter.ConvertATPMatchScore)
                    .ToList();

                if (liveScores.Count > 0)
                {
                    await scoreProcessing.ProcessScoreAsync(liveScores, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                await LogService.ErrorAsync("Error fetching or processing live matches from ATP API", ex);
            }

            await Task.Delay(refreshInterval, stoppingToken);
        }
    }
}
