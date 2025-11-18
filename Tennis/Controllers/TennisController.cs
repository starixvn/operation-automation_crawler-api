using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    DataService dataService,
    OperationCrawlerDbContext dbContext
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

    [HttpGet]
    [Route("test")]
    public async Task<IActionResult> Test()
    {
        var countries = await dbContext.CountryCodes.Select(c=>$"({c.EnglishName})").ToListAsync();
        var soccer = await dbContext.Basketball7m.Where(s=>s.english_name.Contains("(")
                                                       && s.english_name.Contains(")")).ToListAsync();
        foreach (var s in soccer)
        {
            var name = s.english_name.Trim();
            foreach (var code in countries)
            {
                if (name.EndsWith(code))
                {
                    name = name.Replace(code, "").Trim();
                    break;
                }
            }

            s.english_name = name;
        }

        await dbContext.SaveChangesAsync();

        return Ok("");
    }
}
