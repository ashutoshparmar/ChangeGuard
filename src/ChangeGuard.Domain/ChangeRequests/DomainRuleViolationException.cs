namespace ChangeGuard.Domain.ChangeRequests;

public sealed class DomainRuleViolationException : Exception
{
    public DomainRuleViolationException(string message)
        : base(message)
    {
    }
}
