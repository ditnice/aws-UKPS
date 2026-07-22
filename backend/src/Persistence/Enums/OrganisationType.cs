namespace UKPS.Api.Persistence.Enums;

/// <summary>
/// Represents the different types of organisations within the system.
/// </summary>
public enum OrganisationType
{
    /// <summary>
    /// Represents a pharmaceutical company.
    /// </summary>
    PharmaCompany = 0,

    /// <summary>
    /// Represents an organisation involved in horizon scanning activities.
    /// </summary>
    HorizonScanning = 1,

    /// <summary>
    /// Represents a strategic organisation.
    /// </summary>
    Strategic = 2,

    /// <summary>
    /// Represents an internal organisation.
    /// </summary>
    Internal = 3,
}
