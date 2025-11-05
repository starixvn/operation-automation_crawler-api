using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    OperationCrawlerDbContext dbContext
    ) : ControllerBase
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
        var existingIds = await dbContext.LiveTennisPlayers
            .Where(p => players.Select(np => np.id).Contains(p.id))
            .Select(p => p.id)
            .ToListAsync();

        var playersToAdd = players
            .Where(p => !existingIds.Contains(p.id))
            .ToList();

        var playersToUpdate = players
            .Where(p => existingIds.Contains(p.id))
            .ToList();

        if (playersToAdd.Any())
        {
            await dbContext.LiveTennisPlayers.AddRangeAsync(playersToAdd);
            await dbContext.SaveChangesAsync();
        }

        if (playersToUpdate.Any())
        {
            dbContext.LiveTennisPlayers.UpdateRange(playersToUpdate);
            await dbContext.SaveChangesAsync();
        }

        return Ok(playersToAdd);
    }

}
