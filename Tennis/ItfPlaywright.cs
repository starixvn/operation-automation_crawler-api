using Microsoft.Playwright;

namespace Tennis;

public static class ItfPlaywright
{
    private static IPlaywright _playwright;
    private static IBrowser _browser;
    private static IBrowserContext _context;
    private static readonly SemaphoreSlim _semaphore = new(1, 1);
    private static readonly Dictionary<string, string> _defaultHeaders = new()
    {
        ["accept"] = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7",
        ["accept-encoding"] = "gzip, deflate, br, zstd",
        ["accept-language"] = "en-US,en;q=0.9",
        ["sec-ch-ua"] = "\"Chromium\";v=\"140\", \"Not=A?Brand\";v=\"24\", \"Google Chrome\";v=\"140\"",
        ["sec-ch-ua-mobile"] = "?0",
        ["sec-ch-ua-platform"] = "\"Windows\"",
        ["sec-fetch-dest"] = "document",
        ["sec-fetch-mode"] = "navigate",
        ["sec-fetch-site"] = "none",
        ["sec-fetch-user"] = "?1",
        ["upgrade-insecure-requests"] = "1",
        ["user-agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/140.0.0.0 Safari/537.36"
    };

    private static async Task EnsureBrowserInitialized()
    {
        if (_browser != null && _context != null) return;

        await _semaphore.WaitAsync();
        try
        {
            if (_browser != null && _context != null) return;

            _playwright = await Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });
            _context = await _browser.NewContextAsync();

            // Set default cookies
            await _context.AddCookiesAsync(new[] {
                new Cookie {
                    Name = "visid_incap_178373",
                    Value = "GnAykRp6S3Cam49xIUdQhW2F7GgAAAAAQUIPAAAAAABUYVQNy/LvjuNrTMFnHDYh",
                    Domain = "www.itftennis.com",
                    Path = "/"
                }
            });
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public static async Task<string> GetJsonByPlayWright(string url = "https://www.itftennis.com/tennis/api/TournamentApi/GetOrderOfPlay?orderOfPlayDayId=2581740")
    {
        await EnsureBrowserInitialized();

        var page = await _context.NewPageAsync();
        try
        {
            await page.SetExtraHTTPHeadersAsync(_defaultHeaders);

            var response = await page.GotoAsync(url, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 30000 // 30 seconds timeout
            });

            if (response == null)
                throw new Exception($"Failed to load URL: {url}");

            if (!response.Ok)
                throw new Exception($"HTTP {response.Status} received for URL: {url}");

            return await response.TextAsync();
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    public static async Task DisposeAsync()
    {
        if (_context != null)
        {
            await _context.DisposeAsync();
            _context = null;
        }

        if (_browser != null)
        {
            await _browser.DisposeAsync();
            _browser = null;
        }

        if (_playwright != null)
        {
            _playwright.Dispose();
            _playwright = null;
        }
    }
}