using Microsoft.AspNetCore.Mvc;
using Tennis.Storage;

namespace Tennis.Controllers;

[Route("api/tennis-storage")]
[ApiController]

public class TennisStorageController(TennisDbContext dbContext) : ControllerBase
{
    [HttpPost]
    [Route("create-league")]
    public async Task<IActionResult> CreateLeague(TennisLeague league)
    {
        league.UpdatedDate = DateTime.Now;
        await dbContext.TennisLeagues.AddAsync(league);
        await dbContext.SaveChangesAsync();

        return Ok(league);
    }

    [HttpPost]
    [Route("create-match")]
    public async Task<IActionResult> CreateMatch(TennisMatch match)
    {
        match.UpdatedDate = DateTime.Now;
        await dbContext.TennisMatches.AddAsync(match);
        await dbContext.SaveChangesAsync();

        return Ok(match);
    }
}
