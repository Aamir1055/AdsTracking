namespace AdsTracking.Api.Models;

public class Visitor
{
    public long Id { get; set; }
    public string VisitorId { get; set; } = string.Empty;
    public DateTime FirstSeenUtc { get; set; }
    public string UtmSource { get; set; } = string.Empty;
    public string UtmMedium { get; set; } = string.Empty;
    public string UtmCampaign { get; set; } = string.Empty;
    public string UtmTerm { get; set; } = string.Empty;
    public string UtmContent { get; set; } = string.Empty;
    public string Fbclid { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
