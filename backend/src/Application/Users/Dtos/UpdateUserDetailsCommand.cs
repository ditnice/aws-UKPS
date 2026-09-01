using System.ComponentModel.DataAnnotations;
using UKPS.Api.WebApi.Validators;

namespace UKPS.Api.Application.Users.Dtos;

/// <summary>
/// Represents the details to update for an existing user.
/// </summary>
public sealed record UpdateUserDetailsCommand
{
    /// <summary>
    /// Gets the user's full name.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public required string FullName { get; init; }

    /// <summary>
    /// Gets the user's work email address.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    [EmailAddress]
    public required string WorkEmail { get; init; }

    /// <summary>
    /// Gets the user's work telephone number.
    /// </summary>
    [Required]
    [PhoneNumber]
    public required string WorkTelephone { get; init; }
}
