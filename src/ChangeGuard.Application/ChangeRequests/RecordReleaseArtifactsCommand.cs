namespace ChangeGuard.Application.ChangeRequests;

public sealed record RecordReleaseArtifactsCommand(
    string? QaEvidenceNotes,
    string? RollbackPlan,
    string Actor);
