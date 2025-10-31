using Microsoft.Extensions.Caching.Abstractions;
using Microsoft.Extensions.Caching.InMemory;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace Tennis.Extensions;

public static class HttpClientExtensions
{
    private const int DefaultTimeoutSeconds = 300;
  
    public static HttpClient CreateAtpTourHttpClient()
    {
        var httpClientHandler = new  HttpClientHandler
        {
            UseCookies = false,
            AllowAutoRedirect = true,
            CookieContainer = new CookieContainer(),    
        };
        var cacheExpirationPerHttpResponseCode = CacheExpirationProvider.CreateSimple(TimeSpan.FromDays(7), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(5));
        var handler = new InMemoryCacheHandler(httpClientHandler, cacheExpirationPerHttpResponseCode);

        var client = new HttpClient(handler);
        client.DefaultRequestHeaders.Add("Accept", "text/html");
        client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en");
        client.Timeout = TimeSpan.FromSeconds(DefaultTimeoutSeconds);

        return client;
    }

    public static IHttpClientBuilder AddN8nHttpClient(this IServiceCollection services)
    {
        return services.AddHttpClient<TriggerN8NWorkflow>();
    }

    public static IHttpClientBuilder AddTimeIsHttpClient(
        this IServiceCollection services)
    {
        return services.AddHttpClient<TimeIsCrawler>(client =>
        {
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/141.0.0.0 Safari/537.36 Edg/141.0.0.0");
            client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
            client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9,fr;q=0.8,vi;q=0.7,af;q=0.6");
            client.DefaultRequestHeaders.Add("Priority", "u=0, i");
            client.DefaultRequestHeaders.Add("Sec-CH-UA", "\"Microsoft Edge\";v=\"141\", \"Not?A_Brand\";v=\"8\", \"Chromium\";v=\"141\"");
            client.DefaultRequestHeaders.Add("Sec-CH-UA-Mobile", "?0");
            client.DefaultRequestHeaders.Add("Sec-CH-UA-Platform", "\"Windows\"");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "document");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "navigate");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Site", "none");
            client.DefaultRequestHeaders.Add("Sec-Fetch-User", "?1");
            client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
            client.Timeout = TimeSpan.FromSeconds(DefaultTimeoutSeconds);
        }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            CookieContainer = new CookieContainer(),
            UseCookies = true
        });
    }
}
