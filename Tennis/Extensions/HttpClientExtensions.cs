using System.IO.Compression;
using System.Net;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Tennis.Extensions;

public static class HttpClientExtensions
{
    private const int DefaultTimeoutSeconds = 300;

    public static IHttpClientBuilder CreateAtpTourHttpClient(
        this IServiceCollection services)
    {
        var cookieContainer = new CookieContainer();
        var handler = new HttpClientHandler
        {
            CookieContainer = cookieContainer,
            UseCookies = true
        };

        return services.AddHttpClient<AtpTourCrawler>(client =>
        {
            client.DefaultRequestHeaders.Add("Accept", "text/html");
            client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en");
            client.Timeout = TimeSpan.FromSeconds(DefaultTimeoutSeconds);
        }).ConfigurePrimaryHttpMessageHandler(() => handler);
    }

    public static IHttpClientBuilder CreateItfHttpClient(
        this IServiceCollection services)
    {
        var cookieContainer = new CookieContainer();
        var handler = new HttpClientHandler
        {
            CookieContainer = cookieContainer,
            UseCookies = true
        };

        return services.AddHttpClient<ItfTennisCrawler>(client =>
        {
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/141.0.0.0 Safari/537.36 Edg/141.0.0.0");
            client.DefaultRequestHeaders.Add("Accept", "ext/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
            client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9,fr;q=0.8,vi;q=0.7,af;q=0.6");
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br"); 
            client.DefaultRequestHeaders.Add("Pragma", "no-cache");
            client.DefaultRequestHeaders.Add("Priority", "u=0, i");
            client.DefaultRequestHeaders.Add("Sec-CH-UA", "\"Microsoft Edge\";v=\"141\", \"Not?A_Brand\";v=\"8\", \"Chromium\";v=\"141\"");
            client.DefaultRequestHeaders.Add("Sec-CH-UA-Mobile", "?0");
            client.DefaultRequestHeaders.Add("Sec-CH-UA-Platform", "\"Windows\"");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "document");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "navigate");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Site", "none");
            client.DefaultRequestHeaders.Add("Sec-Fetch-User", "?1");
            client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
            client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            client.Timeout = TimeSpan.FromSeconds(DefaultTimeoutSeconds);
        }).ConfigurePrimaryHttpMessageHandler(() => handler);
    }

    public static IHttpClientBuilder CreateTimeIsHttpClient(
        this IServiceCollection services)
    {
        var cookieContainer = new CookieContainer();
        var handler = new HttpClientHandler
        {
            CookieContainer = cookieContainer,
            UseCookies = true
        };

        return services.AddHttpClient<TimeIsCrawler>(client =>
        {
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/141.0.0.0 Safari/537.36 Edg/141.0.0.0");
            client.DefaultRequestHeaders.Add("Accept", "ext/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
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
        }).ConfigurePrimaryHttpMessageHandler(() => handler);
    }

    public static async Task<string> GetAndUnzipStringAsync(this HttpClient client, string url)
    {
        using var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var content = response.Content;
        var contentEncoding = content.Headers.ContentEncoding.FirstOrDefault()?.ToLower();

        if (string.IsNullOrEmpty(contentEncoding))
        {
            return await content.ReadAsStringAsync();
        }

        await using var stream = await content.ReadAsStreamAsync();
        switch (contentEncoding)
        {
            case "gzip":
                {
                    await using var decompressedStream = new GZipStream(stream, CompressionMode.Decompress);
                    using var reader = new StreamReader(decompressedStream, Encoding.UTF8);
                    return await reader.ReadLineAsync();
                }
            case "deflate":
                {
                    await using var decompressedStream = new DeflateStream(stream, CompressionMode.Decompress);
                    using var reader = new StreamReader(decompressedStream, Encoding.UTF8);
                    return await reader.ReadLineAsync();
                }
            case "br":
                {
                    await using var decompressedStream = new BrotliStream(stream, CompressionMode.Decompress);
                    using var reader = new StreamReader(decompressedStream, Encoding.UTF8);
                    return await reader.ReadLineAsync();
                }
            default:
                throw new InvalidOperationException($"Unsupported content encoding: {contentEncoding}");
        }

    }
}
