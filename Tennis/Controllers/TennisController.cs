using Microsoft.AspNetCore.Mvc;
using Tennis.Models;
using Tennis.Models.ItfTennisModels;
using Tennis.Models.LiveTennis;
using Tennis.Storage;

namespace Tennis.Controllers;

[Route("api/[controller]")]
[ApiController]

public class TennisController(
    AtpTourCrawler atpTourCrawler,
    ItfTennisParser itfTennisParser,
    TriggerN8NWorkflow triggerWorkflow,
    DataService dataService
    ) : ControllerBase
{
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
            Console.WriteLine($"Found {matches.Count} matches for league {league.ScheduleUrl}");
            league.Matches = matches.Where(m => !string.IsNullOrEmpty(m.Home))
                .Where(m => !string.IsNullOrEmpty(m.Away))
            .ToList();
        }

        await triggerWorkflow.CallWorkFlowForATP(leagues);

        return Ok(leagues);
    }


    [HttpPost]
    [Route("itf-tennis")]
    public async Task<IActionResult> ItfTennis(List<ItfTennisLeague> tournaments)
    {
        var activeFlags = await atpTourCrawler.GetActiveFlags();
        var models = await itfTennisParser.Parse(tournaments, activeFlags);
        await triggerWorkflow.CallWorkFlowForITF(models);
        return Ok(models);
    }

    [HttpPost]
    [Route("live-tennis")]
    public async Task<IActionResult> LiveTennis(List<LiveTennisPlayer> players)
    {
        var playersToAdd = await dataService.UpdateLiveTennis(players);
        return Ok(playersToAdd);
    }

    [HttpPost]
    [Route("soccer-7m")]
    public async Task<IActionResult> Soccer(List<Soccer7m> players)
    {
        var playersToAdd = await dataService.UpdateSoccer7m(players);
        return Ok(playersToAdd);
    }

    [HttpPost]
    [Route("basketball-7m")]
    public async Task<IActionResult> Basketball(List<Basketball7m> players)
    {
        var playersToAdd = await dataService.UpdateBasketball7m(players);

        return Ok(playersToAdd);
    }

    [HttpPost]
    [Route("badminton")]
    public async Task<IActionResult> Badminton(List<Badminton> players)
    {
        var playersToAdd = await dataService.UpdateBadminton(players);

        return Ok(playersToAdd);
    }
}
