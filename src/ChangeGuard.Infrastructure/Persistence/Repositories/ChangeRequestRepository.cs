using System;
using System.Threading;
using System.Threading.Tasks;

using ChangeGuard.Application.ChangeRequests.Abstractions;
using ChangeGuard.Application.ChangeRequests;
using ChangeGuard.Domain.ChangeRequests;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ChangeGuard.Infrastructure.Persistence.Repositories;

internal sealed class ChangeRequestRepository
    : IChangeRequestRepository
{
    private readonly ChangeGuardDbContext _dbContext;

    public ChangeRequestRepository(
        ChangeGuardDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ChangeRequest?>
        GetByReferenceNumberAsync(
            string referenceNumber,
            CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            referenceNumber);

        var normalizedReferenceNumber =
            referenceNumber.Trim();

        return await _dbContext.ChangeRequests
            .AsNoTracking()
            .SingleOrDefaultAsync(
                changeRequest =>
                    changeRequest.ReferenceNumber ==
                    normalizedReferenceNumber,
                cancellationToken);
    }

    public async Task<ChangeRequest?>
        GetTrackedByReferenceNumberAsync(
            string referenceNumber,
            CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(referenceNumber);
        var normalizedReferenceNumber = referenceNumber.Trim();

        return await _dbContext.ChangeRequests
            .SingleOrDefaultAsync(
                changeRequest =>
                    changeRequest.ReferenceNumber ==
                    normalizedReferenceNumber,
                cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        string referenceNumber,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(referenceNumber);
        var normalizedReferenceNumber = referenceNumber.Trim();

        return await _dbContext.ChangeRequests
            .AsNoTracking()
            .AnyAsync(
                changeRequest =>
                    changeRequest.ReferenceNumber ==
                    normalizedReferenceNumber,
                cancellationToken);
    }

    public async Task<(IReadOnlyList<ChangeRequest> Items, int TotalCount)>
        SearchAsync(
            ChangeRequestSearchCriteria criteria,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(criteria);
        var query = _dbContext.ChangeRequests.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(criteria.Search))
        {
            var search = criteria.Search.Trim();
            query = query.Where(changeRequest =>
                changeRequest.ReferenceNumber.Contains(search)
                || changeRequest.Title.Contains(search));
        }

        if (criteria.Priority.HasValue)
        {
            query = query.Where(changeRequest =>
                changeRequest.Priority == criteria.Priority.Value);
        }

        if (criteria.Status.HasValue)
        {
            query = query.Where(changeRequest =>
                changeRequest.Status == criteria.Status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(changeRequest => changeRequest.UpdatedUtc)
            .ThenBy(changeRequest => changeRequest.ReferenceNumber)
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToListAsync(cancellationToken);

        return (items.AsReadOnly(), totalCount);
    }

    public async Task<IReadOnlyList<ChangeRequest>>
        GetDashboardRequestsAsync(
            CancellationToken cancellationToken = default)
    {
        var items = await _dbContext.ChangeRequests
            .AsNoTracking()
            .OrderByDescending(changeRequest => changeRequest.UpdatedUtc)
            .ToListAsync(cancellationToken);

        return items.AsReadOnly();
    }

    public async Task<IReadOnlyList<ChangeRequestAuditEntry>>
        GetAuditEntriesAsync(
            Guid changeRequestId,
            CancellationToken cancellationToken = default)
    {
        var entries = await _dbContext.ChangeRequestAuditEntries
            .AsNoTracking()
            .Where(entry => entry.ChangeRequestId == changeRequestId)
            .OrderByDescending(entry => entry.OccurredUtc)
            .ToListAsync(cancellationToken);

        return entries.AsReadOnly();
    }

    public async Task AddAsync(
        ChangeRequest changeRequest,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(changeRequest);

        await _dbContext.ChangeRequests.AddAsync(
            changeRequest,
            cancellationToken);
    }

    public async Task AddAuditEntryAsync(
        ChangeRequestAuditEntry auditEntry,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(auditEntry);

        await _dbContext.ChangeRequestAuditEntries.AddAsync(
            auditEntry,
            cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (IsUniqueConstraintViolation(exception))
        {
            var referenceNumber = exception.Entries
                .Select(entry => entry.Entity)
                .OfType<ChangeRequest>()
                .Select(changeRequest => changeRequest.ReferenceNumber)
                .FirstOrDefault()
                ?? "unknown";

            throw new DuplicateChangeRequestException(
                referenceNumber,
                exception);
        }
    }

    private static bool IsUniqueConstraintViolation(
        DbUpdateException exception)
    {
        return exception.InnerException is SqlException
        {
            Number: 2601 or 2627
        };
    }
}
