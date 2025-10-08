using Microsoft.AspNetCore.Mvc;
using Tennis.Models;

namespace Tennis.Controllers;

[Route("api/[controller]")]
[ApiController]

public class TennisController(AtpTourCrawler atpTourCrawler, ItfTennisCrawler itfTennisCrawler) : ControllerBase
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
    public async Task<IActionResult> AtpTourMatches(string leagues = "5014;9410")
    {
        await atpTourCrawler.GetActiveFlags();

        var keys = leagues.Split(';');
        var models = new List<Match>();

        foreach (var key in keys)
        {
            var matches = await atpTourCrawler.GetMatches(key);
            models.AddRange(matches);
        }
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
    /// </summary>
    /// <param name="keys"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("itf-tennis/matches")]
    public async Task<IActionResult> ItfTennisMatches(string keys = "M-ITF-TUN-2025-045;M-ITF-BRA-2025-004")
    {
        var activeFlags = await atpTourCrawler.GetActiveFlags();
        var keyList = keys.Split(';').ToList();
        var models = await itfTennisCrawler.GetMatches(keyList, activeFlags);
        return Ok(models);
    }

    //[HttpGet]
    //[Route("utr-sports/leagues")]
    //public async Task<IActionResult> UtrSportsLeagues()
    //{
    //    await utrSportsCrawler.Login();
    //    var models = await utrSportsCrawler.GetAllLeagues();
    //    models = models.Where(m => m.DateFrom <= DateTime.Today.AddDays(10))
    //        .Where(m => m.DateTo >= DateTime.Today.AddDays(-1))
    //        .ToList();
    //    return Ok(models);
    //}

    //[HttpGet]
    //[Route("utr-sports/matches")]
    //public async Task<IActionResult> UtrSportsMatches(string leagues = "303253;315527")
    //{
    //    await utrSportsCrawler.Login();
    //    var leagueIds = leagues.Split(';').Select(long.Parse).ToList();
    //    var models = new List<Match>();
    //    foreach (var id in leagueIds)
    //    {
    //        var matches = await utrSportsCrawler.GetAllMatches(id);
    //        models.AddRange(matches);
    //    }

    //    return Ok(models);
    //}
}
