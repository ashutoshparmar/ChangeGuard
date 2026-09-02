namespace ChangeGuard.Application.ChangeRequests;

public sealed record ChangeRequestAuditResponse(
    Guid Id,
    string Action,
    string Actor,
    string Comment,
    string? FromStatus,
    string ToStatus,
    DateTimeOffset OccurredUtc);
