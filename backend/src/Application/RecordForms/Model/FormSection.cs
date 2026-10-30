namespace UKPS.Api.Application.RecordForms.Model;

/// <summary>A titled group of tasks within a form.</summary>
internal sealed record FormSection(
    string Id,
    string Title,
    int Order,
    IReadOnlyList<FormTask> Tasks
);
