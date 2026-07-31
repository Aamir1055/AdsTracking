using AdsTracking.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AdsTracking.Api.Controllers;

[ApiController]
[Route("api/market")]
public class MarketController : ControllerBase
{
    private readonly MarketDataService _marketService;

    public MarketController(MarketDataService marketService)
    {
        _marketService = marketService;
    }

    [HttpGet("tickers")]
    public async Task<IActionResult> GetTickers()
    {
        var data = await _marketService.GetTickersAsync();
        return Ok(data);
    }
}
