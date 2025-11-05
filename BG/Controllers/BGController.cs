using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace BG.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BGController(HttpClient client, IConfiguration configuration) : ControllerBase
{
    private readonly string? BGSoccerUrl = configuration.GetValue<string>("BGUrl");
    [HttpGet]
    [Route("bg-soccer")]
    public async Task<IActionResult> BGAutoDownload()
    {
        var url = $"{BGSoccerUrl}/soccer";
        var response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            return BadRequest(response.Content.ReadAsStringAsync());
        }
        var content = await response.Content.ReadAsStringAsync();
        return Ok(content);
    }

    [HttpGet]
    [Route("bg-other-sports")]
    public async Task<IActionResult> BGOtherSportsDownload()
    {
        var url = $"{BGSoccerUrl}/other-sports";
        var response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            return BadRequest(response.Content.ReadAsStringAsync());
        }
        var content = await response.Content.ReadAsStringAsync();
        return Ok(content);
    }
}
