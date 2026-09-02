using ChangeGuard.Domain.ChangeRequests;

namespace ChangeGuard.Application.ChangeRequests;

public sealed record ChangeRequestWorkflowCommand(
    ChangeRequestWorkflowAction Action,
    string Actor,
    string? Comment);
