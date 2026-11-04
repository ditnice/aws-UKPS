namespace UKPS.Api.Persistence.FormAnnotations;

/// <summary>
/// Places a page and its questions into a task within the entity's section. Every declaration of
/// the same task ID must agree on title and order.
/// </summary>
/// <param name="id">
/// The task's ID: a lowercase slug such as <c>indication-details</c>
/// (<c>^[a-z0-9]+(-[a-z0-9]+)*$</c>).
/// </param>
/// <param name="title">The task title shown in the record task list. Must not be blank.</param>
[AttributeUsage(AttributeTargets.Property, Inherited = false)]
internal sealed class FormTaskAttribute(string id, string title) : Attribute
{
    /// <summary>The task's lowercase slug ID.</summary>
    public string Id { get; } = id;

    /// <summary>The task title.</summary>
    public string Title { get; } = title;

    /// <summary>Position of the task within its section.</summary>
    public int Order { get; set; }
}
