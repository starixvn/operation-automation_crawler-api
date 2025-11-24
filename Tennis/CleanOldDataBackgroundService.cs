using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Tennis.Storage;

namespace Tennis;

public class CleanOldDataBackgroundService(TennisDbContext context):BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await SafeCleanData(stoppingToken);
            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }

    private async Task SafeCleanData(CancellationToken stoppingToken)
    {
        try
        {
            await CleanData(stoppingToken);
        }
        catch (Exception)
        {
        }
    }

    private async Task CleanData(CancellationToken stoppingToken)
    {
        var cutoffDate = DateTime.Today.AddDays(-30);
        var oldMatches = await context.TennisMatches
            .Where(m => m.KickoffTime < cutoffDate)
            .ToListAsync(stoppingToken);

        context.TennisMatches.RemoveRange(oldMatches);
        await context.SaveChangesAsync(stoppingToken);
    }
}