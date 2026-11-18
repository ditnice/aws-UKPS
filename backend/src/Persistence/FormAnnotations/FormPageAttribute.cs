namespace UKPS.Api.Persistence.FormAnnotations;

/// <summary>
/// Places a question on a page declared by a record type's <see cref="FormLayout"/>. The page
/// decides the question's task, section and form, and holds its title, order and "Commercial in
/// confidence" flag.
/// </summary>
/// <param name="id">
/// The ID of the page. Use the layout's constant, such as <c>MedicineFormLayout.BnfChapter</c>.
/// Questions on several entities may share a page; an entity's pages must all be in one form.
/// </param>
[AttributeUsage(AttributeTargets.Property, Inherited = false)]
internal sealed class FormPageAttribute(string id) : Attribute
{
    /// <summary>The ID of the page the question is on.</summary>
    public string Id { get; } = id;
}
