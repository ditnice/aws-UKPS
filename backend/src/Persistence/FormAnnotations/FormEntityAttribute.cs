namespace UKPS.Api.Persistence.FormAnnotations;

/// <summary>
/// Marks a content entity whose properties hold form questions. Every mapped column then needs
/// <see cref="FormFieldAttribute"/> or <see cref="FormIgnoreAttribute"/>. The entity's pages name
/// their task, which decides the form; an entity's pages must all be in one form.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
internal sealed class FormEntityAttribute : Attribute
{
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
