using Microsoft.AspNetCore.Mvc;
using Tennis.Models.ItfTennisModels;
using Tennis.Storage;

namespace Tennis.Controllers;

[Route("api/[controller]")]
[ApiController]

public class TennisController(
    AtpTourCrawler atpTourCrawler,
    ItfTennisParser itfTennisParser,
    TennisDataService dataService,
    TriggerN8NWorkflow triggerWorkflow
) : ControllerBase
{
    [HttpGet]
    [Route("atp-tour")]
    public async Task<IActionResult> AtpTour()
    {
        var leagues = await atpTourCrawler.CrawlAll();
        await dataService.SyncLeagues(leagues);
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
}
