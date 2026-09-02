using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChangeGuard.Api.Controllers;

[ApiController]
[Route("api/system")]
public sealed class SystemController : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("health")]
    [ProducesResponseType(
        typeof(SystemHealthResponse),
        StatusCodes.Status200OK)]
    public ActionResult<SystemHealthResponse> GetHealth()
    {
        var response = new SystemHealthResponse(
            Status: "Healthy",
            Service: "ChangeGuard.Api",
            Version: "1.0.0",
            TimestampUtc: DateTimeOffset.UtcNow);

        return Ok(response);
    }
}

public sealed record SystemHealthResponse(
    string Status,
    string Service,
    string Version,
    DateTimeOffset TimestampUtc);