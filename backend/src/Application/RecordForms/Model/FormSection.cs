namespace UKPS.Api.Application.RecordForms.Model;

/// <summary>A titled group of tasks within a form.</summary>
/// <param name="Id">The section's ID.</param>
/// <param name="Title">The section name.</param>
/// <param name="Order">Position of the section within the form.</param>
/// <param name="Tasks">The section's tasks, in order.</param>
/// <param name="Optional">
/// No question in the section is required to submit, even once the user has started a task in it.
/// </param>
/// <param name="SubText">
/// Text shown under the section heading in the record task list, or null for none.
/// </param>
internal sealed record FormSection(
    string Id,
    string Title,
    int Order,
    IReadOnlyList<FormTask> Tasks,
    bool Optional = false,
    string? SubText = null
);
