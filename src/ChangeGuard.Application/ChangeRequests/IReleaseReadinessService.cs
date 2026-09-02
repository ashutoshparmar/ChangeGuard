using System.Threading;
using System.Threading.Tasks;

namespace ChangeGuard.Application.ChangeRequests;

public interface IReleaseReadinessService
{
    Task<ReleaseReadinessResponse?>
        GetReleaseReadinessAsync(
            string referenceNumber,
            CancellationToken cancellationToken = default);
}