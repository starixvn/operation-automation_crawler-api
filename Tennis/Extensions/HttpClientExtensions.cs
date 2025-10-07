using System.IO.Compression;
using System.Net;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Tennis.Extensions;

public static class HttpClientExtensions
{
    private const int DefaultTimeoutSeconds = 120;

    public static IHttpClientBuilder CreateHttpClient(
        this IServiceCollection services,
        string name,
        int? timeoutSeconds = null)
    {
        return services.AddHttpClient(name, client =>
        {
            // Set timeout
            client.Timeout = TimeSpan.FromSeconds(timeoutSeconds ?? DefaultTimeoutSeconds);
        });
    }

    public static IHttpClientBuilder CreateTypedHttpClient<TClient>(
        this IServiceCollection services,
        string name,
        int? timeoutSeconds = null) where TClient : class
    {
        var cookieContainer = new CookieContainer();
        var handler = new HttpClientHandler
        {
            CookieContainer = cookieContainer,
            UseCookies = true
        };

        return services.AddHttpClient<TClient>(name, client =>
        {
            // Set timeout
            client.Timeout = TimeSpan.FromSeconds(timeoutSeconds ?? DefaultTimeoutSeconds);
        }).ConfigurePrimaryHttpMessageHandler(() => handler);
    }

    public static async Task<string> GetAndUnzipStringAsync(this HttpClient client, string url)
    {
        using var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var content = response.Content;
        var contentEncoding = content.Headers.ContentEncoding?.FirstOrDefault()?.ToLower();

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
