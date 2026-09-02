using ChangeGuard.Domain.ChangeRequests;

namespace ChangeGuard.Application.ChangeRequests;

internal static class ChangeRequestResponseMapper
{
    public static ChangeRequestSummaryResponse ToSummary(
        ChangeRequest changeRequest,
        DateTimeOffset currentUtc)
    {
        var readiness = changeRequest.AssessReleaseReadiness();
        var sla = ChangeRequestSlaPolicy.Assess(
            changeRequest,
            currentUtc);

        return new ChangeRequestSummaryResponse(
            changeRequest.Id,
            changeRequest.ReferenceNumber,
            changeRequest.Title,
            changeRequest.Priority.ToString(),
            changeRequest.Status.ToString(),
            readiness.Score,
            readiness.IsBlocked,
            sla.DueUtc,
            sla.IsBreached,
            changeRequest.UpdatedUtc);
    }

    public static ChangeRequestDetailsResponse ToDetails(
        ChangeRequest changeRequest,
        DateTimeOffset currentUtc)
    {
        var readiness = changeRequest.AssessReleaseReadiness();
        var sla = ChangeRequestSlaPolicy.Assess(
            changeRequest,
            currentUtc);

        return new ChangeRequestDetailsResponse(
            changeRequest.Id,
            changeRequest.ReferenceNumber,
            changeRequest.Title,
            changeRequest.Description,
            changeRequest.Priority.ToString(),
            changeRequest.Status.ToString(),
            changeRequest.HasQaEvidence,
            changeRequest.QaEvidenceNotes,
            changeRequest.HasRollbackPlan,
            changeRequest.RollbackPlan,
            readiness.Score,
            readiness.IsBlocked,
            readiness.CanMoveToReleaseApproval,
            readiness.Blockers,
            changeRequest.CreatedUtc,
            changeRequest.UpdatedUtc,
            sla.DueUtc,
            sla.IsBreached,
            Math.Round(sla.RemainingHours, 1));
    }
}
