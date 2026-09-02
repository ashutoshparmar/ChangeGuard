using System.ComponentModel.DataAnnotations;

using ChangeGuard.Domain.ChangeRequests;

namespace ChangeGuard.Api.Contracts.ChangeRequests;

public sealed class WorkflowActionRequest
{
    [EnumDataType(typeof(ChangeRequestWorkflowAction))]
    public ChangeRequestWorkflowAction Action { get; init; }

    [Required]
    [StringLength(200)]
    public string Actor { get; init; } = "local-user";

    [StringLength(2000)]
    public string? Comment { get; init; }
}
