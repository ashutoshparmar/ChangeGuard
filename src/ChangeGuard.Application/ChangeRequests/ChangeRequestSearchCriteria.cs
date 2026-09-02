using ChangeGuard.Domain.ChangeRequests;

namespace ChangeGuard.Application.ChangeRequests;

public sealed record ChangeRequestSearchCriteria(
    string? Search,
    ChangePriority? Priority,
    ChangeRequestStatus? Status,
    int Page = 1,
    int PageSize = 20);
