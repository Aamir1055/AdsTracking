using AdsTracking.Api.Data;
using AdsTracking.Api.Models;
using Dapper;

namespace AdsTracking.Api.Infrastructure;

public class RetryQueueService : BackgroundService
{
    private readonly RetryQueue _queue;
    private readonly DbConnectionFactory _dbFactory;
    private readonly ILogger<RetryQueueService> _logger;

    public RetryQueueService(RetryQueue queue, DbConnectionFactory dbFactory, ILogger<RetryQueueService> logger)
    {
        _queue = queue;
        _dbFactory = dbFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var item in _queue.Reader.ReadAllAsync(stoppingToken))
        {
            var success = false;
            for (int attempt = 0; attempt < item.MaxRetries && !success; attempt++)
            {
                try
                {
                    await Task.Delay(item.RetryInterval, stoppingToken);
                    success = await TryWriteAsync(item);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Retry attempt {Attempt} failed for {Type}.", attempt + 1, item.Type);
                }
            }

            if (!success)
            {
                _logger.LogError("Retry exhausted for {Type}. Discarding item.", item.Type);
            }
        }
    }

    private async Task<bool> TryWriteAsync(RetryItem item)
    {
        using var connection = _dbFactory.CreateConnection();

        switch (item.Type)
        {
            case RetryItemType.DownloadEvent:
                var download = (DownloadEvent)item.Payload;
                await connection.ExecuteAsync(@"
                    INSERT INTO download_events (timestamp_utc, ip_address, user_agent)
                    VALUES (@TimestampUtc, @IpAddress, @UserAgent)",
                    download);
                break;
        }

        _logger.LogInformation("Successfully retried write for {Type}.", item.Type);
        return true;
    }
}
