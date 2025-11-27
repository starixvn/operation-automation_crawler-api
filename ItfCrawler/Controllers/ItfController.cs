using ItfCrawler.Storage;
using Microsoft.AspNetCore.Mvc;

namespace ItfCrawler.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItfController(ItfStorage storage) : ControllerBase
{
    
    /// <summary>
    /// Create or update a league
    /// </summary>
    /// <param name="league">League data</param>
    /// <returns>Success message</returns>
    [HttpPost("leagues")]
    public async Task<IActionResult> CreateOrUpdateLeague([FromBody] ItfLeague league)
    {
        if (string.IsNullOrEmpty(league.Id))
        {
            return BadRequest(new { message = "League ID is required" });
        }

        await storage.CreateOrUpdateLeague(league);
        return Ok(new { message = "League created/updated successfully", leagueId = league.Id });
    }

    /// <summary>
    /// Create or update a match
    /// </summary>
    /// <param name="match">Match data</param>
    /// <returns>Success message</returns>
    [HttpPost("matches")]
    public async Task<IActionResult> CreateOrUpdateMatch([FromBody] ItfMatch match)
    {
        if (match.Id == 0)
        {
            return BadRequest(new { message = "Match ID is required" });
        }

        await storage.CreateOrUpdateMatch(match);
        return Ok(new { message = "Match created/updated successfully", matchId = match.Id });
    }

    /// <summary>
    /// Create or update a player
    /// </summary>
    /// <param name="player">Player data</param>
    /// <returns>Success message</returns>
    [HttpPost("players")]
    public async Task<IActionResult> CreateOrUpdatePlayer([FromBody] ItfPlayer player)
    {
        if (player.Id == 0)
        {
            return BadRequest(new { message = "Player ID is required" });
        }

        await storage.CreateOrUpdatePlayer(player);
        return Ok(new { message = "Player created/updated successfully", playerId = player.Id });
    }

    /// <summary>
    /// Get a player by name and country code
    /// </summary>
    /// <param name="name">Player name</param>
    /// <param name="countryCode">Country code</param>
    /// <returns>Player data or not found</returns>
    [HttpGet("players")]
    public async Task<IActionResult> GetPlayer([FromQuery] string name, [FromQuery] string countryCode)
    {
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(countryCode))
        {
            return BadRequest(new { message = "Both name and countryCode are required" });
        }

        var player = await storage.GetPlayer(name, countryCode);
        
        if (player == null)
        {
            return NotFound(new { message = "Player not found" });
        }

        return Ok(player);
    }
}