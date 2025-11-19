using Microsoft.AspNetCore.Mvc;

namespace OperationCrawlerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BgController : ControllerBase
{
    private static readonly HttpClient HttpClient = new();

    [Route("status")]
    [HttpGet]
    public async Task<IActionResult> GetStatus()
    {
        var response = await HttpClient.GetStringAsync("https://bg-download.klc.nexdev.net");
        return Ok(response);
    }

    [Route("run")]
    [HttpGet]
    public async Task<IActionResult> Run()
    {
        var response = await HttpClient.GetStringAsync("https://bg-download.klc.nexdev.net/api/run/all");
        return Ok(response);
    }

}