using ChangeGuard.Api.Contracts.ChangeRequests;
using ChangeGuard.Application.ChangeRequests;
using ChangeGuard.Domain.ChangeRequests;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChangeGuard.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/change-requests")]
public sealed class ChangeRequestsController : ControllerBase
{
    private readonly ICreateChangeRequestService _createService;
    private readonly IChangeRequestQueryService _queryService;
    private readonly IChangeRequestWorkflowService _workflowService;
    private readonly IReleaseReadinessService _releaseReadinessService;

    public ChangeRequestsController(
        ICreateChangeRequestService createService,
        IChangeRequestQueryService queryService,
        IChangeRequestWorkflowService workflowService,
        IReleaseReadinessService releaseReadinessService)
    {
        _createService = createService;
        _queryService = queryService;
        _workflowService = workflowService;
        _releaseReadinessService = releaseReadinessService;
    }

    [HttpPost]
    [ProducesResponseType<CreateChangeRequestResponse>(
        StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CreateChangeRequestResponse>> Create(
        CreateChangeRequestRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _createService.CreateAsync(
            new CreateChangeRequestCommand(
                request.ReferenceNumber,
                request.Title,
                request.Priority,
                request.Description,
                request.Actor),
            cancellationToken);

        return CreatedAtAction(
            nameof(GetByReferenceNumber),
            new { referenceNumber = response.ReferenceNumber },
            response);
    }

    [HttpGet]
    [ProducesResponseType<PagedResponse<ChangeRequestSummaryResponse>>(
        StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<ChangeRequestSummaryResponse>>>
        Search(
            [FromQuery] string? search,
            [FromQuery] ChangePriority? priority,
            [FromQuery] ChangeRequestStatus? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
    {
        var response = await _queryService.SearchAsync(
            new ChangeRequestSearchCriteria(
                search,
                priority,
                status,
                page,
                pageSize),
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("dashboard")]
    [ProducesResponseType<DashboardResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardResponse>> GetDashboard(
        CancellationToken cancellationToken)
    {
        return Ok(await _queryService.GetDashboardAsync(cancellationToken));
    }

    [HttpGet("{referenceNumber}")]
    [ProducesResponseType<ChangeRequestDetailsResponse>(
        StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChangeRequestDetailsResponse>>
        GetByReferenceNumber(
            string referenceNumber,
            CancellationToken cancellationToken)
    {
        return Ok(await _queryService.GetByReferenceNumberAsync(
            referenceNumber,
            cancellationToken));
    }

    [HttpGet("{referenceNumber}/release-readiness")]
    [ProducesResponseType<ReleaseReadinessResponse>(
        StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReleaseReadinessResponse>>
        GetReleaseReadiness(
            string referenceNumber,
            CancellationToken cancellationToken)
    {
        var response = await _releaseReadinessService
            .GetReleaseReadinessAsync(
                referenceNumber,
                cancellationToken);

        if (response is null)
        {
            throw new ChangeRequestNotFoundException(referenceNumber);
        }

        return Ok(response);
    }

    [HttpGet("{referenceNumber}/audit")]
    [ProducesResponseType<IReadOnlyList<ChangeRequestAuditResponse>>(
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ChangeRequestAuditResponse>>>
        GetAudit(
            string referenceNumber,
            CancellationToken cancellationToken)
    {
        return Ok(await _queryService.GetAuditAsync(
            referenceNumber,
            cancellationToken));
    }

    [HttpPost("{referenceNumber}/workflow")]
    [ProducesResponseType<ChangeRequestDetailsResponse>(
        StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ChangeRequestDetailsResponse>> ApplyAction(
        string referenceNumber,
        WorkflowActionRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _workflowService.ApplyActionAsync(
            referenceNumber,
            new ChangeRequestWorkflowCommand(
                request.Action,
                request.Actor,
                request.Comment),
            cancellationToken);

        return Ok(response);
    }

    [HttpPut("{referenceNumber}/release-artifacts")]
    [ProducesResponseType<ChangeRequestDetailsResponse>(
        StatusCodes.Status200OK)]
    public async Task<ActionResult<ChangeRequestDetailsResponse>>
        RecordReleaseArtifacts(
            string referenceNumber,
            ReleaseArtifactsRequest request,
            CancellationToken cancellationToken)
    {
        var response = await _workflowService.RecordReleaseArtifactsAsync(
            referenceNumber,
            new RecordReleaseArtifactsCommand(
                request.QaEvidenceNotes,
                request.RollbackPlan,
                request.Actor),
            cancellationToken);

        return Ok(response);
    }
}
