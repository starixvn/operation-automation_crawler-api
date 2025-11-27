using HtmlAgilityPack;
using ItfCrawler.Storage;

namespace ItfCrawler.Crawler;

public class CountryCrawler(IHttpClientFactory httpClientFactory, ItfStorage storage)
{
    private const string ApiUrl = "https://www.atptour.com/assets/atptour/assets/flags.svg";
    private readonly HttpClient client = httpClientFactory.CreateClient();

    public async Task CrawlAndSaveCountries()
    {
        var html = await client.GetStringAsync(ApiUrl);
        var countries = ParseCountries(html).ToList();

        await storage.SaveCountriesAsync(countries);
    }

    private IEnumerable<TennisCountry> ParseCountries(string svgContent)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(svgContent);

        var symbolNodes = doc.DocumentNode.SelectNodes("//symbol");

        foreach (var symbol in symbolNodes)
        {
            var id = symbol.GetAttributeValue("id", "");
            var deactive = id.Contains("deactive");
            var code = id.Replace("flag", "")
                .Replace("deactive", "")
                .Replace("-", "");
            yield return new TennisCountry
            {
                Code = code,
                Active = !deactive
            };
        }
    }
}