using ItfCrawler.Crawler;
using Microsoft.Extensions.Hosting;

namespace ItfCrawler;

internal class ItfCountryCrawlerBackgroundService(CountryCrawler countryCrawler) : BackgroundService
{
    private static readonly TimeSpan Delay = TimeSpan.FromDays(1);
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
        while (!stoppingToken.IsCancellationRequested)
        {
            await countryCrawler.CrawlAndSaveCountries();
            await Task.Delay(Delay, stoppingToken);
        }
    }
}