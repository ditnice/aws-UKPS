namespace UKPS.Api.Persistence.Enums;

/// <summary>
/// Represents the different types of users in the system.
/// </summary>
public enum UserType
{
    /// <summary>
    /// Represents a pharmaceutical user.
    /// </summary>
    PharmaUser = 0,

    /// <summary>
    /// Represents a horizon scanner user.
    /// </summary>
    HorizonScanner = 1,

    /// <summary>
    /// Represents a strategic user.
    /// </summary>
    StrategicUser = 2,

    /// <summary>
    /// Represents a quality assurance user.
    /// </summary>
    QaUser = 3,

    /// <summary>
    /// Represents an IT administrator.
    /// </summary>
    ItAdmin = 4,
}
