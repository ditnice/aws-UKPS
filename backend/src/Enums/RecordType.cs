namespace UKPS.Api.Enums;

/// <summary>
/// Represents the type of a record, such as Medicine or Vaccine.
/// </summary>
public enum RecordType
{
    /// <summary>
    /// Indicates that the record is related to medicine.
    /// </summary>
    Medicine = 0,

    /// <summary>
    /// Indicates that the record is related to a vaccine.
    /// </summary>
    Vaccine = 1,
}
