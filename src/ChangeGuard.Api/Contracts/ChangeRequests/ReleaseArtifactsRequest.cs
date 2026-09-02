using System.ComponentModel.DataAnnotations;

namespace ChangeGuard.Api.Contracts.ChangeRequests;

public sealed class ReleaseArtifactsRequest
{
    [StringLength(4000)]
    public string? QaEvidenceNotes { get; init; }

    [StringLength(4000)]
    public string? RollbackPlan { get; init; }

    [Required]
    [StringLength(200)]
    public string Actor { get; init; } = "local-user";
}
