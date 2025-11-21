using Domain.Models;
using Domain.Models.LiveTennis;
using Domain.Models.TeamModels;
using Microsoft.AspNetCore.Mvc;
using Translator.Services;
using Translator.Storage;

namespace Translator.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TeamController(
    CheckTeamNameService teamService,
    DataService dataService) : ControllerBase
{
    // GET: /api/team-name?name=Liverpool&sportTypeId=1
    [HttpGet("check-exist")]
    public async Task<IActionResult> CheckTeamName([FromQuery] string name, [FromQuery] int sportTypeId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest(new CheckTeamNameResult
            {
                IsExist = false,
                Message = "Parameter 'name' is required"
            });
        }

        var result = await teamService.GetTeam(name, sportTypeId);

        return Ok(result);
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