namespace ChangeGuard.Application.ChangeRequests;

public sealed class DuplicateChangeRequestException : Exception
{
    public DuplicateChangeRequestException(
        string referenceNumber)
        : base(
            $"A change request with reference number " +
            $"'{referenceNumber}' already exists.")
    {
        ReferenceNumber = referenceNumber;
    }

    public DuplicateChangeRequestException(
        string referenceNumber,
        Exception innerException)
        : base(
            $"A change request with reference number " +
            $"'{referenceNumber}' already exists.",
            innerException)
    {
        ReferenceNumber = referenceNumber;
    }

    public string ReferenceNumber { get; }
}