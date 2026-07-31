using System.Text.Json.Serialization;

namespace AdsTracking.Api.DTOs;

// Sent on first page load — registers visitor
public record RegisterVisitorRequest(
    [property: JsonPropertyName("visitorId")] string VisitorId,
    [property: JsonPropertyName("utmSource")] string? UtmSource,
    [property: JsonPropertyName("utmMedium")] string? UtmMedium,
    [property: JsonPropertyName("utmCampaign")] string? UtmCampaign,
    [property: JsonPropertyName("utmTerm")] string? UtmTerm,
    [property: JsonPropertyName("utmContent")] string? UtmContent,
    [property: JsonPropertyName("fbclid")] string? Fbclid,
    [property: JsonPropertyName("timestamp")] DateTime? Timestamp
);

// Sent on every page load
public record TrackPageViewRequest(
    [property: JsonPropertyName("visitorId")] string VisitorId,
    [property: JsonPropertyName("pageUrl")] string PageUrl,
    [property: JsonPropertyName("pageTitle")] string? PageTitle,
    [property: JsonPropertyName("enteredAt")] DateTime EnteredAt
);

// Sent when user leaves a page (beforeunload / visibilitychange)
public record UpdateTimeOnPageRequest(
    [property: JsonPropertyName("visitorId")] string VisitorId,
    [property: JsonPropertyName("pageUrl")] string PageUrl,
    [property: JsonPropertyName("enteredAt")] DateTime EnteredAt,
    [property: JsonPropertyName("timeOnPageSeconds")] int TimeOnPageSeconds
);

// Sent on clicks, navigations, etc.
public record TrackEventRequest(
    [property: JsonPropertyName("visitorId")] string VisitorId,
    [property: JsonPropertyName("eventType")] string EventType,
    [property: JsonPropertyName("eventData")] string? EventData,
    [property: JsonPropertyName("pageUrl")] string? PageUrl,
    [property: JsonPropertyName("timestamp")] DateTime Timestamp
);
