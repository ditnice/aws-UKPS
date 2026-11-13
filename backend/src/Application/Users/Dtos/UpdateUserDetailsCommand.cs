using System.ComponentModel.DataAnnotations;

namespace UKPS.Api.Application.Users.Dtos;

/// <summary>
/// Represents the details to update for an existing user.
/// </summary>
public sealed record UpdateUserDetailsCommand
{
    /// <summary>
    /// Gets the user detail that should be updated.
    /// </summary>
    [Required]
    public required EditableUserDetail Property { get; init; }

    /// <summary>
    /// Gets the new value to assign to the specified <see cref="Property"/>.
    /// </summary>
    /// <remarks>
    /// Cannot be null or empty.
    /// </remarks>
    [Required(AllowEmptyStrings = false)]
    public required string Value { get; init; }
}
