using System.Diagnostics.CodeAnalysis;
using UKPS.Api.Persistence.Entities.RecordWorkflow;
using UKPS.Api.Persistence.Entities.ReferenceData;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Persistence.FormAnnotations;

#pragma warning disable MA0048 // Small test-only entities are kept together.
#pragma warning disable CA1812 // Instantiated by EF metadata only.

namespace UKPS.Api.Tests.Application.RecordForms.Scanning;

/// <summary>A valid entity covering every binding kind.</summary>
[FormSection(RecordType.Vaccine, "valid", "Valid", Order = 10)]
[SuppressMessage("Minor Code Smell", "S1144", Justification = "Read by EF.")]
internal sealed class ValidEntity
{
    [FormIgnore("Database identity")]
    public int Id { get; set; }

    [FormIgnore("Revision ownership")]
    public int RevisionId { get; set; }

    [FormTask("valid-task", "Valid task", Order = 10)]
    [FormPage("text", "Text", Order = 10)]
    [FormField("valid_entity.text", "Text", FormControl.Text, Order = 10)]
    [FormMaxLength(10, "Too long")]
    public string? Text { get; set; }

    [FormTask("valid-task", "Valid task", Order = 10)]
    [FormPage("text", "Text", Order = 10)]
    [FormField("valid_entity.choice", "Choice", FormControl.Radio, Order = 20)]
    public YesNoUnknown? Choice { get; set; }

    [FormTask("valid-task", "Valid task", Order = 10)]
    [FormPage("chapter", "Chapter", Order = 20)]
    [FormField(
        "valid_entity.chapter_id",
        "Chapter",
        FormControl.HierarchicalSelect,
        ChildLabel = "Sub"
    )]
    public int? ChapterId { get; set; }

    public RecordRevision? Revision { get; set; }
    public BnfChapter? Chapter { get; set; }

    [FormTask("valid-task", "Valid task", Order = 10)]
    [FormPage("areas", "Areas", Order = 30)]
    [FormField(
        "valid_entity_area",
        "Areas",
        FormControl.HierarchicalSelect,
        ChildLabel = "Condition",
        AddAnotherLabel = "Add another"
    )]
    [FormOptions(OptionKeys.TherapeuticAreas)]
    [FormMaxItems(3, "Up to 3")]
    public ICollection<ValidEntityArea> Areas { get; set; } = [];
}

internal sealed class ValidEntityArea
{
    public int ValidEntityId { get; set; }
    public int TherapeuticAreaId { get; set; }
    public ValidEntity? ValidEntity { get; set; }
    public TherapeuticArea? TherapeuticArea { get; set; }
}

/// <summary>An enum with no registered option provider.</summary>
internal enum UnprovidedChoice
{
    First = 0,
    Second = 1,
}

/// <summary>Breaks coverage, pairing, ID, control, option and rule checks.</summary>
[FormSection(RecordType.Vaccine, "broken", "Broken", Order = 20)]
internal sealed class BrokenEntity
{
    public int Id { get; set; }

    [FormIgnore("Revision ownership")]
    public int RevisionId { get; set; }

    [FormIgnore(" ")]
    public string? BlankReason { get; set; }

    [FormTask("broken-task", "Broken task", Order = 10)]
    [FormField("broken_entity.no_page", "No page", FormControl.Text)]
    public string? NoPage { get; set; }

    [FormTask("broken-task", "Broken task", Order = 10)]
    [FormPage("Bad Page", "Bad")]
    [FormField("Broken.Bad ID", "Bad ID", FormControl.Text)]
    public string? BadIds { get; set; }

    [FormTask("broken-task", "Broken task", Order = 10)]
    [FormPage("broken", "Broken", Order = 10)]
    [FormField("broken_entity.wrong_control", "Wrong control", FormControl.Radio)]
    public string? WrongControl { get; set; }

    [FormTask("broken-task", "Broken task", Order = 10)]
    [FormPage("broken", "Broken", Order = 10)]
    [FormField("broken_entity.choice_without_provider", "Choice", FormControl.Radio, Order = 1)]
    public UnprovidedChoice? ChoiceWithoutProvider { get; set; }

    [FormTask("broken-task", "Broken task", Order = 10)]
    [FormPage("broken", "Broken", Order = 10)]
    [FormField("broken_entity.wrong_enum", "Wrong enum", FormControl.Radio, Order = 2)]
    [FormOptions(OptionKeys.PaediatricStatus)]
    [FormMaxLength(5, "Not for choices")]
    public YesNoUnknown? WrongEnum { get; set; }

    [FormTask("broken-task", "Broken task", Order = 10)]
    [FormPage("broken", "Broken", Order = 10)]
    [FormField("broken_entity.unknown_key", "Unknown key", FormControl.Radio, Order = 3)]
    [FormOptions("no-such-provider")]
    public YesNoUnknown? UnknownKey { get; set; }

    [FormPage("no-task", "No task", Order = 20)]
    [FormField("broken_entity.no_task", "No task", FormControl.Text)]
    public string? NoTask { get; set; }

    [FormTask("Bad Task", " ", Order = 20)]
    [FormPage("bad-task", "Bad task", Order = 30)]
    [FormField("broken_entity.bad_task", "Bad task", FormControl.Text)]
    public string? BadTask { get; set; }

    [FormTask("broken-task", "Broken task", Order = 10)]
    [FormPage("task-list", "Task list", Order = 40)]
    [FormField("broken_entity.reserved_page", "Reserved page", FormControl.Text)]
    public string? ReservedPage { get; set; }

    public RecordRevision? Revision { get; set; }
}

/// <summary>A FirstSave entity with a required column, and no RevisionId.</summary>
[FormSection(RecordType.Vaccine, "rows", "Rows", Order = 30)]
internal sealed class RowsEntity
{
    [FormIgnore("Database identity")]
    public int Id { get; set; }

    [FormTask("rows-task", "Rows task", Order = 10)]
    [FormPage("rows", "Rows", Order = 10)]
    [FormField("rows_entity.required_name", "Name", FormControl.Text)]
    [FormRequired("Enter a name")]
    public required string RequiredName { get; set; }
}

/// <summary>Disagrees with <see cref="ValidEntity"/> about section and page metadata.</summary>
[FormSection(RecordType.Vaccine, "valid", "Different title", Order = 10)]
internal sealed class DisagreeingEntity
{
    [FormIgnore("Database identity")]
    public int Id { get; set; }

    [FormIgnore("Revision ownership")]
    public int RevisionId { get; set; }

    [FormTask("valid-task", "Valid task", Order = 10)]
    [FormPage("text", "Different page title", Order = 10)]
    [FormField("valid_entity.text", "Duplicate ID", FormControl.Text, Order = 30)]
    public string? Text { get; set; }

    public RecordRevision? Revision { get; set; }
}

/// <summary>
/// Disagrees with <see cref="ValidEntity"/> about task metadata, and puts one of its pages in
/// another task with the same order.
/// </summary>
[FormSection(RecordType.Vaccine, "valid", "Valid", Order = 10)]
internal sealed class TaskConflictEntity
{
    [FormIgnore("Database identity")]
    public int Id { get; set; }

    [FormIgnore("Revision ownership")]
    public int RevisionId { get; set; }

    [FormTask("valid-task", "Different task title", Order = 10)]
    [FormPage("extra", "Extra", Order = 40)]
    [FormField("task_conflict_entity.extra", "Extra", FormControl.Text)]
    public string? Extra { get; set; }

    [FormTask("other-task", "Other task", Order = 10)]
    [FormPage("text", "Text", Order = 10)]
    [FormField("task_conflict_entity.moved", "Moved", FormControl.Text, Order = 30)]
    public string? Moved { get; set; }

    public RecordRevision? Revision { get; set; }
}
