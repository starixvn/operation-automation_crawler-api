using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;

namespace Tennis.Extensions;

public static class HttpClientExtensions
{
    private const int DefaultTimeoutSeconds = 30;

    public static IHttpClientBuilder CreateHttpClient(
        this IServiceCollection services,
        string name,
        int? timeoutSeconds = null)
    {
        return services.AddHttpClient(name, client =>
        {
            // Standard headers
            client.DefaultRequestHeaders.Add("Accept", "text/html");
            client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en");

            // Set timeout
            client.Timeout = TimeSpan.FromSeconds(timeoutSeconds ?? DefaultTimeoutSeconds);
        });
    }
}
