using Microsoft.AspNetCore.Mvc;
using Tennis.Models;

namespace Tennis.Controllers;

[Route("api/[controller]")]
[ApiController]

public class TennisController(AtpTourCrawler atpTourCrawler, ItfTennisCrawler itfTennisCrawler, UtrSportsCrawler utrSportsCrawler) : ControllerBase
{
    /// <summary>
    /// http://localhost:5000/api/Tennis/atp-tour?tourUrl=https://www.atptour.com/en/scores/current/shanghai/5014/daily-schedule
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("atp-tour/leagues")]
    public async Task<IActionResult> AtpTourLeague()
    {
        var models = await atpTourCrawler.GetLeagues();
        models = models.Where(m => m.DateFrom <= DateTime.Today.AddDays(10))
            .Where(m => m.DateTo >= DateTime.Today.AddDays(-1))
            .ToList();
        return Ok(models);
    }

    [HttpGet]
    [Route("atp-tour/matches")]
    public async Task<IActionResult> AtpTourMatches(string leagues = "9900,")
    {
        var models = await atpTourCrawler.GetMatches(leagues);
        return Ok(models);
    }

    [HttpGet]
    [Route("itf-tennis/leagues")]
    public async Task<IActionResult> ItfTennisLeague()
    {
        var models = await itfTennisCrawler.GetLeagues();
        models = models.Where(m => m.DateFrom <= DateTime.Today.AddDays(10))
            .Where(m => m.DateTo >= DateTime.Today.AddDays(-1))
            .ToList();
        return Ok(models);
    }

    /// <summary>
    /// http://localhost:5000/api/Tennis/leagues/itf-tennis/matches?leagues=m-itf-ita-2025-038;m-itf-rwa-2025-001
    /// </summary>
    /// <param name="leagues"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("itf-tennis/matches")]
    public async Task<IActionResult> ItfTennisMatches(string leagues)
    {
        leagues = string.IsNullOrEmpty(leagues) ? "m-itf-ita-2025-038;m-itf-rwa-2025-001" : leagues;
        var keyList = leagues.Split(';').ToList();
        var models = await itfTennisCrawler.GetMatches(keyList);

        return Ok(models);
    }

    [HttpGet]
    [Route("utr-sports/leagues")]
    public async Task<IActionResult> UtrSportsLeagues()
    {
        await utrSportsCrawler.Login();
        var models = await utrSportsCrawler.GetAllLeagues();
        models = models.Where(m => m.DateFrom <= DateTime.Today.AddDays(10))
            .Where(m => m.DateTo >= DateTime.Today.AddDays(-1))
            .ToList();
        return Ok(models);
    }

    [HttpGet]
    [Route("utr-sports/matches")]
    public async Task<IActionResult> UtrSportsMatches(string leagues = "303253;315527")
    {
        await utrSportsCrawler.Login();
        var leagueIds = leagues.Split(';').Select(long.Parse).ToList();
        var models = new List<Match>();
        foreach (var id in leagueIds)
        {
            var matches = await utrSportsCrawler.GetAllMatches(id);
            models.AddRange(matches);
        }

        return Ok(models);
    }
}
