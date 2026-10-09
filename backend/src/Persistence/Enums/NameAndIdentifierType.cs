namespace UKPS.Api.Persistence.Enums;

/// <summary>
/// The type of a product name or identifier. See RecordNameAndIdentifier.NameType.
/// </summary>
public enum NameAndIdentifierType
{
    /// <summary>
    /// The generic name of the substance. Medicines only.
    /// </summary>
    GenericName = 0,

    /// <summary>
    /// Another name, code, or synonym for the product.
    /// </summary>
    OtherIdentifier = 1,
}
