using System.ComponentModel.DataAnnotations;

namespace UKPS.Api.DTOs;

public sealed class UpdateOrganisationDetailsDto : IValidatableObject
{
    [Required]
    public required string OrganisationName { get; init; }

    // Allow the custom whitespace check below to handle empty textarea values consistently.
    [Required(AllowEmptyStrings = true)]
    public required string HeadOfficeAddress { get; init; }

    [Required]
    [EmailAddress]
    public required string HeadOfficeEmail { get; init; }

    [Required]
    public required string HeadOfficeTelephone { get; init; }

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
