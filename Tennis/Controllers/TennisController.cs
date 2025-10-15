using Microsoft.AspNetCore.Mvc;
using Tennis.Models.ItfTennisModels;

namespace Tennis.Controllers;

[Route("api/[controller]")]
[ApiController]

public class TennisController(AtpTourCrawler atpTourCrawler, ItfTennisParser itfTennisParser) : ControllerBase
{
    /// <summary>
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("atp-tour")]
    public async Task<IActionResult> AtpTour()
    {
        var leagues = await atpTourCrawler.GetLeagues();
        leagues = leagues.Where(m => m.DateFrom <= DateTime.Today.AddDays(10))
            .Where(m => m.DateTo >= DateTime.Today.AddDays(-1))
            .Distinct()
            .ToList();
        foreach (var league in leagues)
        {
            var matches = await atpTourCrawler.GetMatches(league);
            league.Matches = matches.Where(m => !string.IsNullOrEmpty(m.Home))
                .Where(m => !string.IsNullOrEmpty(m.Away))
                .ToList();
        }

        return Ok(leagues);
    }


    [HttpPost]
    [Route("itf-tennis")]
    public async Task<IActionResult> ItfTennis(List<ItfTennisLeague> tournaments)
    {
        var activeFlags = await atpTourCrawler.GetActiveFlags();
        var models = await itfTennisParser.Parse(tournaments, activeFlags);

        return Ok(models);
    }

}
