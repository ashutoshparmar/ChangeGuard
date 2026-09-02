namespace ChangeGuard.Application.ChangeRequests;

public interface IChangeRequestWorkflowService
{
    Task<ChangeRequestDetailsResponse> ApplyActionAsync(
        string referenceNumber,
        ChangeRequestWorkflowCommand command,
        CancellationToken cancellationToken = default);

    Task<ChangeRequestDetailsResponse> RecordReleaseArtifactsAsync(
        string referenceNumber,
        RecordReleaseArtifactsCommand command,
        CancellationToken cancellationToken = default);
}
