using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Tennis.Extensions;

public static class HttpClientExtensions
{
    private const int DefaultTimeoutSeconds = 300;
    private static readonly HttpClientHandler ItfHttpClientHandler = new()
    {
        CookieContainer = new CookieContainer(),
        UseCookies = true
    };

    public static IHttpClientBuilder CreateAtpTourHttpClient(
        this IServiceCollection services)
    {
        return services.AddHttpClient<AtpTourCrawler>(client =>
        {
            client.DefaultRequestHeaders.Add("Accept", "text/html");
            client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en");
            client.Timeout = TimeSpan.FromSeconds(DefaultTimeoutSeconds);
        }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            CookieContainer = new CookieContainer(),
            UseCookies = true
        });
    }

    public static IHttpClientBuilder CreateItfHttpClient(
        this IServiceCollection services)
    {
        return services.AddHttpClient<ItfTennisCrawler>(client =>
        {
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/141.0.0.0 Safari/537.36 Edg/141.0.0.0");
            client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
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
        }).ConfigurePrimaryHttpMessageHandler(() => ItfHttpClientHandler);
    }

    public static HttpClient CreateItfHttpClientInstance()
    {
        var handler = new HttpClientHandler
        {
            CookieContainer = new CookieContainer(),
            UseCookies = true,
            AllowAutoRedirect = true
        };

        var client = new HttpClient(handler);

        SetItfDefaultHeader(client);
        client.Timeout = TimeSpan.FromSeconds(DefaultTimeoutSeconds);

        return client;
    }

    public static void SetItfDefaultHeader(this HttpClient client)
    {
        const string baseAuthority = "https://www.itftennis.com";
        // === 1. Set standard required headers ===

        // 1.1. User-Agent
        // The User-Agent string is crucial for mimicking a browser.
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/140.0.0.0 Safari/537.36");

        // 1.2. Accept Header
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");

        // 1.3. Accept-Encoding (This is often handled automatically by HttpClient, but we specify it)
        client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate, br, zstd");

        // 1.4. Accept-Language
        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US,en;q=0.9");

        // 1.5. Cache-Control and If-Modified-Since
        client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { MaxAge = TimeSpan.FromSeconds(0) };
        client.DefaultRequestHeaders.IfModifiedSince = new DateTimeOffset(2025, 10, 13, 3, 53, 36, TimeSpan.Zero); // GMT time

        // 1.6. Upgrade-Insecure-Requests
        client.DefaultRequestHeaders.TryAddWithoutValidation("Upgrade-Insecure-Requests", "1");

        // 1.7. Referer Policy (simulated via the Referer header)
        // Although the API request is often direct, providing a Referer is safer for scraping.
        client.DefaultRequestHeaders.Referrer = new Uri(baseAuthority);


        // === 2. Set Chrome-specific/Sec-Fetch/Custom Headers ===
        // HttpClient doesn't have native properties for these, so we use TryAddWithoutValidation.
        // Note: :authority, :method, :path, :scheme are pseudo-headers in HTTP/2 and not sent as standard headers.

        // Sec-Ch-Ua Headers (Client Hints)
        client.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua", "\"Chromium\";v=\"140\", \"Not=A?Brand\";v=\"24\", \"Google Chrome\";v=\"140\"");
        client.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
        client.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-platform", "\"Windows\"");

        // Sec-Fetch Headers
        client.DefaultRequestHeaders.TryAddWithoutValidation("sec-fetch-dest", "document");
        client.DefaultRequestHeaders.TryAddWithoutValidation("sec-fetch-mode", "navigate");
        client.DefaultRequestHeaders.TryAddWithoutValidation("sec-fetch-site", "none");
        client.DefaultRequestHeaders.TryAddWithoutValidation("sec-fetch-user", "?1");

        // Priority
        client.DefaultRequestHeaders.TryAddWithoutValidation("priority", "u=0, i");
    }

    public static IHttpClientBuilder CreateTimeIsHttpClient(
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
