using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.FormAnnotations;

/// <summary>
/// The sections, tasks and pages of one record type's form, declared once. Questions join a page
/// by naming its ID in <see cref="FormPageAttribute"/>; the page decides their task, section and
/// form.
/// </summary>
/// <remarks>
/// Subclass it once per record type, with a parameterless constructor; startup finds every
/// subclass. Give each page ID a <c>const string</c> on the subclass so <c>[FormPage]</c> can
/// name it. A page with no questions yet, and a task or section with no such pages, are left out
/// of the form. Section, task and page IDs, titles and orders are part of the form fingerprint, so
/// changing one needs a form version bump.
/// </remarks>
internal abstract class FormLayout(RecordType recordType, IReadOnlyList<FormLayoutSection> sections)
{
    /// <summary>The record type whose form this is.</summary>
    public RecordType RecordType { get; } = recordType;

    /// <summary>The form's sections, each with its tasks.</summary>
    public IReadOnlyList<FormLayoutSection> Sections { get; } = sections;
}
