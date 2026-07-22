namespace UKPS.Api.Persistence.Enums;

/// <summary>
/// Represents the types of events that can occur in the IAM (Identity and Access Management) system.
/// </summary>
public enum IamEventType
{
    /// <summary>
    /// Indicates that a field has been updated.
    /// </summary>
    FieldUpdated = 0,

    /// <summary>
    /// Indicates that the status has changed.
    /// </summary>
    StatusChanged = 1,

    /// <summary>
    /// Indicates that the role has changed.
    /// </summary>
    RoleChanged = 2,

    /// <summary>
    /// Indicates that the entity has been disabled.
    /// </summary>
    Disabled = 3,

    /// <summary>
    /// Indicates that the entity has been enabled.
    /// </summary>
    Enabled = 4,

    /// <summary>
    /// Indicates that the entity has been created.
    /// </summary>
    Created = 5,

    /// <summary>
    /// Indicates that additional information has been requested.
    /// </summary>
    InformationRequested = 6,

    /// <summary>
    /// Indicates that terms have been requested.
    /// </summary>
    TermsRequested = 7,

    /// <summary>
    /// Indicates that terms have been signed.
    /// </summary>
    TermsSigned = 8,

    /// <summary>
    /// Indicates that terms have expired.
    /// </summary>
    TermsExpired = 9,
}
