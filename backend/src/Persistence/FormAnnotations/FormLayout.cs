using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.FormAnnotations;

/// <summary>
/// The sections and tasks of one record type's form, declared once. Pages join a task by naming
/// its ID in <see cref="FormPageAttribute"/>; the task decides the page's section and form.
/// </summary>
/// <remarks>
/// Subclass it once per record type, with a parameterless constructor; startup finds every
/// subclass. Give each task ID a <c>const string</c> on the subclass so <c>[FormPage]</c> can
/// name it. A task with no pages yet, and a section with no such tasks, are left out of the form.
/// Section and task IDs, titles and orders are part of the form fingerprint, so changing one
/// needs a form version bump.
/// </remarks>
internal abstract class FormLayout(RecordType recordType, IReadOnlyList<FormLayoutSection> sections)
{
    /// <summary>The record type whose form this is.</summary>
    public RecordType RecordType { get; } = recordType;

    /// <summary>The form's sections, each with its tasks.</summary>
    public IReadOnlyList<FormLayoutSection> Sections { get; } = sections;
}
