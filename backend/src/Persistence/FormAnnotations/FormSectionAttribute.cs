using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.FormAnnotations;

/// <summary>
/// Puts an entity's questions into a section of a record type's form. One per record type.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
internal sealed class FormSectionAttribute(RecordType recordType, string id, string title)
    : Attribute
{
    public RecordType RecordType { get; } = recordType;
    public string Id { get; } = id;
    public string Title { get; } = title;

    /// <summary>Position of the section within the form.</summary>
    public int Order { get; set; }

    public FormRowSource RowSource { get; set; } = FormRowSource.FirstSave;
}
