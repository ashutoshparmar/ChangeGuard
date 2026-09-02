using ChangeGuard.Domain.ChangeRequests;
using Xunit;
using System;

namespace ChangeGuard.UnitTests.Domain.ChangeRequests;

public sealed class ChangeRequestTests
{
    [Fact]
    public void AssessReleaseReadiness_WhenMandatoryArtifactsAreMissing_ReturnsBlockedScoreOf45()
    {
        // Arrange
        var changeRequest = new ChangeRequest(
            referenceNumber: "CG-101",
            title: "Payment validation update",
            priority: ChangePriority.Critical,
            status: ChangeRequestStatus.QaTesting);

        // Act
        var result = changeRequest.AssessReleaseReadiness();

        // Assert
        Assert.Equal(45, result.Score);
        Assert.True(result.IsBlocked);
        Assert.False(result.CanMoveToReleaseApproval);
        Assert.Equal(2, result.Blockers.Count);
        Assert.Contains("QA evidence is missing.", result.Blockers);
        Assert.Contains("Rollback plan is missing.", result.Blockers);
    }

    [Fact]
    public void AssessReleaseReadiness_WhenOnlyRollbackPlanIsMissing_ReturnsBlockedScoreOf75()
    {
        // Arrange
        var changeRequest = new ChangeRequest(
            referenceNumber: "CG-101",
            title: "Payment validation update",
            priority: ChangePriority.Critical,
            status: ChangeRequestStatus.QaTesting);

        changeRequest.RecordQaEvidence();

        // Act
        var result = changeRequest.AssessReleaseReadiness();

        // Assert
        Assert.Equal(75, result.Score);
        Assert.True(result.IsBlocked);
        Assert.False(result.CanMoveToReleaseApproval);
        Assert.Single(result.Blockers);
        Assert.Contains("Rollback plan is missing.", result.Blockers);
    }

    [Fact]
    public void AssessReleaseReadiness_WhenAllMandatoryArtifactsExist_AllowsReleaseApproval()
    {
        // Arrange
        var changeRequest = new ChangeRequest(
            referenceNumber: "CG-101",
            title: "Payment validation update",
            priority: ChangePriority.Critical,
            status: ChangeRequestStatus.QaTesting);

        changeRequest.RecordQaEvidence();
        changeRequest.RecordRollbackPlan();

        // Act
        var result = changeRequest.AssessReleaseReadiness();

        // Assert
        Assert.Equal(100, result.Score);
        Assert.False(result.IsBlocked);
        Assert.True(result.CanMoveToReleaseApproval);
        Assert.Empty(result.Blockers);
    }

    [Fact]
    public void AssessReleaseReadiness_WhenRequestIsNotInQaTesting_BlocksReleaseApproval()
    {
        // Arrange
        var changeRequest = new ChangeRequest(
            referenceNumber: "CG-101",
            title: "Payment validation update",
            priority: ChangePriority.Critical,
            status: ChangeRequestStatus.InDevelopment);

        changeRequest.RecordRollbackPlan();

        // Act
        var result = changeRequest.AssessReleaseReadiness();

        // Assert
        Assert.Equal(70, result.Score);
        Assert.True(result.IsBlocked);
        Assert.False(result.CanMoveToReleaseApproval);
        Assert.Contains(
            "Request must be in QA Testing before release approval.",
            result.Blockers);
        Assert.Contains("QA evidence is missing.", result.Blockers);
    }
    [Fact]
    public void Constructor_WhenDataIsValid_CreatesIdentityAndTimestamp()
    {
        var firstRequest = new ChangeRequest(
            "CG-101",
            "Payment validation update",
            ChangePriority.Critical,
            ChangeRequestStatus.QaTesting);

        var secondRequest = new ChangeRequest(
            "CG-102",
            "Customer notification update",
            ChangePriority.High,
            ChangeRequestStatus.QaTesting);

        Assert.NotEqual(Guid.Empty, firstRequest.Id);
        Assert.NotEqual(Guid.Empty, secondRequest.Id);
        Assert.NotEqual(firstRequest.Id, secondRequest.Id);
        Assert.NotEqual(
            default(DateTimeOffset),
            firstRequest.CreatedUtc);
    }
    [Fact]
    public void CreateDraft_WhenInputIsValid_CreatesDraftRequest()
    {
        var changeRequest = ChangeRequest.CreateDraft(
            referenceNumber: " CG-102 ",
            title: " Improve payment validation ",
            priority: ChangePriority.High);

        Assert.NotEqual(Guid.Empty, changeRequest.Id);
        Assert.Equal("CG-102", changeRequest.ReferenceNumber);
        Assert.Equal(
            "Improve payment validation",
            changeRequest.Title);
        Assert.Equal(
            ChangePriority.High,
            changeRequest.Priority);
        Assert.Equal(
            ChangeRequestStatus.Draft,
            changeRequest.Status);
        Assert.False(changeRequest.HasQaEvidence);
        Assert.False(changeRequest.HasRollbackPlan);
        Assert.NotEqual(
            default,
            changeRequest.CreatedUtc);
    }
    [Fact]
    public void CreateDraft_WhenReferenceNumberIsBlank_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => ChangeRequest.CreateDraft(
                referenceNumber: " ",
                title: "Improve payment validation",
                priority: ChangePriority.High));

        Assert.Equal(
            "referenceNumber",
            exception.ParamName);
    }

    [Fact]
    public void CompleteQaTesting_WhenEvidenceAndRollbackExist_MovesToReleaseApproval()
    {
        var changeRequest = new ChangeRequest(
            "CG-103",
            "Improve payment validation",
            ChangePriority.High,
            ChangeRequestStatus.QaTesting);
        changeRequest.RecordQaEvidence("Regression suite passed.");
        changeRequest.RecordRollbackPlan("Redeploy the previous image.");

        changeRequest.CompleteQaTesting();

        Assert.Equal(
            ChangeRequestStatus.ReleaseApproval,
            changeRequest.Status);
    }

    [Fact]
    public void StartDevelopment_WhenRequestIsDraft_ThrowsDomainRuleViolation()
    {
        var changeRequest = ChangeRequest.CreateDraft(
            "CG-104",
            "Improve payment validation",
            ChangePriority.High);

        Assert.Throws<DomainRuleViolationException>(
            changeRequest.StartDevelopment);
        Assert.Equal(ChangeRequestStatus.Draft, changeRequest.Status);
    }
}
