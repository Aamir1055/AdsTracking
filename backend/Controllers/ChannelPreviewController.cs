using AdsTracking.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AdsTracking.Api.Controllers;

[ApiController]
[Route("api/channel")]
public class ChannelPreviewController : ControllerBase
{
    private readonly TelegramMessagesService _messagesService;

    public ChannelPreviewController(TelegramMessagesService messagesService)
    {
        _messagesService = messagesService;
    }

    [HttpGet("messages")]
    public async Task<IActionResult> GetRecentMessages([FromQuery] int count = 10)
    {
        count = Math.Min(count, 20);
        var messages = await _messagesService.GetRecentMessagesAsync(count);
        return Ok(messages);
    }

    [HttpGet("latest-ids")]
    public async Task<IActionResult> GetLatestMessageIds([FromQuery] int count = 10)
    {
        count = Math.Min(count, 20);
        var ids = await _messagesService.GetRecentMessageIdsAsync(count);
        return Ok(ids);
    }

    [HttpGet("highest-id")]
    public async Task<IActionResult> GetHighestMessageId()
    {
        var ids = await _messagesService.GetRecentMessageIdsAsync(50);
        var highest = ids.Count > 0 ? ids.Max() : 20;
        return Ok(new { highestId = highest });
    }
}
