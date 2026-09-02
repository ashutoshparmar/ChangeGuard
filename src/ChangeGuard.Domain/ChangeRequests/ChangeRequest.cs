using System;
using System.Collections.Generic;

namespace ChangeGuard.Domain.ChangeRequests;

public sealed class ChangeRequest
{
    private const int MissingQaEvidencePenalty = 30;
    private const int MissingRollbackPlanPenalty = 25;

    private ChangeRequest()
    {
        ReferenceNumber = string.Empty;
        Title = string.Empty;
        Description = string.Empty;
    }

    public ChangeRequest(
        string referenceNumber,
        string title,
        ChangePriority priority,
        ChangeRequestStatus status)
        : this(
            referenceNumber,
            title,
            description: string.Empty,
            priority,
            status)
    {
    }

    private ChangeRequest(
        string referenceNumber,
        string title,
        string description,
        ChangePriority priority,
        ChangeRequestStatus status)
    {
        if (string.IsNullOrWhiteSpace(referenceNumber))
        {
            throw new ArgumentException(
                "Reference number is required.",
                nameof(referenceNumber));
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException(
                "Title is required.",
                nameof(title));
        }

        Id = Guid.NewGuid();
        ReferenceNumber = referenceNumber.Trim();
        Title = title.Trim();
        Description = description?.Trim() ?? string.Empty;
        Priority = priority;
        Status = status;
        CreatedUtc = DateTimeOffset.UtcNow;
        UpdatedUtc = CreatedUtc;
    }

    public Guid Id { get; private set; }

    public string ReferenceNumber { get; private set; }

    public string Title { get; private set; }

    public string Description { get; private set; }

    public ChangePriority Priority { get; private set; }

    public ChangeRequestStatus Status { get; private set; }

    public bool HasQaEvidence { get; private set; }

    public string? QaEvidenceNotes { get; private set; }

    public bool HasRollbackPlan { get; private set; }

    public string? RollbackPlan { get; private set; }

    public DateTimeOffset CreatedUtc { get; private set; }

    public DateTimeOffset UpdatedUtc { get; private set; }

    public static ChangeRequest CreateDraft(
        string referenceNumber,
        string title,
        ChangePriority priority)
    {
        return CreateDraft(
            referenceNumber,
            title,
            description: string.Empty,
            priority);
    }

    public static ChangeRequest CreateDraft(
        string referenceNumber,
        string title,
        string description,
        ChangePriority priority)
    {
        return new ChangeRequest(
            referenceNumber,
            title,
            description,
            priority,
            ChangeRequestStatus.Draft);
    }

    public void RecordQaEvidence()
    {
        RecordQaEvidence("QA evidence recorded.");
    }

    public void RecordQaEvidence(string notes)
    {
        EnsureStatus(ChangeRequestStatus.QaTesting);

        if (string.IsNullOrWhiteSpace(notes))
        {
            throw new DomainRuleViolationException(
                "QA evidence notes are required.");
        }

        HasQaEvidence = true;
        QaEvidenceNotes = notes.Trim();
        Touch();
    }

    public void RecordRollbackPlan()
    {
        RecordRollbackPlan("Rollback plan recorded.");
    }

    public void RecordRollbackPlan(string rollbackPlan)
    {
        if (Status is not ChangeRequestStatus.InDevelopment
            and not ChangeRequestStatus.QaTesting)
        {
            throw new DomainRuleViolationException(
                "A rollback plan can only be recorded during development or QA testing.");
        }

        if (string.IsNullOrWhiteSpace(rollbackPlan))
        {
            throw new DomainRuleViolationException(
                "Rollback plan details are required.");
        }

        HasRollbackPlan = true;
        RollbackPlan = rollbackPlan.Trim();
        Touch();
    }

    public void UpdateDraftDetails(
        string title,
        string description,
        ChangePriority priority)
    {
        if (Status is not ChangeRequestStatus.Draft
            and not ChangeRequestStatus.RequirementReview)
        {
            throw new DomainRuleViolationException(
                "Request details can only be changed while the request is in Draft or Requirement Review.");
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException(
                "Title is required.",
                nameof(title));
        }

        Title = title.Trim();
        Description = description?.Trim() ?? string.Empty;
        Priority = priority;
        Touch();
    }

    public void SubmitForRequirementReview()
    {
        Transition(
            ChangeRequestStatus.Draft,
            ChangeRequestStatus.RequirementReview);
    }

    public void StartDevelopment()
    {
        Transition(
            ChangeRequestStatus.RequirementReview,
            ChangeRequestStatus.InDevelopment);
    }

    public void StartQaTesting()
    {
        Transition(
            ChangeRequestStatus.InDevelopment,
            ChangeRequestStatus.QaTesting);
    }

    public void CompleteQaTesting()
    {
        EnsureStatus(ChangeRequestStatus.QaTesting);

        if (!HasQaEvidence)
        {
            throw new DomainRuleViolationException(
                "QA testing cannot be completed until evidence is recorded.");
        }

        if (!HasRollbackPlan)
        {
            throw new DomainRuleViolationException(
                "QA testing cannot be completed until a rollback plan is recorded.");
        }

        Status = ChangeRequestStatus.ReleaseApproval;
        Touch();
    }

    public void ApproveRelease()
    {
        Transition(
            ChangeRequestStatus.ReleaseApproval,
            ChangeRequestStatus.Released);
    }

    public void Reject()
    {
        if (Status is ChangeRequestStatus.Released
            or ChangeRequestStatus.Rejected
            or ChangeRequestStatus.Closed)
        {
            throw new DomainRuleViolationException(
                $"A request in {Status} cannot be rejected.");
        }

        Status = ChangeRequestStatus.Rejected;
        Touch();
    }

    public void Close()
    {
        if (Status is not ChangeRequestStatus.Released
            and not ChangeRequestStatus.Rejected)
        {
            throw new DomainRuleViolationException(
                "Only a Released or Rejected request can be closed.");
        }

        Status = ChangeRequestStatus.Closed;
        Touch();
    }

    public ReleaseReadinessAssessment AssessReleaseReadiness()
    {
        var score = 100;
        var blockers = new List<string>();

        if (Status is ChangeRequestStatus.Draft
            or ChangeRequestStatus.RequirementReview
            or ChangeRequestStatus.InDevelopment)
        {
            blockers.Add(
                "Request must be in QA Testing before release approval.");
        }

        if (Status == ChangeRequestStatus.Rejected)
        {
            blockers.Add("A rejected request is not eligible for release.");
        }

        if (!HasQaEvidence)
        {
            score -= MissingQaEvidencePenalty;
            blockers.Add("QA evidence is missing.");
        }

        if (!HasRollbackPlan)
        {
            score -= MissingRollbackPlanPenalty;
            blockers.Add("Rollback plan is missing.");
        }

        return new ReleaseReadinessAssessment(
            Score: Math.Max(0, score),
            Blockers: blockers.AsReadOnly());
    }

    private void Transition(
        ChangeRequestStatus expectedStatus,
        ChangeRequestStatus nextStatus)
    {
        EnsureStatus(expectedStatus);
        Status = nextStatus;
        Touch();
    }

    private void EnsureStatus(
        ChangeRequestStatus expectedStatus)
    {
        if (Status != expectedStatus)
        {
            throw new DomainRuleViolationException(
                $"Request must be in {expectedStatus} but is currently in {Status}.");
        }
    }

    private void Touch()
    {
        UpdatedUtc = DateTimeOffset.UtcNow;
    }
}
