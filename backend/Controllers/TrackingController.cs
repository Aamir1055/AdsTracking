using AdsTracking.Api.DTOs;
using AdsTracking.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AdsTracking.Api.Controllers;

[ApiController]
[Route("api/track")]
public class TrackingController : ControllerBase
{
    private readonly TrackingService _trackingService;

    public TrackingController(TrackingService trackingService)
    {
        _trackingService = trackingService;
    }

    /// <summary>
    /// Register a new visitor (called once per new visitor)
    /// </summary>
    [HttpPost("visitor")]
    public async Task<IActionResult> RegisterVisitor([FromBody] RegisterVisitorRequest request)
    {
        var ip = GetClientIp();
        var ua = Request.Headers.UserAgent.ToString();

        var success = await _trackingService.RegisterVisitorAsync(request, ip, ua);
        return success ? Ok(new { status = "ok" }) : BadRequest(new { error = "Invalid visitor data" });
    }

    /// <summary>
    /// Track a page view (called on every page load)
    /// </summary>
    [HttpPost("pageview")]
    public async Task<IActionResult> TrackPageView([FromBody] TrackPageViewRequest request)
    {
        var ip = GetClientIp();
        var ua = Request.Headers.UserAgent.ToString();

        var success = await _trackingService.TrackPageViewAsync(request, ip, ua);
        return success ? Ok(new { status = "ok" }) : BadRequest(new { error = "Invalid page view data" });
    }

    /// <summary>
    /// Update time spent on a page (called on page unload / visibility change)
    /// </summary>
    [HttpPost("timeOnPage")]
    public async Task<IActionResult> UpdateTimeOnPage([FromBody] UpdateTimeOnPageRequest request)
    {
        var success = await _trackingService.UpdateTimeOnPageAsync(request);
        return success ? Ok(new { status = "ok" }) : BadRequest(new { error = "Invalid time data" });
    }

    /// <summary>
    /// Track an event (click, navigation, telegram_join, etc.)
    /// </summary>
    [HttpPost("event")]
    public async Task<IActionResult> TrackEvent([FromBody] TrackEventRequest request)
    {
        var ip = GetClientIp();

        var success = await _trackingService.TrackEventAsync(request, ip);
        return success ? Ok(new { status = "ok" }) : BadRequest(new { error = "Invalid event data" });
    }

    private string GetClientIp()
    {
        // Check forwarded headers first (for reverse proxy/production)
        var forwarded = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(forwarded))
        {
            return forwarded.Split(',')[0].Trim();
        }

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        // Convert ::1 (IPv6 loopback) to 127.0.0.1 for readability
        if (ip == "::1") ip = "127.0.0.1";
        return ip;
    }
}
