using System.Collections.Generic;

namespace ChangeGuard.Application.ChangeRequests;

public sealed record ReleaseReadinessResponse(
    string ReferenceNumber,
    string Title,
    string Priority,
    string Status,
    int Score,
    bool IsBlocked,
    bool CanMoveToReleaseApproval,
    IReadOnlyList<string> Blockers);