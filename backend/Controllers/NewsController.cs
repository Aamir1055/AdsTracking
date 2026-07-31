using AdsTracking.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AdsTracking.Api.Controllers;

[ApiController]
[Route("api/news")]
public class NewsController : ControllerBase
{
    private readonly NewsService _newsService;

    public NewsController(NewsService newsService)
    {
        _newsService = newsService;
    }

    [HttpGet]
    public async Task<IActionResult> GetNews([FromQuery] int count = 6)
    {
        var news = await _newsService.GetMarketNewsAsync(count);
        return Ok(news);
    }
}
