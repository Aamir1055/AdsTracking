using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace AdsTracking.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private static readonly string ValidUsername = "JpTradeBazaar";
    private static readonly string ValidPassword = "*9AJcdasq+LC(!kT4ziX+";

    // Simple token store (in-memory, survives until app restart)
    private static readonly HashSet<string> ValidTokens = new();

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (request.Username == ValidUsername && request.Password == ValidPassword)
        {
            var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            ValidTokens.Add(token);
            return Ok(new { token });
        }

        return Unauthorized(new { message = "Invalid username or password" });
    }

    [HttpGet("verify")]
    public IActionResult Verify([FromHeader(Name = "X-Dashboard-Token")] string? token)
    {
        if (!string.IsNullOrEmpty(token) && ValidTokens.Contains(token))
            return Ok(new { valid = true });

        return Unauthorized(new { valid = false });
    }

    public record LoginRequest(string Username, string Password);
}
