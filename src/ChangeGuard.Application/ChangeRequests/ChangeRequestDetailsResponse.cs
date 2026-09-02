namespace ChangeGuard.Application.ChangeRequests;

public sealed record ChangeRequestDetailsResponse(
    Guid Id,
    string ReferenceNumber,
    string Title,
    string Description,
    string Priority,
    string Status,
    bool HasQaEvidence,
    string? QaEvidenceNotes,
    bool HasRollbackPlan,
    string? RollbackPlan,
    int ReadinessScore,
    bool IsBlocked,
    bool CanMoveToReleaseApproval,
    IReadOnlyList<string> Blockers,
    DateTimeOffset CreatedUtc,
    DateTimeOffset UpdatedUtc,
    DateTimeOffset SlaDueUtc,
    bool IsSlaBreached,
    double RemainingSlaHours);
