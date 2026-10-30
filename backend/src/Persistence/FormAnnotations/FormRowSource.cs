namespace UKPS.Api.Persistence.FormAnnotations;

/// <summary>
/// How the revision's row for a form section entity comes to exist. Set it with
/// <see cref="FormSectionAttribute.RowSource"/>. If an entity has several sections that
/// disagree, <see cref="RecordCreation"/> wins.
/// </summary>
public enum FormRowSource
{
    /// <summary>
    /// The first save that writes to the entity creates the row. Until then, its questions read
    /// as unanswered. The default.
    /// </summary>
    /// <remarks>
    /// The row is inserted with only <c>RevisionId</c> and the saved answers set, so every other
    /// non-nullable column (apart from the key) needs a database default. Startup rejects an
    /// entity that does not meet this; use <see cref="RecordCreation"/> for it instead.
    /// </remarks>
    FirstSave = 0,

    /// <summary>
    /// Record creation inserts the row; the form only updates it.
    /// </summary>
    /// <remarks>
    /// Use for an entity with required columns that the form cannot fill. Saving a page whose
    /// row is missing fails with an error, because record creation should have inserted it.
    /// </remarks>
    RecordCreation = 1,
}
