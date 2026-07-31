namespace AdsTracking.Api.Models;

public class TrackingEvent
{
    public long Id { get; set; }
    public string VisitorId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string? EventData { get; set; }
    public string PageUrl { get; set; } = string.Empty;
    public DateTime TimestampUtc { get; set; }
    public DateTime CreatedAt { get; set; }
}
