using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Tennis;

public class UtrSportsCrawler
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public UtrSportsCrawler(IConfiguration configuration, HttpClient httpClient)
    {
        _configuration = configuration;
        _httpClient = httpClient;
    }
    public async Task<string> Login()
    {
        var username = _configuration.GetValue<string>("UtrSportsUsername");
        var password = _configuration.GetValue<string>("UtrSportsPassword");
        const string url = "https://app.utrsports.net/api/v1/auth/login";

        var loginData = new { email = username, password = password };

        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
        
        request.Content = JsonContent.Create(loginData);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }
}