using Microsoft.AspNetCore.Mvc;

namespace Tennis.Controllers;

[Route("api/[controller]")]
[ApiController]

public class TennisController(AtpTourCrawler atpTourCrawler, ItfTennisCrawler itfTennisCrawler) : ControllerBase
{
    /// <summary>
    /// http://localhost:5000/api/Tennis/atp-tour?tourUrl=https://www.atptour.com/en/scores/current/shanghai/5014/daily-schedule
    /// </summary>
    /// <param name="tourUrl"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("leagues/atp-tour")]
    public async Task<IActionResult> AtpTour()
    {
        var leagues = await atpTourCrawler.GetLeagues();
        return Ok(leagues);
    }

    [HttpGet]
    [Route("leagues/itf-tennis")]
    public async Task<IActionResult> ItfTennis()
    {
        var leagues = await itfTennisCrawler.GetLeagues();
        return Ok(leagues);
    }

    /// <summary>
    /// http://localhost:5000/api/Tennis/leagues/itf-tennis/matches?keys=m-itf-ita-2025-038;m-itf-rwa-2025-001
    /// </summary>
    /// <param name="keys"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("leagues/itf-tennis/matches")]
    public async Task<IActionResult> ItfTennisMatches(string keys)
    {
        keys = string.IsNullOrEmpty(keys) ? "m-itf-ita-2025-038;m-itf-rwa-2025-001" : keys;
        var keyList = keys.Split(';').ToList();
        var matches = await itfTennisCrawler.GetMatches(keyList);
        
        return Ok(matches);
    }
}
