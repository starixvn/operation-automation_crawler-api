using System.Net;
using HtmlAgilityPack;

namespace Tennis;

public class TimeIsCrawler(HttpClient httpClient)
{
    public record TimeIsModel
    {
        public double UtcOffset { get; init; }

        public string Location { get; init; }        
    }

    private static readonly Dictionary<string, TimeIsModel> Cache = new();
    public async Task<TimeIsModel> GetTimeIsModel(string location)
    {
        location = location.ToLower().Replace(" city", "").Trim();
        if (Cache.TryGetValue(location, out var cachedModel))
        {
            return cachedModel;
        }

        var url = $"https://time.is/{location}";
        
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
            if (utcOffsetNode == null)
            {
                return null;
            }

            var utcOffsetText = utcOffsetNode.InnerText.Trim(); // "UTC +8" or "UTC -8"
            var utcOffset = ParseUtcOffset(utcOffsetText);

            // Get location
            var locationNode = doc.DocumentNode.SelectSingleNode("//div[@id='msgdiv']");
            if (locationNode == null)
            {
                return null;
            }

            var locationText = locationNode.InnerText.Trim();
            locationText = locationText.Replace("Time in ", "").Replace("&nbsp;"," ").Trim();
            if (locationText.EndsWith(" now"))
            {
                locationText = locationText.Substring(0, locationText.Length - 4);
            }
            
            var model = new TimeIsModel
            {
                UtcOffset = utcOffset,
                Location = locationText
            };
            Cache[location] = model;

            return model;
        }
        catch (HttpRequestException)
        {
            return null;
        }
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