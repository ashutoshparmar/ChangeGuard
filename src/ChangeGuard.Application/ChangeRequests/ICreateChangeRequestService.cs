namespace ChangeGuard.Application.ChangeRequests;

public interface ICreateChangeRequestService
{
    Task<CreateChangeRequestResponse> CreateAsync(
        CreateChangeRequestCommand command,
        CancellationToken cancellationToken = default);
}