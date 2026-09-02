namespace ChangeGuard.Application.ChangeRequests;

public interface IChangeRequestQueryService
{
    Task<PagedResponse<ChangeRequestSummaryResponse>> SearchAsync(
        ChangeRequestSearchCriteria criteria,
        CancellationToken cancellationToken = default);

    Task<ChangeRequestDetailsResponse> GetByReferenceNumberAsync(
        string referenceNumber,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ChangeRequestAuditResponse>> GetAuditAsync(
        string referenceNumber,
        CancellationToken cancellationToken = default);

    Task<DashboardResponse> GetDashboardAsync(
        CancellationToken cancellationToken = default);
}
