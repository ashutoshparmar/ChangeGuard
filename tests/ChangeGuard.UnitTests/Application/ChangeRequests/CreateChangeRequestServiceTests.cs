using ChangeGuard.Application.ChangeRequests;
using ChangeGuard.Domain.ChangeRequests;

namespace ChangeGuard.UnitTests.Application.ChangeRequests;

public sealed class CreateChangeRequestServiceTests
{
    [Fact]
    public async Task CreateAsync_WhenReferenceIsNew_PersistsRequestAndAudit()
    {
        var repository = new FakeChangeRequestRepository();
        var service = new CreateChangeRequestService(repository);

        var response = await service.CreateAsync(
            new CreateChangeRequestCommand(
                "CG-501",
                "Improve payment validation",
                ChangePriority.High,
                "Prevent duplicate settlements.",
                "product-owner@example.com"));

        Assert.Equal("CG-501", response.ReferenceNumber);
        Assert.Equal("Draft", response.Status);
        Assert.Equal("Prevent duplicate settlements.", response.Description);
        Assert.Single(repository.AuditEntries);
        Assert.Equal("Created", repository.AuditEntries[0].Action);
        Assert.Equal(1, repository.SaveCount);
    }

    [Fact]
    public async Task CreateAsync_WhenReferenceExists_ThrowsConflict()
    {
        var existing = ChangeRequest.CreateDraft(
            "CG-501",
            "Existing request",
            ChangePriority.Medium);
        var repository = new FakeChangeRequestRepository(existing);
        var service = new CreateChangeRequestService(repository);

        await Assert.ThrowsAsync<DuplicateChangeRequestException>(() =>
            service.CreateAsync(new CreateChangeRequestCommand(
                "CG-501",
                "Duplicate request",
                ChangePriority.High)));

        Assert.Equal(0, repository.SaveCount);
        Assert.Empty(repository.AuditEntries);
    }
}
