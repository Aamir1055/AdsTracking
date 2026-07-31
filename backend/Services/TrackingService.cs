using AdsTracking.Api.Data;
using AdsTracking.Api.DTOs;
using AdsTracking.Api.Models;
using Dapper;
using MySqlConnector;

namespace AdsTracking.Api.Services;

public class TrackingService
{
    private readonly DbConnectionFactory _dbFactory;
    private readonly ILogger<TrackingService> _logger;

    public TrackingService(DbConnectionFactory dbFactory, ILogger<TrackingService> logger)
    {
        _dbFactory = dbFactory;
        _logger = logger;
    }

    public async Task<bool> RegisterVisitorAsync(RegisterVisitorRequest request, string ipAddress, string userAgent)
    {
        if (string.IsNullOrWhiteSpace(request.VisitorId) || !Guid.TryParse(request.VisitorId, out _))
            return false;

        var firstSeen = request.Timestamp.HasValue
            ? request.Timestamp.Value.ToUniversalTime()
            : DateTime.UtcNow;

        try
        {
            using var connection = _dbFactory.CreateConnection();
            // INSERT IGNORE — if visitor already exists, do nothing
            await connection.ExecuteAsync(@"
                INSERT IGNORE INTO visitors (visitor_id, first_seen_utc, utm_source, utm_medium, utm_campaign, utm_term, utm_content, fbclid, ip_address)
                VALUES (@VisitorId, @FirstSeen, @UtmSource, @UtmMedium, @UtmCampaign, @UtmTerm, @UtmContent, @Fbclid, @IpAddress)",
                new
                {
                    VisitorId = request.VisitorId,
                    FirstSeen = firstSeen,
                    UtmSource = Truncate(request.UtmSource ?? "", 256),
                    UtmMedium = Truncate(request.UtmMedium ?? "", 256),
                    UtmCampaign = Truncate(request.UtmCampaign ?? "", 256),
                    UtmTerm = Truncate(request.UtmTerm ?? "", 256),
                    UtmContent = Truncate(request.UtmContent ?? "", 256),
                    Fbclid = Truncate(request.Fbclid ?? "", 512),
                    IpAddress = ipAddress
                });
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register visitor {VisitorId}", request.VisitorId);
            return false;
        }
    }

    public async Task<bool> TrackPageViewAsync(TrackPageViewRequest request, string ipAddress, string userAgent)
    {
        if (string.IsNullOrWhiteSpace(request.VisitorId))
            return false;

        try
        {
            using var connection = _dbFactory.CreateConnection();
            await connection.ExecuteAsync(@"
                INSERT INTO page_views (visitor_id, page_url, page_title, entered_at_utc, ip_address, user_agent)
                VALUES (@VisitorId, @PageUrl, @PageTitle, @EnteredAt, @IpAddress, @UserAgent)",
                new
                {
                    VisitorId = request.VisitorId,
                    PageUrl = Truncate(request.PageUrl ?? "", 2048),
                    PageTitle = Truncate(request.PageTitle ?? "", 512),
                    EnteredAt = request.EnteredAt.ToUniversalTime(),
                    IpAddress = ipAddress,
                    UserAgent = Truncate(userAgent, 1024)
                });
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to track page view for {VisitorId}", request.VisitorId);
            return false;
        }
    }

    public async Task<bool> UpdateTimeOnPageAsync(UpdateTimeOnPageRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.VisitorId))
            return false;

        try
        {
            using var connection = _dbFactory.CreateConnection();
            // Update the most recent page_view matching visitor + page + entered time
            await connection.ExecuteAsync(@"
                UPDATE page_views 
                SET time_on_page_seconds = @TimeOnPage
                WHERE visitor_id = @VisitorId 
                  AND page_url = @PageUrl
                  AND entered_at_utc = @EnteredAt
                ORDER BY id DESC
                LIMIT 1",
                new
                {
                    VisitorId = request.VisitorId,
                    PageUrl = request.PageUrl,
                    EnteredAt = request.EnteredAt.ToUniversalTime(),
                    TimeOnPage = request.TimeOnPageSeconds
                });
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update time on page for {VisitorId}", request.VisitorId);
            return false;
        }
    }

    public async Task<bool> TrackEventAsync(TrackEventRequest request, string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(request.VisitorId) || string.IsNullOrWhiteSpace(request.EventType))
            return false;

        try
        {
            using var connection = _dbFactory.CreateConnection();
            await connection.ExecuteAsync(@"
                INSERT INTO events (visitor_id, event_type, event_data, page_url, timestamp_utc)
                VALUES (@VisitorId, @EventType, @EventData, @PageUrl, @TimestampUtc)",
                new
                {
                    VisitorId = request.VisitorId,
                    EventType = request.EventType,
                    EventData = request.EventData,
                    PageUrl = request.PageUrl ?? "",
                    TimestampUtc = request.Timestamp.ToUniversalTime()
                });
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to track event {EventType} for {VisitorId}", request.EventType, request.VisitorId);
            return false;
        }
    }

    private static string Truncate(string value, int maxLength)
        => value.Length <= maxLength ? value : value[..maxLength];
}
