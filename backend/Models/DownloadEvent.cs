namespace AdsTracking.Api.Models;

public class DownloadEvent
{
    public long Id { get; set; }
    public DateTime TimestampUtc { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
