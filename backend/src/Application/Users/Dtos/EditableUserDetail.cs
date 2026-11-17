namespace UKPS.Api.Application.Users.Dtos;

/// <summary>
/// Specifies the user details that can be edited.
/// </summary>
public enum EditableUserDetail
{
    /// <summary>
    /// The user's full name.
    /// </summary>
    FullName = 0,

    /// <summary>
    /// The user's work phone number.
    /// </summary>
    WorkPhone = 1,
}
