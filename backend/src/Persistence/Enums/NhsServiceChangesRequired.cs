namespace UKPS.Api.Persistence.Enums;

/// <summary>
/// The extent of NHS service change required to deliver a medicine.
/// </summary>
public enum NhsServiceChangesRequired
{
    /// <summary>The extent of service change required is not yet known.</summary>
    Unknown = 0,

    /// <summary>No changes to the existing NHS service are required.</summary>
    NoChanges = 1,

    /// <summary>Some change to the existing NHS service is required.</summary>
    SomeChange = 2,

    /// <summary>Complete transformation of the NHS service is required.</summary>
    CompleteTransformation = 3,
}
