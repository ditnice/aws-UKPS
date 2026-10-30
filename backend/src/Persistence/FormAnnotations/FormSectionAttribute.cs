using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.FormAnnotations;

/// <summary>
/// Puts an entity's questions into a section of a record type's form. One per record type.
/// </summary>
/// <param name="recordType">
/// The record type whose form the section belongs to. Repeat the attribute with a different
/// record type to share the entity between forms; two sections for the same record type are
/// rejected at startup.
/// </param>
/// <param name="id">
/// The section's ID: a lowercase slug such as <c>indication-details</c>
/// (<c>^[a-z0-9]+(-[a-z0-9]+)*$</c>).
/// </param>
/// <param name="title">
/// The section name, shown as the caption above each page heading. Must not be blank.
/// </param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
internal sealed class FormSectionAttribute(RecordType recordType, string id, string title)
    : Attribute
{
    /// <summary>The record type whose form the section belongs to.</summary>
    public RecordType RecordType { get; } = recordType;

    /// <summary>The section's lowercase slug ID.</summary>
    public string Id { get; } = id;

    /// <summary>The section name, shown as the caption above each page heading.</summary>
    public string Title { get; } = title;

    /// <summary>Position of the section within the form.</summary>
    public int Order { get; set; }

    /// <summary>
    /// How the revision's row for the entity comes to exist. Defaults to
    /// <see cref="FormRowSource.FirstSave"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="FormRowSource.FirstSave"/> inserts the row on the first save, so every
    /// non-nullable column other than the key and <c>RevisionId</c> needs a database default. If
    /// it does not, use <see cref="FormRowSource.RecordCreation"/>.
    /// </remarks>
    public FormRowSource RowSource { get; set; } = FormRowSource.FirstSave;
}
