namespace UKPS.Api.Persistence.FormAnnotations;

/// <summary>
/// How the revision's row for a form section entity comes to exist.
/// </summary>
public enum FormRowSource
{
    /// <summary>The first save that writes to the entity creates the row.</summary>
    FirstSave = 0,

    /// <summary>Record creation inserts the row; the form only updates it.</summary>
    RecordCreation = 1,
}
