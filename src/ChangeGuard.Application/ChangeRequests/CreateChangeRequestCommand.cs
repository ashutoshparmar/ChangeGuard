using ChangeGuard.Domain.ChangeRequests;

namespace ChangeGuard.Application.ChangeRequests;

public sealed record CreateChangeRequestCommand(
    string ReferenceNumber,
    string Title,
    ChangePriority Priority,
    string Description = "",
    string Actor = "local-user");
