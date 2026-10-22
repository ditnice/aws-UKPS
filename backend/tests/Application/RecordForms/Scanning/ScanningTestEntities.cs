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

    [FormPage("text", "Text", Order = 10)]
    [FormField("valid_entity.text", "Text", Order = 10)]
    [FormMaxLength(10, "Too long")]
    public string? Text { get; set; }

    [FormPage("text", "Text", Order = 10)]
    [FormField("valid_entity.choice", "Choice", Order = 20)]
    [FormOptions(OptionKeys.YesNoUnknown)]
    public YesNoUnknown? Choice { get; set; }

    [FormPage("chapter", "Chapter", Order = 20)]
    [FormField(
        "valid_entity.chapter_id",
        "Chapter",
        Control = FormControl.HierarchicalSelect,
        ChildLabel = "Sub"
    )]
    [FormOptions(OptionKeys.BnfChapters)]
    public int? ChapterId { get; set; }

    public RecordRevision? Revision { get; set; }
    public BnfChapter? Chapter { get; set; }

    [FormPage("areas", "Areas", Order = 30)]
    [FormField(
        "valid_entity_area",
        "Areas",
        Control = FormControl.HierarchicalSelect,
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

/// <summary>Breaks coverage, pairing, ID, control, option and rule checks.</summary>
[FormSection(RecordType.Vaccine, "broken", "Broken", Order = 20)]
internal sealed class BrokenEntity
{
    public int Id { get; set; }

    [FormIgnore("Revision ownership")]
    public int RevisionId { get; set; }

    [FormIgnore(" ")]
    public string? BlankReason { get; set; }

    [FormField("broken_entity.no_page", "No page")]
    public string? NoPage { get; set; }

    [FormPage("Bad Page", "Bad")]
    [FormField("Broken.Bad ID", "Bad ID")]
    public string? BadIds { get; set; }

    [FormPage("broken", "Broken", Order = 10)]
    [FormField("broken_entity.wrong_control", "Wrong control", Control = FormControl.Radio)]
    public string? WrongControl { get; set; }

    [FormPage("broken", "Broken", Order = 10)]
    [FormField("broken_entity.reference_without_control", "Reference")]
    [FormOptions(OptionKeys.BnfChapters)]
    public int? ReferenceWithoutControlId { get; set; }

    [FormPage("broken", "Broken", Order = 10)]
    [FormField("broken_entity.choice_without_options", "Choice", Order = 1)]
    public YesNoUnknown? ChoiceWithoutOptions { get; set; }

    [FormPage("broken", "Broken", Order = 10)]
    [FormField("broken_entity.wrong_enum", "Wrong enum", Order = 2)]
    [FormOptions(OptionKeys.PaediatricStatus)]
    [FormMaxLength(5, "Not for choices")]
    public YesNoUnknown? WrongEnum { get; set; }

    [FormPage("broken", "Broken", Order = 10)]
    [FormField("broken_entity.unknown_key", "Unknown key", Order = 3)]
    [FormOptions("no-such-provider")]
    public YesNoUnknown? UnknownKey { get; set; }

    public RecordRevision? Revision { get; set; }
    public BnfChapter? ReferenceWithoutControl { get; set; }
}

/// <summary>A FirstSave entity with a required column, and no RevisionId.</summary>
[FormSection(RecordType.Vaccine, "rows", "Rows", Order = 30)]
internal sealed class RowsEntity
{
    [FormIgnore("Database identity")]
    public int Id { get; set; }

    [FormPage("rows", "Rows", Order = 10)]
    [FormField("rows_entity.required_name", "Name")]
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

    [FormPage("text", "Different page title", Order = 10)]
    [FormField("valid_entity.text", "Duplicate ID", Order = 30)]
    public string? Text { get; set; }

    public RecordRevision? Revision { get; set; }
}
