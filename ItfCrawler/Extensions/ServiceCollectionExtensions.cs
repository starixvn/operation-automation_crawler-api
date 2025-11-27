using System.Net;
using System.Net.Http.Headers;
using ItfCrawler.Crawler;
using ItfCrawler.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ItfCrawler.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddItfService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<CountryCrawler>();
        services.AddSingleton<LeagueCrawler>();
        services.AddSingleton<MatchCrawler>();
        services.AddSingleton<PlayerCrawler>();
        services.AddSingleton<ItfStorage>();
        //services.AddHostedService<ItfCrawlerBackgroundService>();
        //services.AddHostedService<ItfCountryCrawlerBackgroundService>();
        services.AddItfHttpClient();
        services.AddItfMySqlDatabase(configuration);

        return services;
    }

    public static IServiceCollection AddItfMySqlDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("TennisDatabase");

        services.AddDbContext<ItfDbContext>(options =>
                options.UseMySql(
                    connectionString,
                    ServerVersion.AutoDetect(connectionString)
                ).LogTo(Console.WriteLine, LogLevel.Information),
            ServiceLifetime.Singleton
        );
        return services;
    }

    public static IHttpClientBuilder AddItfHttpClient(this IServiceCollection services)
    {
        AppContext.SetSwitch("System.Net.Http.EnableActivityPropagation", false);

        var cookieContainer = new CookieContainer();
        var cookieString = "ARRAffinity=267e41c40942d87f5c40e95a0bc8e4e360ff8dfc03abb6f61c6fbff0beb2bde6; ARRAffinitySameSite=267e41c40942d87f5c40e95a0bc8e4e360ff8dfc03abb6f61c6fbff0beb2bde6; nlbi_178373=ctj9YSvjNiHx+Jt7toSRdQAAAAC2k6ORVLzNxSHIl6ShBIXb; visid_incap_178373=WOkzcENeS0ic9MZdx+wv8vx1JmkAAAAAQUIPAAAAAACYdRvLpT59uKI5l4MjNp9S; incap_ses_301_178373=5e2IV99uBkzflceTV14tBDSaJmkAAAAAOD63/3BaA7cE1K/fbMrmKQ==";

        cookieContainer.SetCookies(new Uri("https://www.itftennis.com"), cookieString);

        return services.AddHttpClient("ItfHttpClient", ConfigureItfHttpClient())
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = cookieContainer,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli,
            });
    }

    private static Action<HttpClient> ConfigureItfHttpClient()
    {
        return httpClient =>
        {
            httpClient.DefaultRequestVersion = HttpVersion.Version20;
            httpClient.DefaultRequestHeaders.Clear();

            httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");

            httpClient.DefaultRequestHeaders.Pragma.ParseAdd("no-cache");

            httpClient.DefaultRequestHeaders.CacheControl = CacheControlHeaderValue.Parse("no-cache");

            httpClient.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");

            var userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/142.0.0.0 Safari/537.36 Edg/142.0.0.0";
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);

            httpClient.DefaultRequestHeaders.Add("sec-ch-ua", "\"Chromium\";v=\"142\", \"Microsoft Edge\";v=\"142\", \"Not_A Brand\";v=\"99\"");

            httpClient.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");

            httpClient.DefaultRequestHeaders.Accept.ParseAdd("*/*");

            httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Site", "none");

            httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "cors");

            httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "empty");

            httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Storage-Access", "active");

            httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
            httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("br"));
            httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("zstd"));

            httpClient.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US,en;q=0.9,fr;q=0.8,vi;q=0.7,af;q=0.6");
        };
    }
}