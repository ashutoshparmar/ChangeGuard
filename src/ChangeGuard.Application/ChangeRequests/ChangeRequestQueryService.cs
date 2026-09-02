using ChangeGuard.Application.ChangeRequests.Abstractions;
using ChangeGuard.Domain.ChangeRequests;

namespace ChangeGuard.Application.ChangeRequests;

public sealed class ChangeRequestQueryService
    : IChangeRequestQueryService
{
    private readonly IChangeRequestRepository _repository;

    public ChangeRequestQueryService(
        IChangeRequestRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResponse<ChangeRequestSummaryResponse>> SearchAsync(
        ChangeRequestSearchCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        if (criteria.Page < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(criteria),
                "Page must be at least 1.");
        }

        if (criteria.PageSize is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(criteria),
                "Page size must be between 1 and 100.");
        }

        var (items, totalCount) = await _repository.SearchAsync(
            criteria,
            cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var responses = items
            .Select(item => ChangeRequestResponseMapper.ToSummary(item, now))
            .ToList()
            .AsReadOnly();

        return new PagedResponse<ChangeRequestSummaryResponse>(
            responses,
            criteria.Page,
            criteria.PageSize,
            totalCount);
    }

    public async Task<ChangeRequestDetailsResponse> GetByReferenceNumberAsync(
        string referenceNumber,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(referenceNumber);

        var changeRequest = await _repository.GetByReferenceNumberAsync(
            referenceNumber,
            cancellationToken);

        if (changeRequest is null)
        {
            throw new ChangeRequestNotFoundException(referenceNumber);
        }

        return ChangeRequestResponseMapper.ToDetails(
            changeRequest,
            DateTimeOffset.UtcNow);
    }

    public async Task<IReadOnlyList<ChangeRequestAuditResponse>> GetAuditAsync(
        string referenceNumber,
        CancellationToken cancellationToken = default)
    {
        var changeRequest = await _repository.GetByReferenceNumberAsync(
            referenceNumber,
            cancellationToken);

        if (changeRequest is null)
        {
            throw new ChangeRequestNotFoundException(referenceNumber);
        }

        var entries = await _repository.GetAuditEntriesAsync(
            changeRequest.Id,
            cancellationToken);

        return entries
            .Select(entry => new ChangeRequestAuditResponse(
                entry.Id,
                entry.Action,
                entry.Actor,
                entry.Comment,
                entry.FromStatus?.ToString(),
                entry.ToStatus.ToString(),
                entry.OccurredUtc))
            .ToList()
            .AsReadOnly();
    }

    public async Task<DashboardResponse> GetDashboardAsync(
        CancellationToken cancellationToken = default)
    {
        var requests = await _repository.GetDashboardRequestsAsync(
            cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var summaries = requests
            .Select(item => ChangeRequestResponseMapper.ToSummary(item, now))
            .ToList();
        var completedStatuses = new[]
        {
            ChangeRequestStatus.Released.ToString(),
            ChangeRequestStatus.Rejected.ToString(),
            ChangeRequestStatus.Closed.ToString()
        };

        return new DashboardResponse(
            TotalRequests: summaries.Count,
            ActiveRequests: summaries.Count(item =>
                !completedStatuses.Contains(item.Status)),
            BlockedRequests: summaries.Count(item =>
                !completedStatuses.Contains(item.Status)
                && item.IsBlocked),
            SlaBreachedRequests: summaries.Count(item => item.IsSlaBreached),
            ByStatus: summaries
                .GroupBy(item => item.Status)
                .OrderBy(group => group.Key)
                .Select(group => new BreakdownItemResponse(
                    group.Key,
                    group.Count()))
                .ToList()
                .AsReadOnly(),
            ByPriority: summaries
                .GroupBy(item => item.Priority)
                .OrderByDescending(group => group.Count())
                .Select(group => new BreakdownItemResponse(
                    group.Key,
                    group.Count()))
                .ToList()
                .AsReadOnly(),
            RecentRequests: summaries
                .OrderByDescending(item => item.UpdatedUtc)
                .Take(5)
                .ToList()
                .AsReadOnly());
    }
}
