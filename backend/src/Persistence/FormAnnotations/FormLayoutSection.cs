namespace UKPS.Api.Persistence.FormAnnotations;

/// <summary>A section of a <see cref="FormLayout"/>.</summary>
/// <param name="Id">
/// The section's ID: a lowercase slug such as <c>indication-and-development</c>
/// (<c>^[a-z0-9]+(-[a-z0-9]+)*$</c>), unique within the form.
/// </param>
/// <param name="Title">
/// The section name, shown as a heading in the record task list above the section's tasks. Must
/// not be blank.
/// </param>
/// <param name="Order">Position of the section within the form. No two sections may share one.</param>
/// <param name="Tasks">The section's tasks.</param>
internal sealed record FormLayoutSection(
    string Id,
    string Title,
    int Order,
    IReadOnlyList<FormLayoutTask> Tasks
);
