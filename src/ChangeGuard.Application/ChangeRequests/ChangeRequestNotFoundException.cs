namespace ChangeGuard.Application.ChangeRequests;

public sealed class ChangeRequestNotFoundException : Exception
{
    public ChangeRequestNotFoundException(string referenceNumber)
        : base(
            $"No change request with reference number " +
            $"'{referenceNumber}' was found.")
    {
        ReferenceNumber = referenceNumber;
    }

    public string ReferenceNumber { get; }
}
