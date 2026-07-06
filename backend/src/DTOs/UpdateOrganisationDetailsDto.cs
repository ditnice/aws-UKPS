using System.ComponentModel.DataAnnotations;

namespace UKPS.Api.DTOs;

public sealed record UpdateOrganisationDetailsDto : IValidatableObject
{
    [Required]
    public string OrganisationName { get; init; } = null!;

    // Allow the custom whitespace check below to handle empty textarea values consistently.
    [Required(AllowEmptyStrings = true)]
    public string HeadOfficeAddress { get; init; } = null!;

    [Required]
    [EmailAddress]
    public string HeadOfficeEmail { get; init; } = null!;

    [Required]
    public string HeadOfficeTelephone { get; init; } = null!;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(HeadOfficeAddress))
        {
            yield return new ValidationResult(
                "HeadOfficeAddress cannot be empty or whitespace.",
                [nameof(HeadOfficeAddress)]
            );
        }
    }
}
