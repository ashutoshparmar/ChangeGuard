using ChangeGuard.Application.ChangeRequests;
using ChangeGuard.Domain.ChangeRequests;

namespace ChangeGuard.UnitTests.Application.ChangeRequests;

public sealed class ChangeRequestWorkflowServiceTests
{
    [Fact]
    public async Task ApplyActionAsync_WhenTransitionIsValid_UpdatesAndAudits()
    {
        var changeRequest = ChangeRequest.CreateDraft(
            "CG-601",
            "Introduce fraud checks",
            ChangePriority.Critical);
        var repository = new FakeChangeRequestRepository(changeRequest);
        var service = new ChangeRequestWorkflowService(repository);

        var response = await service.ApplyActionAsync(
            "CG-601",
            new ChangeRequestWorkflowCommand(
                ChangeRequestWorkflowAction.SubmitForReview,
                "product-owner@example.com",
                "Ready for requirement review."));

        Assert.Equal("RequirementReview", response.Status);
        Assert.Single(repository.AuditEntries);
        Assert.Equal(
            ChangeRequestStatus.Draft,
            repository.AuditEntries[0].FromStatus);
        Assert.Equal(
            ChangeRequestStatus.RequirementReview,
            repository.AuditEntries[0].ToStatus);
        Assert.Equal(1, repository.SaveCount);
    }

    [Fact]
    public async Task ApplyActionAsync_WhenQaArtifactsAreMissing_RejectsCompletion()
    {
        var changeRequest = new ChangeRequest(
            "CG-602",
            "Introduce fraud checks",
            ChangePriority.Critical,
            ChangeRequestStatus.QaTesting);
        var repository = new FakeChangeRequestRepository(changeRequest);
        var service = new ChangeRequestWorkflowService(repository);

        await Assert.ThrowsAsync<DomainRuleViolationException>(() =>
            service.ApplyActionAsync(
                "CG-602",
                new ChangeRequestWorkflowCommand(
                    ChangeRequestWorkflowAction.CompleteQaTesting,
                    "qa@example.com",
                    "Testing completed.")));

        Assert.Equal(ChangeRequestStatus.QaTesting, changeRequest.Status);
        Assert.Empty(repository.AuditEntries);
        Assert.Equal(0, repository.SaveCount);
    }

    [Fact]
    public async Task RecordReleaseArtifactsAsync_InQa_RecordsBothArtifacts()
    {
        var changeRequest = new ChangeRequest(
            "CG-603",
            "Introduce fraud checks",
            ChangePriority.Critical,
            ChangeRequestStatus.QaTesting);
        var repository = new FakeChangeRequestRepository(changeRequest);
        var service = new ChangeRequestWorkflowService(repository);

        var response = await service.RecordReleaseArtifactsAsync(
            "CG-603",
            new RecordReleaseArtifactsCommand(
                "All regression tests passed; evidence link DEV-42.",
                "Restore database backup and redeploy version 1.4.",
                "qa@example.com"));

        Assert.True(response.HasQaEvidence);
        Assert.True(response.HasRollbackPlan);
        Assert.Equal(100, response.ReadinessScore);
        Assert.Single(repository.AuditEntries);
        Assert.Equal(1, repository.SaveCount);
    }
}
