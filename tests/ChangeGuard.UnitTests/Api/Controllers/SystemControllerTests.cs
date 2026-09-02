using ChangeGuard.Api.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ChangeGuard.UnitTests.Api.Controllers;

public sealed class SystemControllerTests
{
    [Fact]
    public void GetHealth_ReturnsOkWithHealthyServiceInformation()
    {
        // Arrange
        var controller = new SystemController();
        var beforeCall = DateTimeOffset.UtcNow;

        // Act
        ActionResult<SystemHealthResponse> actionResult =
            controller.GetHealth();

        var afterCall = DateTimeOffset.UtcNow;

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(
            actionResult.Result);

        var response = Assert.IsType<SystemHealthResponse>(
            okResult.Value);

        Assert.Equal(
            StatusCodes.Status200OK,
            okResult.StatusCode);

        Assert.Equal("Healthy", response.Status);
        Assert.Equal("ChangeGuard.Api", response.Service);
        Assert.Equal("1.0.0", response.Version);

        Assert.InRange(
            response.TimestampUtc,
            beforeCall,
            afterCall);
    }
}