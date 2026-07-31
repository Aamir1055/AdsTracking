using AdsTracking.Api.Data;
using AdsTracking.Api.Models;
using AdsTracking.Api.Infrastructure;
using Dapper;

namespace AdsTracking.Api.Services;

public class DownloadService
{
    private readonly DbConnectionFactory _dbFactory;
    private readonly RetryQueue _retryQueue;
    private readonly ILogger<DownloadService> _logger;
    private readonly IConfiguration _config;

    public DownloadService(DbConnectionFactory dbFactory, RetryQueue retryQueue, ILogger<DownloadService> logger, IConfiguration config)
    {
        _dbFactory = dbFactory;
        _retryQueue = retryQueue;
        _logger = logger;
        _config = config;
    }

    public async Task<string> LogDownloadAndGetRedirectUrlAsync(string ipAddress, string userAgent)
    {
        var downloadEvent = new DownloadEvent
        {
            TimestampUtc = DateTime.UtcNow,
            IpAddress = ipAddress,
            UserAgent = userAgent.Length > 1024 ? userAgent[..1024] : userAgent
        };

        try
        {
            using var connection = _dbFactory.CreateConnection();
            await connection.ExecuteAsync(@"
                INSERT INTO download_events (timestamp_utc, ip_address, user_agent)
                VALUES (@TimestampUtc, @IpAddress, @UserAgent)",
                downloadEvent);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "DB unavailable, queuing download event write.");
            _retryQueue.Enqueue(new RetryItem
            {
                Type = RetryItemType.DownloadEvent,
                Payload = downloadEvent,
                MaxRetries = 3,
                RetryInterval = TimeSpan.FromSeconds(20)
            });
        }

        return _config["DownloadUrl"] ?? "http://android.tradekaro.com/";
    }
}
