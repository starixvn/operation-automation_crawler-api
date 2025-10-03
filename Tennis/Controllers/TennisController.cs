using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.RegularExpressions;

namespace Tennis.Controllers;

[Route("api/[controller]")]
[ApiController]

public class TennisController(AtpTourCrawler atpTourCrawler) : ControllerBase
{
    /// <summary>
    /// http://localhost:5000/api/Tennis/atp-tour?tourUrl=https://www.atptour.com/en/scores/current/shanghai/5014/daily-schedule
    /// </summary>
    /// <param name="tourUrl"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("atp-tour")]
    public async Task<IActionResult> AtpTour(string tourUrl)
    {
        var schedules = await atpTourCrawler.GetSchedule(tourUrl);
        return Ok(schedules);
    }
}
