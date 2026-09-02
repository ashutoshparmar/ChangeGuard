using ChangeGuard.Application.ChangeRequests.Abstractions;
using ChangeGuard.Domain.ChangeRequests;

namespace ChangeGuard.Application.ChangeRequests;

public sealed class CreateChangeRequestService
    : ICreateChangeRequestService
{
    private readonly IChangeRequestRepository
        _changeRequestRepository;

    public CreateChangeRequestService(
        IChangeRequestRepository changeRequestRepository)
    {
        _changeRequestRepository =
            changeRequestRepository;
    }

    public async Task<CreateChangeRequestResponse> CreateAsync(
        CreateChangeRequestCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var changeRequest = ChangeRequest.CreateDraft(
            command.ReferenceNumber,
            command.Title,
            command.Description,
            command.Priority);

        if (await _changeRequestRepository.ExistsAsync(
                changeRequest.ReferenceNumber,
                cancellationToken))
        {
            throw new DuplicateChangeRequestException(
                changeRequest.ReferenceNumber);
        }

        await _changeRequestRepository.AddAsync(
            changeRequest,
            cancellationToken);

        await _changeRequestRepository.AddAuditEntryAsync(
            ChangeRequestAuditEntry.Create(
                changeRequest.Id,
                action: "Created",
                actor: command.Actor,
                comment: "Change request created as a draft.",
                fromStatus: null,
                toStatus: changeRequest.Status),
            cancellationToken);

        await _changeRequestRepository.SaveChangesAsync(
            cancellationToken);

        var sla = ChangeRequestSlaPolicy.Assess(
            changeRequest,
            DateTimeOffset.UtcNow);

        return new CreateChangeRequestResponse(
            Id: changeRequest.Id,
            ReferenceNumber: changeRequest.ReferenceNumber,
            Title: changeRequest.Title,
            Description: changeRequest.Description,
            Priority: changeRequest.Priority.ToString(),
            Status: changeRequest.Status.ToString(),
            CreatedUtc: changeRequest.CreatedUtc,
            SlaDueUtc: sla.DueUtc);
    }
}
