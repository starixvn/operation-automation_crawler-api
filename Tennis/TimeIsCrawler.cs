using Helper;
using HtmlAgilityPack;
using System.Net;

namespace Tennis;

public class TimeIsCrawler(HttpClient httpClient, DynamicCaching cache)
{
    public record TimeIsModel
    {
        public double UtcOffset { get; init; }

        public string Location { get; init; }
    }

    public async Task<TimeIsModel> GetTimeIsModel(string location)
    {
        var exact = await GetExact(location);
        if (exact != null)
        {
            return exact;
        }

        var guess = await Guess(location);
        if (guess != null)
        {
            location = location.ToLower().Replace(" city", "").Trim();
            var cacheKey = $"TimeIs-{location}";
            cache.SetValue(cacheKey, guess, TimeSpan.FromDays(30));
        }
        else
        {
            Console.WriteLine($"Cannot find timezone for {location}");
        }

        return guess;
    }

    private async Task<TimeIsModel> GetExact(string location)
    {
        location = location.ToLower().Replace(" city", "").Trim();
        var cacheKey = $"TimeIs-{location}";
        if (cache.TryGetValue(cacheKey, out TimeIsModel cachedModel))
        {
            return cachedModel;
        }

        var url = $"https://time.is/{location}";
        Console.WriteLine($"Fetching time information for: {url}");

        try
        {
            using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

            // Follow redirects automatically (HttpClient does this by default)
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Get UTC offset
            var utcOffsetNode = doc.DocumentNode.SelectSingleNode("//div[@class='current_UTC_offset']");


            var utcOffsetText = utcOffsetNode.InnerText.Trim(); // "UTC +8" or "UTC -8"
            var utcOffset = ParseUtcOffset(utcOffsetText);

            // Get location
            var locationNode = doc.DocumentNode.SelectSingleNode("//div[@id='msgdiv']");

            var locationText = locationNode.InnerText.Trim();
            locationText = locationText.Replace("Time in ", "").Replace("&nbsp;", " ").Trim();
            if (locationText.EndsWith(" now"))
            {
                locationText = locationText.Substring(0, locationText.Length - 4);
            }

            var model = new TimeIsModel
            {
                UtcOffset = utcOffset,
                Location = locationText
            };
            cache.SetValue(cacheKey, model, TimeSpan.FromDays(30));

            return model;
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    private async Task<TimeIsModel> Guess(string location)
    {
        var parts = location.Split([' ', ',', ';'], StringSplitOptions.RemoveEmptyEntries);

        var tasks = parts.Select(GetExact).ToList();

        await Task.WhenAll(tasks);

        var models = tasks.Select(t => t.Result)
            .Where(t => t != null)
            .ToList();

        if (models.Count > 0 && models.Select(m => m.UtcOffset).Distinct().Count() == 1)
        {
            return models[0];
        }

        return null;
    }

    private static double ParseUtcOffset(string utcOffsetText)
    {
        var offset = utcOffsetText.Replace("UTC ", "").Trim();
        if (offset.Contains(":"))
        {
            var parts = offset.Split(':');
            var hours = double.Parse(parts[0]);
            var minutes = double.Parse(parts[1]);
            return hours + (minutes / 60.0);
        }
        return double.Parse(offset);
    }

}