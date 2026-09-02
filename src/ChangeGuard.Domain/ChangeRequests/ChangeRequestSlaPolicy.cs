namespace ChangeGuard.Domain.ChangeRequests;

public static class ChangeRequestSlaPolicy
{
    public static SlaAssessment Assess(
        ChangeRequest changeRequest,
        DateTimeOffset currentUtc)
    {
        ArgumentNullException.ThrowIfNull(changeRequest);

        var dueUtc = changeRequest.CreatedUtc.Add(
            GetTarget(changeRequest.Priority));

        var isCompleted = changeRequest.Status is
            ChangeRequestStatus.Released
            or ChangeRequestStatus.Rejected
            or ChangeRequestStatus.Closed;

        var remainingHours =
            (dueUtc - currentUtc).TotalHours;

        return new SlaAssessment(
            DueUtc: dueUtc,
            IsBreached: !isCompleted && remainingHours < 0,
            RemainingHours: remainingHours);
    }

    public static TimeSpan GetTarget(
        ChangePriority priority)
    {
        return priority switch
        {
            ChangePriority.Critical => TimeSpan.FromHours(4),
            ChangePriority.High => TimeSpan.FromHours(24),
            ChangePriority.Medium => TimeSpan.FromHours(72),
            ChangePriority.Low => TimeSpan.FromHours(120),
            _ => throw new ArgumentOutOfRangeException(
                nameof(priority),
                priority,
                "Unsupported change priority.")
        };
    }
}
