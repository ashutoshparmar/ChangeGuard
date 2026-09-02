using System.Collections.Generic;

namespace ChangeGuard.Domain.ChangeRequests;

public sealed record ReleaseReadinessAssessment(
    int Score,
    IReadOnlyList<string> Blockers)
{
    public bool IsBlocked => Blockers.Count > 0;

    public bool CanMoveToReleaseApproval => !IsBlocked;
}