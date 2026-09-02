namespace ChangeGuard.Domain.ChangeRequests;

public sealed record SlaAssessment(
    DateTimeOffset DueUtc,
    bool IsBreached,
    double RemainingHours);
