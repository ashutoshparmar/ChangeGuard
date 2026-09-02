using System.ComponentModel.DataAnnotations;

using ChangeGuard.Domain.ChangeRequests;

namespace ChangeGuard.Api.Contracts.ChangeRequests;

public sealed class CreateChangeRequestRequest
{
    [Required]
    [StringLength(30)]
    [RegularExpression(
        @"^CG-[0-9]{3,8}$",
        ErrorMessage =
            "Reference number must use the format CG- followed by 3 to 8 digits.")]
    public string ReferenceNumber { get; init; } = string.Empty;

    [Required]
    [StringLength(200, MinimumLength = 5)]
    public string Title { get; init; } = string.Empty;

    [StringLength(4000)]
    public string Description { get; init; } = string.Empty;

    [EnumDataType(typeof(ChangePriority))]
    public ChangePriority Priority { get; init; }

    [StringLength(200)]
    public string Actor { get; init; } = "local-user";
}
