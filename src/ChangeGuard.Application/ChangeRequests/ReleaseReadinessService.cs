using System;
using System.Threading;
using System.Threading.Tasks;

using ChangeGuard.Application.ChangeRequests.Abstractions;

namespace ChangeGuard.Application.ChangeRequests;

public sealed class ReleaseReadinessService
    : IReleaseReadinessService
{
    private readonly IChangeRequestRepository
        _changeRequestRepository;

    public ReleaseReadinessService(
        IChangeRequestRepository changeRequestRepository)
    {
        _changeRequestRepository =
            changeRequestRepository;
    }

    public async Task<ReleaseReadinessResponse?>
        GetReleaseReadinessAsync(
            string referenceNumber,
            CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            referenceNumber);

        var changeRequest =
            await _changeRequestRepository
                .GetByReferenceNumberAsync(
                    referenceNumber,
                    cancellationToken);

        if (changeRequest is null)
        {
            return null;
        }

        var assessment =
            changeRequest.AssessReleaseReadiness();

        return new ReleaseReadinessResponse(
            ReferenceNumber:
                changeRequest.ReferenceNumber,
            Title:
                changeRequest.Title,
            Priority:
                changeRequest.Priority.ToString(),
            Status:
                changeRequest.Status.ToString(),
            Score:
                assessment.Score,
            IsBlocked:
                assessment.IsBlocked,
            CanMoveToReleaseApproval:
                assessment.CanMoveToReleaseApproval,
            Blockers:
                assessment.Blockers);
    }
}