using System.Threading;
using System.Threading.Tasks;

using ChangeGuard.Application.ChangeRequests;
using ChangeGuard.Domain.ChangeRequests;

namespace ChangeGuard.Application.ChangeRequests.Abstractions;

public interface IChangeRequestRepository
{
    Task<ChangeRequest?> GetByReferenceNumberAsync(
        string referenceNumber,
        CancellationToken cancellationToken = default);

    Task<ChangeRequest?> GetTrackedByReferenceNumberAsync(
        string referenceNumber,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        string referenceNumber,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<ChangeRequest> Items, int TotalCount)> SearchAsync(
        ChangeRequestSearchCriteria criteria,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ChangeRequest>> GetDashboardRequestsAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ChangeRequestAuditEntry>> GetAuditEntriesAsync(
        Guid changeRequestId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        ChangeRequest changeRequest,
        CancellationToken cancellationToken = default);

    Task AddAuditEntryAsync(
        ChangeRequestAuditEntry auditEntry,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
