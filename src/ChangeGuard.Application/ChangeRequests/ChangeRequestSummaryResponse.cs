namespace ChangeGuard.Application.ChangeRequests;

public sealed record ChangeRequestSummaryResponse(
    Guid Id,
    string ReferenceNumber,
    string Title,
    string Priority,
    string Status,
    int ReadinessScore,
    bool IsBlocked,
    DateTimeOffset SlaDueUtc,
    bool IsSlaBreached,
    DateTimeOffset UpdatedUtc);
