using System.Diagnostics.CodeAnalysis;
using UKPS.Api.Persistence.Entities.MedicinesRevisionContent;
using UKPS.Api.Persistence.Entities.RecordWorkflow;
using UKPS.Api.Persistence.Entities.ReferenceData;
using UKPS.Api.Persistence.Entities.SharedRevisionContent;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Persistence.FormAnnotations;

#pragma warning disable MA0048 // Small test-only entities are kept together.
#pragma warning disable CA1812 // Instantiated by EF metadata only.

namespace UKPS.Api.Tests.Application.RecordForms.Scanning;

/// <summary>A valid entity covering every binding kind.</summary>
[FormEntity]
[SuppressMessage("Minor Code Smell", "S1144", Justification = "Read by EF.")]
internal sealed class ValidEntity
{
    [FormIgnore("Database identity")]
    public int Id { get; set; }

    [FormIgnore("Revision ownership")]
    public int RevisionId { get; set; }

    [FormPage("valid-task", "text", "Text", Order = 10)]
    [FormField("valid_entity.text", "Text", FormControl.Text, Order = 10)]
    [FormMaxLength(10, "Too long")]
    public string? Text { get; set; }

    [FormPage("valid-task", "text", "Text", Order = 10)]
    [FormField("valid_entity.choice", "Choice", FormControl.Radio, Order = 20)]
    public YesNoUnknown? Choice { get; set; }

    [FormPage("valid-task", "chapter", "Chapter", Order = 20)]
    [FormField(
        "valid_entity.chapter_id",
        "Chapter",
        FormControl.HierarchicalSelect,
        ChildLabel = "Sub"
    )]
    public int? ChapterId { get; set; }

    public RecordRevision? Revision { get; set; }
    public BnfChapter? Chapter { get; set; }

    [FormPage("valid-task", "areas", "Areas", Order = 30)]
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
[FormEntity]
internal sealed class BrokenEntity
{
    public int Id { get; set; }

    [FormIgnore("Revision ownership")]
    public int RevisionId { get; set; }

    [FormIgnore(" ")]
    public string? BlankReason { get; set; }

    [FormField("broken_entity.no_page", "No page", FormControl.Text)]
    public string? NoPage { get; set; }

    [FormPage("broken-task", "Bad Page", "Bad")]
    [FormField("Broken.Bad ID", "Bad ID", FormControl.Text)]
    public string? BadIds { get; set; }

    [FormPage("broken-task", "broken", "Broken", Order = 10)]
    [FormField("broken_entity.wrong_control", "Wrong control", FormControl.Radio)]
    public string? WrongControl { get; set; }

    [FormPage("broken-task", "broken", "Broken", Order = 10)]
    [FormField("broken_entity.choice_without_provider", "Choice", FormControl.Radio, Order = 1)]
    public UnprovidedChoice? ChoiceWithoutProvider { get; set; }

    [FormPage("broken-task", "broken", "Broken", Order = 10)]
    [FormField("broken_entity.wrong_enum", "Wrong enum", FormControl.Radio, Order = 2)]
    [FormOptions(OptionKeys.PaediatricStatus)]
    [FormMaxLength(5, "Not for choices")]
    public YesNoUnknown? WrongEnum { get; set; }

    [FormPage("broken-task", "broken", "Broken", Order = 10)]
    [FormField("broken_entity.unknown_key", "Unknown key", FormControl.Radio, Order = 3)]
    [FormOptions("no-such-provider")]
    public YesNoUnknown? UnknownKey { get; set; }

    [FormPage("no-such-task", "unknown-task", "Unknown task", Order = 20)]
    [FormField("broken_entity.unknown_task", "Unknown task", FormControl.Text)]
    public string? UnknownTask { get; set; }

    [FormPage("broken-task", "task-list", "Task list", Order = 40)]
    [FormField("broken_entity.reserved_page", "Reserved page", FormControl.Text)]
    public string? ReservedPage { get; set; }

    [FormPage("broken-task", "broken-prefixes", "Broken prefixes", Order = 50)]
    [FormField("broken_entity.prefixes_on_text", "Prefixes on text", FormControl.Text, Order = 1)]
    [FormValuePrefixes("GID-TA")]
    public string? PrefixesOnText { get; set; }

    [FormPage("broken-task", "broken-prefixes", "Broken prefixes", Order = 50)]
    [FormField(
        "broken_entity.prefixed_without_prefixes",
        "Prefixed without prefixes",
        FormControl.PrefixedNumber,
        Order = 2
    )]
    public string? PrefixedWithoutPrefixes { get; set; }

    [FormPage("broken-task", "broken-prefixes", "Broken prefixes", Order = 50)]
    [FormField(
        "broken_entity.digit_ending_prefix",
        "Digit ending prefix",
        FormControl.PrefixedNumber,
        Order = 3
    )]
    [FormValuePrefixes("GID-TA1")]
    public string? DigitEndingPrefix { get; set; }

    [FormPage("broken-task", "broken-prefixes", "Broken prefixes", Order = 50)]
    [FormField(
        "broken_entity.repeated_prefix",
        "Repeated prefix",
        FormControl.PrefixedNumber,
        Order = 4
    )]
    [FormValuePrefixes("GID-TA", "GID-TA")]
    public string? RepeatedPrefix { get; set; }

    [FormPage("broken-task", "broken-prefixes", "Broken prefixes", Order = 50)]
    [FormField(
        "broken_entity.overlapping_prefix",
        "Overlapping prefix",
        FormControl.PrefixedNumber,
        Order = 5
    )]
    [FormValuePrefixes("GID", "GID-TA")]
    public string? OverlappingPrefix { get; set; }

    public RecordRevision? Revision { get; set; }
}

/// <summary>A FirstSave entity with a required column, and no RevisionId.</summary>
[FormEntity]
internal sealed class RowsEntity
{
    [FormIgnore("Database identity")]
    public int Id { get; set; }

    [FormPage("rows-task", "rows", "Rows", Order = 10)]
    [FormField("rows_entity.required_name", "Name", FormControl.Text)]
    [FormRequired("Enter a name")]
    public required string RequiredName { get; set; }
}

/// <summary>Disagrees with <see cref="ValidEntity"/> about page metadata and reuses a question ID.</summary>
[FormEntity]
internal sealed class DisagreeingEntity
{
    [FormIgnore("Database identity")]
    public int Id { get; set; }

    [FormIgnore("Revision ownership")]
    public int RevisionId { get; set; }

    [FormPage("valid-task", "text", "Different page title", Order = 10)]
    [FormField("valid_entity.text", "Duplicate ID", FormControl.Text, Order = 30)]
    public string? Text { get; set; }

    public RecordRevision? Revision { get; set; }
}

/// <summary>Agrees with <see cref="ValidEntity"/> except for a page's "Commercial in confidence" flag.</summary>
[FormEntity]
internal sealed class FlagDisagreeingEntity
{
    [FormIgnore("Database identity")]
    public int Id { get; set; }

    [FormIgnore("Revision ownership")]
    public int RevisionId { get; set; }

    [FormPage("valid-task", "text", "Text", Order = 10, CommercialInConfidence = true)]
    [FormField("flag_entity.text", "Text", FormControl.Text, Order = 30)]
    public string? Text { get; set; }

    public RecordRevision? Revision { get; set; }
}

/// <summary>Puts one of <see cref="ValidEntity"/>'s pages in another task.</summary>
[FormEntity]
internal sealed class TaskConflictEntity
{
    [FormIgnore("Database identity")]
    public int Id { get; set; }

    [FormIgnore("Revision ownership")]
    public int RevisionId { get; set; }

    [FormPage("other-task", "text", "Text", Order = 10)]
    [FormField("task_conflict_entity.moved", "Moved", FormControl.Text, Order = 30)]
    public string? Moved { get; set; }

    public RecordRevision? Revision { get; set; }
}

/// <summary>
/// Valid conditions: a reveal on the same page, a page shown by an earlier answer, and a reveal
/// on that page, which chains.
/// </summary>
[FormEntity]
[FormPageShowWhen("follow-up", "conditional_entity.gate", YesNoUnknown.Yes, YesNoUnknown.Unknown)]
[SuppressMessage("Minor Code Smell", "S1144", Justification = "Read by EF.")]
internal sealed class ConditionalEntity
{
    [FormIgnore("Database identity")]
    public int Id { get; set; }

    [FormIgnore("Revision ownership")]
    public int RevisionId { get; set; }

    [FormPage("conditional-task", "gate", "Gate", Order = 10)]
    [FormField("conditional_entity.gate", "Gate", FormControl.Radio, Order = 10)]
    public YesNoUnknown? Gate { get; set; }

    [FormPage("conditional-task", "gate", "Gate", Order = 10)]
    [FormField("conditional_entity.reason", "Reason", FormControl.Text, Order = 20)]
    [FormShowWhen("conditional_entity.gate", YesNoUnknown.No)]
    public string? Reason { get; set; }

    [FormPage("conditional-task", "follow-up", "Follow up", Order = 20)]
    [FormField("conditional_entity.detail", "Detail", FormControl.Radio, Order = 10)]
    public YesNoUnknown? Detail { get; set; }

    [FormPage("conditional-task", "follow-up", "Follow up", Order = 20)]
    [FormField("conditional_entity.notes", "Notes", FormControl.Text, Order = 20)]
    [FormShowWhen("conditional_entity.detail", YesNoUnknown.Yes)]
    public string? Notes { get; set; }

    [FormPage("conditional-task", "end", "End", Order = 30)]
    [FormField("conditional_entity.end", "End", FormControl.Text)]
    public string? End { get; set; }

    public RecordRevision? Revision { get; set; }
}

/// <summary>Breaks each condition check.</summary>
[FormEntity(RowSource = FormRowSource.RecordCreation)]
[FormPageShowWhen("first", "broken_condition_entity.later", YesNoUnknown.Yes)]
[FormPageShowWhen("undeclared", "broken_condition_entity.trigger", YesNoUnknown.Yes)]
[FormPageShowWhen("second", "broken_condition_entity.trigger", YesNoUnknown.Yes)]
[FormPageShowWhen("second", "broken_condition_entity.trigger", YesNoUnknown.No)]
[SuppressMessage("Minor Code Smell", "S1144", Justification = "Read by EF.")]
internal sealed class BrokenConditionEntity
{
    [FormIgnore("Database identity")]
    public int Id { get; set; }

    [FormIgnore("Revision ownership")]
    public int RevisionId { get; set; }

    [FormPage("broken-conditions-task", "first", "First", Order = 10)]
    [FormField("broken_condition_entity.trigger", "Trigger", FormControl.Radio, Order = 10)]
    public YesNoUnknown? Trigger { get; set; }

    [FormPage("broken-conditions-task", "first", "First", Order = 10)]
    [FormField("broken_condition_entity.unknown_trigger", "Unknown", FormControl.Text, Order = 20)]
    [FormShowWhen("no_such.question", YesNoUnknown.Yes)]
    public string? UnknownTrigger { get; set; }

    [FormPage("broken-conditions-task", "first", "First", Order = 10)]
    [FormField("broken_condition_entity.before_trigger", "Before", FormControl.Text, Order = 30)]
    [FormShowWhen("broken_condition_entity.after", YesNoUnknown.Yes)]
    public string? BeforeTrigger { get; set; }

    [FormPage("broken-conditions-task", "first", "First", Order = 10)]
    [FormField("broken_condition_entity.after", "After", FormControl.Radio, Order = 40)]
    public YesNoUnknown? After { get; set; }

    [FormPage("broken-conditions-task", "first", "First", Order = 10)]
    [FormField("broken_condition_entity.text_trigger", "Text", FormControl.Text, Order = 50)]
    [FormShowWhen("broken_condition_entity.before_trigger", YesNoUnknown.Yes)]
    public string? TextTrigger { get; set; }

    [FormPage("broken-conditions-task", "first", "First", Order = 10)]
    [FormField("broken_condition_entity.wrong_value", "Wrong", FormControl.Text, Order = 60)]
    [FormShowWhen("broken_condition_entity.trigger", IndicationPaediatricStatus.Unknown)]
    public string? WrongValue { get; set; }

    [FormPage("broken-conditions-task", "first", "First", Order = 10)]
    [FormField("broken_condition_entity.no_values", "None", FormControl.Text, Order = 70)]
    [FormShowWhen("broken_condition_entity.trigger")]
    public string? NoValues { get; set; }

    [FormPage("broken-conditions-task", "first", "First", Order = 10)]
    [FormField("broken_condition_entity.repeated", "Repeated", FormControl.Text, Order = 80)]
    [FormShowWhen("broken_condition_entity.trigger", YesNoUnknown.Yes, YesNoUnknown.Yes)]
    public string? Repeated { get; set; }

    [FormPage("broken-conditions-task", "first", "First", Order = 10)]
    [FormField("broken_condition_entity.non_nullable", "Name", FormControl.Text, Order = 90)]
    [FormShowWhen("broken_condition_entity.trigger", YesNoUnknown.Yes)]
    [FormRequired("Enter a name")]
    public required string NonNullable { get; set; }

    [FormPage("broken-conditions-task", "second", "Second", Order = 20)]
    [FormField("broken_condition_entity.later", "Later", FormControl.Radio, Order = 10)]
    public YesNoUnknown? Later { get; set; }

    [FormPage("broken-conditions-task", "second", "Second", Order = 20)]
    [FormField("broken_condition_entity.other_page", "Other page", FormControl.Text, Order = 20)]
    [FormShowWhen("broken_condition_entity.trigger", YesNoUnknown.Yes)]
    public string? OtherPage { get; set; }

    public RecordRevision? Revision { get; set; }
}

/// <summary>
/// A second entity in <see cref="ConditionalEntity"/>'s task: a page shown by its gate, and a
/// page that depends on nothing.
/// </summary>
[FormEntity]
[FormPageShowWhen("extra", "conditional_entity.gate", YesNoUnknown.Yes)]
internal sealed class ConditionalExtraEntity
{
    [FormIgnore("Database identity")]
    public int Id { get; set; }

    [FormIgnore("Revision ownership")]
    public int RevisionId { get; set; }

    [FormPage("conditional-task", "extra", "Extra", Order = 40)]
    [FormField("conditional_extra_entity.extra", "Extra", FormControl.Text)]
    public string? Extra { get; set; }

    [FormPage("conditional-task", "other", "Other", Order = 50)]
    [FormField("conditional_extra_entity.other", "Other", FormControl.Text)]
    public string? Other { get; set; }

    public RecordRevision? Revision { get; set; }
}

/// <summary>Valid date questions, and a condition on a date's precision.</summary>
[FormEntity]
[FormPageShowWhen("actual", "dated_entity.submission_date", DatePrecision.ActualDate)]
[SuppressMessage("Minor Code Smell", "S1144", Justification = "Read by EF.")]
internal sealed class DatedEntity
{
    [FormIgnore("Database identity")]
    public int Id { get; set; }

    [FormIgnore("Revision ownership")]
    public int RevisionId { get; set; }

    [FormPage("dated-task", "submission", "Submission", Order = 10)]
    [FormField("dated_entity.submission_date", "Submission", FormControl.PrecisionDate)]
    [FormDateEvent(DateEventType.UkSubmission, Name = "submission")]
    public int? SubmissionDateId { get; set; }

    [FormPage("dated-task", "actual", "Actual", Order = 20)]
    [FormField("dated_entity.actual_date", "Actual", FormControl.PrecisionDate)]
    [FormDateEvent(DateEventType.GlobalFirstSubmission, Name = "first submission")]
    public int? ActualDateId { get; set; }

    public RecordRevision? Revision { get; set; }
    public RegulatoryDate? SubmissionDate { get; set; }
    public RegulatoryDate? ActualDate { get; set; }
}

/// <summary>Breaks the date checks.</summary>
[FormEntity]
[SuppressMessage("Minor Code Smell", "S1144", Justification = "Read by EF.")]
internal sealed class BrokenDatedEntity
{
    [FormIgnore("Database identity")]
    public int Id { get; set; }

    [FormIgnore("Revision ownership")]
    public int RevisionId { get; set; }

    [FormPage("broken-dated-task", "no-event", "No event", Order = 10)]
    [FormField("broken_dated_entity.no_event_date", "No event", FormControl.PrecisionDate)]
    public int? NoEventDateId { get; set; }

    [FormPage("broken-dated-task", "no-event", "No event", Order = 10)]
    [FormField("broken_dated_entity.wrong_value", "Wrong value", FormControl.Text, Order = 10)]
    [FormShowWhen("broken_dated_entity.no_event_date", YesNoUnknown.Yes)]
    public string? WrongValue { get; set; }

    [FormPage("broken-dated-task", "text-event", "Text event", Order = 20)]
    [FormField("broken_dated_entity.text_event", "Text event", FormControl.Text)]
    [FormDateEvent(DateEventType.UkLicence)]
    public string? TextEvent { get; set; }

    [FormPage("broken-dated-task", "wrong-date-control", "Wrong date control", Order = 40)]
    [FormField("broken_dated_entity.wrong_control_date", "Wrong control", FormControl.Text)]
    [FormDateEvent(DateEventType.IntlLicence)]
    public int? WrongControlDateId { get; set; }

    [FormPage("broken-dated-task", "same-event", "Same event", Order = 50)]
    [FormField("broken_dated_entity.same_event_date", "Same event", FormControl.PrecisionDate)]
    [FormDateEvent(DateEventType.IntlLicence)]
    public int? SameEventDateId { get; set; }

    [FormPage("broken-dated-task", "no-name", "No name", Order = 60)]
    [FormField("broken_dated_entity.no_name_date", "No name", FormControl.PrecisionDate)]
    [FormDateEvent(DateEventType.EamsSubmission)]
    public int? NoNameDateId { get; set; }

    [FormPage("broken-dated-task", "no-name-actual", "No name actual", Order = 70)]
    [FormField("broken_dated_entity.no_name_actual_date", "No name actual", FormControl.ActualDate)]
    [FormDateEvent(DateEventType.EamsOpinion)]
    public int? NoNameActualDateId { get; set; }

    public RecordRevision? Revision { get; set; }
    public RegulatoryDate? NoEventDate { get; set; }
    public RegulatoryDate? WrongControlDate { get; set; }
    public RegulatoryDate? SameEventDate { get; set; }
    public RegulatoryDate? NoNameDate { get; set; }
    public RegulatoryDate? NoNameActualDate { get; set; }
}

/// <summary>A question revealed by a reference data code.</summary>
[FormEntity]
[SuppressMessage("Minor Code Smell", "S1144", Justification = "Read by EF.")]
internal sealed class CodedEntity
{
    [FormIgnore("Database identity")]
    public int Id { get; set; }

    [FormIgnore("Revision ownership")]
    public int RevisionId { get; set; }

    [FormPage("coded-task", "procedure", "Procedure", Order = 10)]
    [FormField("coded_entity.procedure_id", "Procedure", FormControl.Radio, Order = 10)]
    [FormOptions(ScanningTestOptions.ProcedureTypes)]
    public int? ProcedureId { get; set; }

    [FormPage("coded-task", "procedure", "Procedure", Order = 10)]
    [FormField("coded_entity.irp_details", "Details", FormControl.Text, Order = 20)]
    [FormShowWhen("coded_entity.procedure_id", MhraProcedureTypeCodes.InternationalRecognition)]
    public string? IrpDetails { get; set; }

    public RecordRevision? Revision { get; set; }
    public MhraProcedureType? Procedure { get; set; }
}

/// <summary>A condition on reference data that has no code.</summary>
[FormEntity]
[SuppressMessage("Minor Code Smell", "S1144", Justification = "Read by EF.")]
internal sealed class BrokenCodedEntity
{
    [FormIgnore("Database identity")]
    public int Id { get; set; }

    [FormIgnore("Revision ownership")]
    public int RevisionId { get; set; }

    [FormPage("broken-coded-task", "route", "Route", Order = 10)]
    [FormField("broken_coded_entity.route_id", "Route", FormControl.Radio, Order = 10)]
    public int? RouteId { get; set; }

    [FormPage("broken-coded-task", "route", "Route", Order = 10)]
    [FormField("broken_coded_entity.uncoded", "Uncoded", FormControl.Text, Order = 20)]
    [FormShowWhen("broken_coded_entity.route_id", "route-a")]
    public string? Uncoded { get; set; }

    [FormPage("broken-coded-task", "route", "Route", Order = 10)]
    [FormField("broken_coded_entity.procedure_id", "Procedure", FormControl.Radio, Order = 30)]
    public int? ProcedureId { get; set; }

    [FormPage("broken-coded-task", "route", "Route", Order = 10)]
    [FormField("broken_coded_entity.blank_code", "Blank code", FormControl.Text, Order = 40)]
    [FormShowWhen("broken_coded_entity.procedure_id", " ")]
    public string? BlankCode { get; set; }

    public RecordRevision? Revision { get; set; }
    public IrpRoute? Route { get; set; }
    public MhraProcedureType? Procedure { get; set; }
}

/// <summary>Has pages in a Vaccine task and in a Medicine task.</summary>
[FormEntity]
internal sealed class SplitFormEntity
{
    [FormIgnore("Database identity")]
    public int Id { get; set; }

    [FormIgnore("Revision ownership")]
    public int RevisionId { get; set; }

    [FormPage("valid-task", "split-vaccine", "Split vaccine", Order = 40)]
    [FormField("split_form_entity.vaccine", "Vaccine", FormControl.Text)]
    public string? Vaccine { get; set; }

    [FormPage(
        MedicineFormLayout.IndicationDetails,
        "split-medicine",
        "Split medicine",
        Order = 990
    )]
    [FormField("split_form_entity.medicine", "Medicine", FormControl.Text)]
    public string? Medicine { get; set; }

    public RecordRevision? Revision { get; set; }
}

/// <summary>The sections and tasks of the test entities, all in the Vaccine form.</summary>
internal sealed class VaccineTestLayout()
    : FormLayout(
        RecordType.Vaccine,
        [
            new FormLayoutSection(
                "valid",
                "Valid",
                10,
                [
                    new FormLayoutTask("valid-task", "Valid task", 10),
                    new FormLayoutTask("other-task", "Other task", 20),
                ]
            ),
            new FormLayoutSection(
                "broken",
                "Broken",
                20,
                [new FormLayoutTask("broken-task", "Broken task", 10)]
            ),
            new FormLayoutSection(
                "rows",
                "Rows",
                30,
                [new FormLayoutTask("rows-task", "Rows task", 10)]
            ),
            new FormLayoutSection(
                "conditional",
                "Conditional",
                40,
                [new FormLayoutTask("conditional-task", "Conditional task", 10)]
            ),
            new FormLayoutSection(
                "broken-conditions",
                "Broken conditions",
                50,
                [new FormLayoutTask("broken-conditions-task", "Broken conditions task", 10)]
            ),
            new FormLayoutSection(
                "dated",
                "Dated",
                60,
                [new FormLayoutTask("dated-task", "Dated task", 10)]
            ),
            new FormLayoutSection(
                "broken-dated",
                "Broken dated",
                70,
                [new FormLayoutTask("broken-dated-task", "Broken dated task", 10)]
            ),
            new FormLayoutSection(
                "coded",
                "Coded",
                80,
                [new FormLayoutTask("coded-task", "Coded task", 10)]
            ),
            new FormLayoutSection(
                "broken-coded",
                "Broken coded",
                90,
                [new FormLayoutTask("broken-coded-task", "Broken coded task", 10)]
            ),
        ]
    );

/// <summary>Breaks each layout check. Scanned on its own.</summary>
internal sealed class BrokenTestLayout()
    : FormLayout(
        RecordType.Medicine,
        [
            new FormLayoutSection(
                "Bad Section",
                " ",
                10,
                [
                    new FormLayoutTask("Bad Task", " ", 10),
                    new FormLayoutTask("tied-task", "Tied task", 10),
                ]
            ),
            new FormLayoutSection(
                "repeated",
                "Repeated",
                10,
                [new FormLayoutTask("repeated-task", "Repeated task", 10)]
            ),
            new FormLayoutSection(
                "repeated",
                "Repeated",
                20,
                [new FormLayoutTask("repeated-task", "Repeated task", 10)]
            ),
        ]
    );
