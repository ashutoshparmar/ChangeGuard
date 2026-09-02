namespace ChangeGuard.Application.ChangeRequests;

public sealed record CreateChangeRequestResponse(
    Guid Id,
    string ReferenceNumber,
    string Title,
    string Description,
    string Priority,
    string Status,
    DateTimeOffset CreatedUtc,
    DateTimeOffset SlaDueUtc);
