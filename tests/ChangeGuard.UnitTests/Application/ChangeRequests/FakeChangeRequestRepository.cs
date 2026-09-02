using ChangeGuard.Application.ChangeRequests;
using ChangeGuard.Application.ChangeRequests.Abstractions;
using ChangeGuard.Domain.ChangeRequests;

namespace ChangeGuard.UnitTests.Application.ChangeRequests;

internal sealed class FakeChangeRequestRepository
    : IChangeRequestRepository
{
    private readonly List<ChangeRequest> _requests;

    public FakeChangeRequestRepository(
        params ChangeRequest[] requests)
    {
        _requests = requests.ToList();
    }

    public List<ChangeRequestAuditEntry> AuditEntries { get; } = [];

    public int SaveCount { get; private set; }

    public Task<ChangeRequest?> GetByReferenceNumberAsync(
        string referenceNumber,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_requests.SingleOrDefault(item =>
            item.ReferenceNumber == referenceNumber));
    }

    public Task<ChangeRequest?> GetTrackedByReferenceNumberAsync(
        string referenceNumber,
        CancellationToken cancellationToken = default)
    {
        return GetByReferenceNumberAsync(referenceNumber, cancellationToken);
    }

    public Task<bool> ExistsAsync(
        string referenceNumber,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_requests.Any(item =>
            item.ReferenceNumber == referenceNumber));
    }

    public Task<(IReadOnlyList<ChangeRequest> Items, int TotalCount)> SearchAsync(
        ChangeRequestSearchCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<ChangeRequest> query = _requests;

        if (!string.IsNullOrWhiteSpace(criteria.Search))
        {
            query = query.Where(item =>
                item.ReferenceNumber.Contains(
                    criteria.Search,
                    StringComparison.OrdinalIgnoreCase)
                || item.Title.Contains(
                    criteria.Search,
                    StringComparison.OrdinalIgnoreCase));
        }

        if (criteria.Priority.HasValue)
        {
            query = query.Where(item =>
                item.Priority == criteria.Priority.Value);
        }

        if (criteria.Status.HasValue)
        {
            query = query.Where(item =>
                item.Status == criteria.Status.Value);
        }

        var filtered = query.ToList();
        IReadOnlyList<ChangeRequest> page = filtered
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToList();

        return Task.FromResult((page, filtered.Count));
    }

    public Task<IReadOnlyList<ChangeRequest>> GetDashboardRequestsAsync(
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<ChangeRequest>>(
            _requests.AsReadOnly());
    }

    public Task<IReadOnlyList<ChangeRequestAuditEntry>> GetAuditEntriesAsync(
        Guid changeRequestId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<ChangeRequestAuditEntry>>(
            AuditEntries
                .Where(entry => entry.ChangeRequestId == changeRequestId)
                .ToList()
                .AsReadOnly());
    }

    public Task AddAsync(
        ChangeRequest changeRequest,
        CancellationToken cancellationToken = default)
    {
        _requests.Add(changeRequest);
        return Task.CompletedTask;
    }

    public Task AddAuditEntryAsync(
        ChangeRequestAuditEntry auditEntry,
        CancellationToken cancellationToken = default)
    {
        AuditEntries.Add(auditEntry);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        SaveCount++;
        return Task.CompletedTask;
    }
}
