namespace ChangeGuard.Domain.ChangeRequests;

public enum ChangeRequestStatus
{
    Draft = 1,
    RequirementReview = 2,
    InDevelopment = 3,
    QaTesting = 4,
    ReleaseApproval = 5,
    Released = 6,
    Rejected = 7,
    Closed = 8
}
