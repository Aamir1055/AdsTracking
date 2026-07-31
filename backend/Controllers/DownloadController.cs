using AdsTracking.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AdsTracking.Api.Controllers;

[ApiController]
[Route("download")]
public class DownloadController : ControllerBase
{
    private readonly DownloadService _downloadService;

    public DownloadController(DownloadService downloadService)
    {
        _downloadService = downloadService;
    }

    [HttpGet]
    public async Task<IActionResult> Download()
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = Request.Headers.UserAgent.ToString();

        var redirectUrl = await _downloadService.LogDownloadAndGetRedirectUrlAsync(ipAddress, userAgent);
        return Redirect(redirectUrl);
    }
}
