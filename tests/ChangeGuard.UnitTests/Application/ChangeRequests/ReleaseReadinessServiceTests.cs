using System.Threading;
using System.Threading.Tasks;

using ChangeGuard.Application.ChangeRequests;
using ChangeGuard.Domain.ChangeRequests;

using Xunit;

namespace ChangeGuard.UnitTests.Application.ChangeRequests;

public sealed class ReleaseReadinessServiceTests
{
    [Fact]
    public async Task GetReleaseReadinessAsync_WhenRequestExists_ReturnsAssessment()
    {
        var changeRequest = new ChangeRequest(
            "CG-101",
            "Payment validation update",
            ChangePriority.Critical,
            ChangeRequestStatus.QaTesting);

        var repository = new FakeChangeRequestRepository(
            changeRequest);

        var service =
            new ReleaseReadinessService(repository);

        var response =
            await service.GetReleaseReadinessAsync(
                "CG-101");

        Assert.NotNull(response);
        Assert.Equal("CG-101", response.ReferenceNumber);
        Assert.Equal(45, response.Score);
        Assert.True(response.IsBlocked);
        Assert.False(
            response.CanMoveToReleaseApproval);
        Assert.Equal(2, response.Blockers.Count);
    }

    [Fact]
    public async Task GetReleaseReadinessAsync_WhenRequestDoesNotExist_ReturnsNull()
    {
        var repository = new FakeChangeRequestRepository();

        var service =
            new ReleaseReadinessService(repository);

        var response =
            await service.GetReleaseReadinessAsync(
                "CG-999");

        Assert.Null(response);
    }
}
