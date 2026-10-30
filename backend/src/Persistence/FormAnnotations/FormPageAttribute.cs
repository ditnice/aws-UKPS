namespace UKPS.Api.Persistence.FormAnnotations;

/// <summary>
/// Places a question on a page. Every declaration of the same page ID must agree on title and
/// order.
/// </summary>
/// <param name="id">
/// The page's ID: a lowercase slug such as <c>bnf-chapter</c> (<c>^[a-z0-9]+(-[a-z0-9]+)*$</c>).
/// It is the last segment of the page URL, so do not change a released ID, and do not use
/// <c>tasks</c>, which is the task list's URL. Questions on several entities may share a page
/// only if the entities are in the same section and task.
/// </param>
/// <param name="title">
/// The page title, shown as the heading when the page has more than one question (a page with
/// one question uses the question's label instead). Must not be blank.
/// </param>
[AttributeUsage(AttributeTargets.Property, Inherited = false)]
internal sealed class FormPageAttribute(string id, string title) : Attribute
{
    /// <summary>The page's lowercase slug ID, used in the page URL.</summary>
    public string Id { get; } = id;

    /// <summary>The page title.</summary>
    public string Title { get; } = title;

    /// <summary>
    /// Position of the page within its task. No two pages in a task may share an order.
    /// </summary>
    public int Order { get; set; }
}
