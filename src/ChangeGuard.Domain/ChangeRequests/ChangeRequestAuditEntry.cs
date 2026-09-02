namespace ChangeGuard.Domain.ChangeRequests;

public sealed class ChangeRequestAuditEntry
{
    private ChangeRequestAuditEntry()
    {
        Action = string.Empty;
        Actor = string.Empty;
        Comment = string.Empty;
    }

    private ChangeRequestAuditEntry(
        Guid changeRequestId,
        string action,
        string actor,
        string comment,
        ChangeRequestStatus? fromStatus,
        ChangeRequestStatus toStatus)
    {
        Id = Guid.NewGuid();
        ChangeRequestId = changeRequestId;
        Action = action;
        Actor = actor;
        Comment = comment;
        FromStatus = fromStatus;
        ToStatus = toStatus;
        OccurredUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid ChangeRequestId { get; private set; }

    public string Action { get; private set; }

    public string Actor { get; private set; }

    public string Comment { get; private set; }

    public ChangeRequestStatus? FromStatus { get; private set; }

    public ChangeRequestStatus ToStatus { get; private set; }

    public DateTimeOffset OccurredUtc { get; private set; }

    public static ChangeRequestAuditEntry Create(
        Guid changeRequestId,
        string action,
        string actor,
        string? comment,
        ChangeRequestStatus? fromStatus,
        ChangeRequestStatus toStatus)
    {
        if (changeRequestId == Guid.Empty)
        {
            throw new ArgumentException(
                "Change request id is required.",
                nameof(changeRequestId));
        }

        if (string.IsNullOrWhiteSpace(action))
        {
            throw new ArgumentException(
                "Audit action is required.",
                nameof(action));
        }

        var normalizedActor = string.IsNullOrWhiteSpace(actor)
            ? "local-user"
            : actor.Trim();

        return new ChangeRequestAuditEntry(
            changeRequestId,
            action.Trim(),
            normalizedActor,
            comment?.Trim() ?? string.Empty,
            fromStatus,
            toStatus);
    }
}
