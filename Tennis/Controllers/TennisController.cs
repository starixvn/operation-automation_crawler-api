using Microsoft.AspNetCore.Mvc;
using Tennis.Models;

namespace Tennis.Controllers;

[Route("api/[controller]")]
[ApiController]

public class TennisController(AtpTourCrawler atpTourCrawler, ItfTennisCrawler itfTennisCrawler) : ControllerBase
{
    /// <summary>
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("atp-tour/leagues")]
    public async Task<IActionResult> AtpTourLeague()
    {
        var models = await atpTourCrawler.GetLeagues();
        models = models.Where(m => m.DateFrom <= DateTime.Today.AddDays(10))
            .Where(m => m.DateTo >= DateTime.Today.AddDays(-1))
            .Distinct()
            .ToList();
        return Ok(models);
    }

    [HttpGet]
    [Route("atp-tour/matches")]
    public async Task<IActionResult> AtpTourMatches(string leagues = "https://www.atptour.com/en/scores/current/shanghai/5014/daily-schedule;https://www.atptour.com/en/scores/current/shanghai/9410/daily-schedule")
    {
        await atpTourCrawler.GetActiveFlags();

        var leagueList = leagues.Split(';').Select(l => l.TrimEnd('/')).Distinct().ToList();
        var models = new List<Match>();

        foreach (var league in leagueList)
        {
            var matches = await atpTourCrawler.GetMatches(league);
            models.AddRange(matches);
        }

        models = models.Where(m => !string.IsNullOrEmpty(m.Home) && !string.IsNullOrEmpty(m.Away))
            .ToList();
        return Ok(models);
    }

    [HttpGet]
    [Route("itf-tennis/leagues")]
    public async Task<IActionResult> ItfTennisLeague()
    {
        var models = await itfTennisCrawler.GetLeagues();
        models = models.Where(m => m.DateFrom <= DateTime.Today.AddDays(10))
            .Where(m => m.DateTo >= DateTime.Today.AddDays(-1))
            .Distinct()
            .ToList();
        return Ok(models);
    }

    /// <summary>
    /// </summary>
    /// <param name="leagues"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("itf-tennis/matches")]
    public async Task<IActionResult> ItfTennisMatches(string leagues = "https://www.itftennis.com/en/tournament/m25-kigali/rwa/2025/m-itf-rwa-2025-001;https://www.itftennis.com/en/tournament/w100-edmond-ok/usa/2025/M-ITF-BRA-2025-004/order-of-play/")
    {
        try
        {
            var activeFlags = await atpTourCrawler.GetActiveFlags();
            var leagueList = leagues.Split(';').Select(l => l.TrimEnd('/')).Distinct().ToList();
            var models = new List<Match>();

            foreach (var league in leagueList)
            {
                var matches = await itfTennisCrawler.GetMatches(league, activeFlags);
                models.AddRange(matches);
            }

            models = models.Where(m => !string.IsNullOrEmpty(m.Home) && !string.IsNullOrEmpty(m.Away))
                .ToList();
            return Ok(models);
        }
        catch (Exception e)
        {
            return BadRequest(e);
        }
      
    }
}
