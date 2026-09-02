namespace ChangeGuard.Application.ChangeRequests;

public sealed record DashboardResponse(
    int TotalRequests,
    int ActiveRequests,
    int BlockedRequests,
    int SlaBreachedRequests,
    IReadOnlyList<BreakdownItemResponse> ByStatus,
    IReadOnlyList<BreakdownItemResponse> ByPriority,
    IReadOnlyList<ChangeRequestSummaryResponse> RecentRequests);

public sealed record BreakdownItemResponse(
    string Name,
    int Count);
