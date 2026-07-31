namespace AdsTracking.Api.Models;

public class PageView
{
    public long Id { get; set; }
    public string VisitorId { get; set; } = string.Empty;
    public string PageUrl { get; set; } = string.Empty;
    public string PageTitle { get; set; } = string.Empty;
    public DateTime EnteredAtUtc { get; set; }
    public int TimeOnPageSeconds { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
