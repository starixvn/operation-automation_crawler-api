using Microsoft.AspNetCore.Mvc;
using Tennis.Models.TeamModels;
using Tennis.Services;

namespace Tennis.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TeamController(CheckTeamNameService teamService) : ControllerBase
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
}