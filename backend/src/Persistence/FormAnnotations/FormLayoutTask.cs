namespace UKPS.Api.Persistence.FormAnnotations;

/// <summary>A task within a <see cref="FormLayoutSection"/>.</summary>
/// <param name="Id">
/// The task's ID: a lowercase slug such as <c>indication-details</c>
/// (<c>^[a-z0-9]+(-[a-z0-9]+)*$</c>), unique across every form.
/// </param>
/// <param name="Title">
/// The task title, shown in the record task list and as the caption above each page heading. Must
/// not be blank.
/// </param>
/// <param name="Order">
/// Position of the task within its section. No two tasks in a section may share one.
/// </param>
/// <param name="Pages">The task's pages. Must not be empty.</param>
internal sealed record FormLayoutTask(
    string Id,
    string Title,
    int Order,
    IReadOnlyList<FormLayoutPage> Pages
);
