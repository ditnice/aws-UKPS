namespace UKPS.Api.Enums;

/// <summary>
/// Represents the different roles a user can have within the system.
/// </summary>
public enum UserRole
{
    /// <summary>
    /// A standard user with basic access rights.
    /// </summary>
    Standard = 0,

    /// <summary>
    /// A champion user with elevated privileges or responsibilities.
    /// </summary>
    Champion = 1,

    /// <summary>
    /// A super user with the highest level of access and permissions.
    /// </summary>
    Super = 2,
}
