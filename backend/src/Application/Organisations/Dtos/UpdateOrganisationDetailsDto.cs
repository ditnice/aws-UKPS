using System.ComponentModel.DataAnnotations;

namespace UKPS.Api.Application.Organisations.Dtos;

/// <summary>
/// Represents the data transfer object for updating organisation details.
/// </summary>
public sealed record UpdateOrganisationDetailsDto : IValidatableObject
{
    /// <summary>
    /// Gets or sets the name of the organisation.
    /// </summary>
    /// <remarks>
    /// This field is required.
    /// </remarks>
    [Required]
    public required string OrganisationName { get; init; }

    /// <summary>
    /// Gets or sets the head office address of the organisation.
    /// </summary>
    /// <remarks>
    /// This field is required. Empty strings are allowed, but whitespace-only values are not.
    /// </remarks>
    [Required(AllowEmptyStrings = true)]
    public required string HeadOfficeAddress { get; init; }

    /// <summary>
    /// Gets or sets the email address of the head office.
    /// </summary>
    /// <remarks>
    /// This field is required and must be a valid email address.
    /// </remarks>
    [Required]
    [EmailAddress]
    public required string HeadOfficeEmail { get; init; }

    /// <summary>
    /// Gets or sets the telephone number of the head office.
    /// </summary>
    /// <remarks>
    /// This field is required.
    /// </remarks>
    [Required]
    public required string HeadOfficeTelephone { get; init; }

    /// <summary>
    /// Validates the object to ensure all properties meet the required conditions.
    /// </summary>
    /// <param name="validationContext">The context in which the validation is performed.</param>
    /// <returns>
    /// A collection of <see cref="ValidationResult"/> containing validation errors, if any.
    /// </returns>
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
