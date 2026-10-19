namespace UKPS.Api.Persistence.FormAnnotations;

/// <summary>
/// Places a question on a page. Every declaration of the same page ID must agree on title and
/// order.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false)]
internal sealed class FormPageAttribute(string id, string title) : Attribute
{
    public string Id { get; } = id;
    public string Title { get; } = title;

    /// <summary>Position of the page within its section.</summary>
    public int Order { get; set; }
}
