namespace ChangeGuard.Domain.ChangeRequests;

public enum ChangeRequestWorkflowAction
{
    SubmitForReview = 1,
    StartDevelopment = 2,
    StartQaTesting = 3,
    CompleteQaTesting = 4,
    ApproveRelease = 5,
    Reject = 6,
    Close = 7
}
