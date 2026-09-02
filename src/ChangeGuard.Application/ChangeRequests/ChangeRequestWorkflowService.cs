using ChangeGuard.Application.ChangeRequests.Abstractions;
using ChangeGuard.Domain.ChangeRequests;

namespace ChangeGuard.Application.ChangeRequests;

public sealed class ChangeRequestWorkflowService
    : IChangeRequestWorkflowService
{
    private readonly IChangeRequestRepository _repository;

    public ChangeRequestWorkflowService(
        IChangeRequestRepository repository)
    {
        _repository = repository;
    }

    public async Task<ChangeRequestDetailsResponse> ApplyActionAsync(
        string referenceNumber,
        ChangeRequestWorkflowCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        var changeRequest = await GetTrackedAsync(
            referenceNumber,
            cancellationToken);
        var fromStatus = changeRequest.Status;

        switch (command.Action)
        {
            case ChangeRequestWorkflowAction.SubmitForReview:
                changeRequest.SubmitForRequirementReview();
                break;
            case ChangeRequestWorkflowAction.StartDevelopment:
                changeRequest.StartDevelopment();
                break;
            case ChangeRequestWorkflowAction.StartQaTesting:
                changeRequest.StartQaTesting();
                break;
            case ChangeRequestWorkflowAction.CompleteQaTesting:
                changeRequest.CompleteQaTesting();
                break;
            case ChangeRequestWorkflowAction.ApproveRelease:
                changeRequest.ApproveRelease();
                break;
            case ChangeRequestWorkflowAction.Reject:
                changeRequest.Reject();
                break;
            case ChangeRequestWorkflowAction.Close:
                changeRequest.Close();
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(command),
                    command.Action,
                    "Unsupported workflow action.");
        }

        await _repository.AddAuditEntryAsync(
            ChangeRequestAuditEntry.Create(
                changeRequest.Id,
                command.Action.ToString(),
                command.Actor,
                command.Comment,
                fromStatus,
                changeRequest.Status),
            cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return ChangeRequestResponseMapper.ToDetails(
            changeRequest,
            DateTimeOffset.UtcNow);
    }

    public async Task<ChangeRequestDetailsResponse> RecordReleaseArtifactsAsync(
        string referenceNumber,
        RecordReleaseArtifactsCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.QaEvidenceNotes)
            && string.IsNullOrWhiteSpace(command.RollbackPlan))
        {
            throw new ArgumentException(
                "Provide QA evidence notes, a rollback plan, or both.",
                nameof(command));
        }

        var changeRequest = await GetTrackedAsync(
            referenceNumber,
            cancellationToken);
        var actions = new List<string>();

        if (!string.IsNullOrWhiteSpace(command.QaEvidenceNotes))
        {
            changeRequest.RecordQaEvidence(command.QaEvidenceNotes);
            actions.Add("QA evidence");
        }

        if (!string.IsNullOrWhiteSpace(command.RollbackPlan))
        {
            changeRequest.RecordRollbackPlan(command.RollbackPlan);
            actions.Add("rollback plan");
        }

        await _repository.AddAuditEntryAsync(
            ChangeRequestAuditEntry.Create(
                changeRequest.Id,
                "ReleaseArtifactsRecorded",
                command.Actor,
                $"Recorded {string.Join(" and ", actions)}.",
                changeRequest.Status,
                changeRequest.Status),
            cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return ChangeRequestResponseMapper.ToDetails(
            changeRequest,
            DateTimeOffset.UtcNow);
    }

    private async Task<ChangeRequest> GetTrackedAsync(
        string referenceNumber,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(referenceNumber);
        var changeRequest = await _repository.GetTrackedByReferenceNumberAsync(
            referenceNumber,
            cancellationToken);

        return changeRequest
            ?? throw new ChangeRequestNotFoundException(referenceNumber);
    }
}
