using ItfCrawler.Crawler;
using Microsoft.Extensions.Hosting;

namespace ItfCrawler;

public class ItfCrawlerBackgroundService(
    LeagueCrawler leagueCrawler,
    MatchCrawler matchCrawler,
    PlayerCrawler playerCrawler):BackgroundService
{
    private static readonly TimeSpan Delay = TimeSpan.FromHours(2);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
        while (!stoppingToken.IsCancellationRequested)
        {
            var leagues = await leagueCrawler.CrawlAndSaveLeagues();
            var keys = leagues.Select(l => l.Id).ToList();
            await matchCrawler.CrawlAndSaveMatches(keys);
            await playerCrawler.CrawlAndSavePlayers(leagues);

            await Task.Delay(Delay, stoppingToken);
        }
    }
}