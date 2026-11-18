using System.Diagnostics.CodeAnalysis;
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

    [FormPage("text")]
    [FormField("valid_entity.text", "Text", FormControl.Text, Order = 10)]
    [FormMaxLength(10, "Too long")]
    public string? Text { get; set; }

    [FormPage("text")]
    [FormField("valid_entity.choice", "Choice", FormControl.Radio, Order = 20)]
    [FormOptions(OptionKeys.YesNoUnknown)]
    public YesNoUnknown? Choice { get; set; }

    [FormPage("chapter")]
    [FormField(
        "valid_entity.chapter_id",
        "Chapter",
        FormControl.HierarchicalSelect,
        ChildLabel = "Sub"
    )]
    public int? ChapterId { get; set; }

    public RecordRevision? Revision { get; set; }
    public BnfChapter? Chapter { get; set; }

    [FormPage("areas")]
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

    [FormPage("bad-ids")]
    [FormField("Broken.Bad ID", "Bad ID", FormControl.Text)]
    public string? BadIds { get; set; }

    [FormPage("broken")]
    [FormField("broken_entity.wrong_control", "Wrong control", FormControl.Radio)]
    public string? WrongControl { get; set; }

    [FormPage("broken")]
    [FormField("broken_entity.choice_without_provider", "Choice", FormControl.Radio, Order = 1)]
    public UnprovidedChoice? ChoiceWithoutProvider { get; set; }

    [FormPage("broken")]
    [FormField("broken_entity.wrong_enum", "Wrong enum", FormControl.Radio, Order = 2)]
    [FormOptions(OptionKeys.PaediatricStatus)]
    [FormMaxLength(5, "Not for choices")]
    public YesNoUnknown? WrongEnum { get; set; }

    [FormPage("broken")]
    [FormField("broken_entity.unknown_key", "Unknown key", FormControl.Radio, Order = 3)]
    [FormOptions("no-such-provider")]
    public YesNoUnknown? UnknownKey { get; set; }

    [FormPage("unknown-page")]
    [FormField("broken_entity.unknown_page", "Unknown page", FormControl.Text)]
    public string? UnknownPage { get; set; }

    [FormPage("broken-prefixes")]
    [FormField("broken_entity.prefixes_on_text", "Prefixes on text", FormControl.Text, Order = 1)]
    [FormValuePrefixes("GID-TA")]
    public string? PrefixesOnText { get; set; }

    [FormPage("broken-prefixes")]
    [FormField(
        "broken_entity.prefixed_without_prefixes",
        "Prefixed without prefixes",
        FormControl.PrefixedNumber,
        Order = 2
    )]
    public string? PrefixedWithoutPrefixes { get; set; }

    [FormPage("broken-prefixes")]
    [FormField(
        "broken_entity.digit_ending_prefix",
        "Digit ending prefix",
        FormControl.PrefixedNumber,
        Order = 3
    )]
    [FormValuePrefixes("GID-TA1")]
    public string? DigitEndingPrefix { get; set; }

    [FormPage("broken-prefixes")]
    [FormField(
        "broken_entity.repeated_prefix",
        "Repeated prefix",
        FormControl.PrefixedNumber,
        Order = 4
    )]
    [FormValuePrefixes("GID-TA", "GID-TA")]
    public string? RepeatedPrefix { get; set; }

    [FormPage("broken-prefixes")]
    [FormField(
        "broken_entity.overlapping_prefix",
        "Overlapping prefix",
        FormControl.PrefixedNumber,
        Order = 5
    )]
    [FormValuePrefixes("GID", "GID-TA")]
    public string? OverlappingPrefix { get; set; }

    [FormPage("broken-prefixes")]
    [FormField(
        "broken_entity.negative_digits",
        "Negative digits",
        FormControl.PrefixedNumber,
        Order = 6
    )]
    [FormValuePrefixes("NCT", Digits = -1)]
    public string? NegativeDigits { get; set; }

    public RecordRevision? Revision { get; set; }
}

/// <summary>A FirstSave entity with a required column, and no RevisionId.</summary>
[FormEntity]
internal sealed class RowsEntity
{
    [FormIgnore("Database identity")]
    public int Id { get; set; }

    [FormPage("rows")]
    [FormField("rows_entity.required_name", "Name", FormControl.Text)]
    [FormRequired("Enter a name")]
    public required string RequiredName { get; set; }
}

/// <summary>Shares a page with <see cref="ValidEntity"/> and reuses one of its question IDs.</summary>
[FormEntity]
internal sealed class DisagreeingEntity
{
    [FormIgnore("Database identity")]
    public int Id { get; set; }

    [FormIgnore("Revision ownership")]
    public int RevisionId { get; set; }

    [FormPage("text")]
    [FormField("valid_entity.text", "Duplicate ID", FormControl.Text, Order = 30)]
    public string? Text { get; set; }

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

    [FormPage("gate")]
    [FormField("conditional_entity.gate", "Gate", FormControl.Radio, Order = 10)]
    [FormOptions(OptionKeys.YesNoUnknown)]
    public YesNoUnknown? Gate { get; set; }

    [FormPage("gate")]
    [FormField("conditional_entity.reason", "Reason", FormControl.Text, Order = 20)]
    [FormShowWhen("conditional_entity.gate", YesNoUnknown.No)]
    public string? Reason { get; set; }

    [FormPage("follow-up")]
    [FormField("conditional_entity.detail", "Detail", FormControl.Radio, Order = 10)]
    [FormOptions(OptionKeys.YesNoUnknown)]
    public YesNoUnknown? Detail { get; set; }

    [FormPage("follow-up")]
    [FormField("conditional_entity.notes", "Notes", FormControl.Text, Order = 20)]
    [FormShowWhen("conditional_entity.detail", YesNoUnknown.Yes)]
    public string? Notes { get; set; }

    [FormPage("end")]
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

    [FormPage("first")]
    [FormField("broken_condition_entity.trigger", "Trigger", FormControl.Radio, Order = 10)]
    [FormOptions(OptionKeys.YesNoUnknown)]
    public YesNoUnknown? Trigger { get; set; }

    [FormPage("first")]
    [FormField("broken_condition_entity.unknown_trigger", "Unknown", FormControl.Text, Order = 20)]
    [FormShowWhen("no_such.question", YesNoUnknown.Yes)]
    public string? UnknownTrigger { get; set; }

    [FormPage("first")]
    [FormField("broken_condition_entity.before_trigger", "Before", FormControl.Text, Order = 30)]
    [FormShowWhen("broken_condition_entity.after", YesNoUnknown.Yes)]
    public string? BeforeTrigger { get; set; }

    [FormPage("first")]
    [FormField("broken_condition_entity.after", "After", FormControl.Radio, Order = 40)]
    [FormOptions(OptionKeys.YesNoUnknown)]
    public YesNoUnknown? After { get; set; }

    [FormPage("first")]
    [FormField("broken_condition_entity.text_trigger", "Text", FormControl.Text, Order = 50)]
    [FormShowWhen("broken_condition_entity.before_trigger", YesNoUnknown.Yes)]
    public string? TextTrigger { get; set; }

    [FormPage("first")]
    [FormField("broken_condition_entity.wrong_value", "Wrong", FormControl.Text, Order = 60)]
    [FormShowWhen("broken_condition_entity.trigger", IndicationPaediatricStatus.Unknown)]
    public string? WrongValue { get; set; }

    [FormPage("first")]
    [FormField("broken_condition_entity.no_values", "None", FormControl.Text, Order = 70)]
    [FormShowWhen("broken_condition_entity.trigger")]
    public string? NoValues { get; set; }

    [FormPage("first")]
    [FormField("broken_condition_entity.repeated", "Repeated", FormControl.Text, Order = 80)]
    [FormShowWhen("broken_condition_entity.trigger", YesNoUnknown.Yes, YesNoUnknown.Yes)]
    public string? Repeated { get; set; }

    [FormPage("first")]
    [FormField("broken_condition_entity.non_nullable", "Name", FormControl.Text, Order = 90)]
    [FormShowWhen("broken_condition_entity.trigger", YesNoUnknown.Yes)]
    [FormRequired("Enter a name")]
    public required string NonNullable { get; set; }

    [FormPage("second")]
    [FormField("broken_condition_entity.later", "Later", FormControl.Radio, Order = 10)]
    [FormOptions(OptionKeys.YesNoUnknown)]
    public YesNoUnknown? Later { get; set; }

    [FormPage("second")]
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

    [FormPage("extra")]
    [FormField("conditional_extra_entity.extra", "Extra", FormControl.Text)]
    public string? Extra { get; set; }

    [FormPage("other")]
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

    [FormPage("submission")]
    [FormField("dated_entity.submission_date", "Submission", FormControl.PrecisionDate)]
    [FormDateEvent(DateEventType.UkSubmission, Name = "submission")]
    public int? SubmissionDateId { get; set; }

    [FormPage("actual")]
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

    [FormPage("no-event")]
    [FormField("broken_dated_entity.no_event_date", "No event", FormControl.PrecisionDate)]
    public int? NoEventDateId { get; set; }

    [FormPage("no-event")]
    [FormField("broken_dated_entity.wrong_value", "Wrong value", FormControl.Text, Order = 10)]
    [FormShowWhen("broken_dated_entity.no_event_date", YesNoUnknown.Yes)]
    public string? WrongValue { get; set; }

    [FormPage("text-event")]
    [FormField("broken_dated_entity.text_event", "Text event", FormControl.Text)]
    [FormDateEvent(DateEventType.UkLicence)]
    public string? TextEvent { get; set; }

    [FormPage("wrong-date-control")]
    [FormField("broken_dated_entity.wrong_control_date", "Wrong control", FormControl.Text)]
    [FormDateEvent(DateEventType.IntlLicence)]
    public int? WrongControlDateId { get; set; }

    [FormPage("same-event")]
    [FormField("broken_dated_entity.same_event_date", "Same event", FormControl.PrecisionDate)]
    [FormDateEvent(DateEventType.IntlLicence)]
    public int? SameEventDateId { get; set; }

    [FormPage("no-name")]
    [FormField("broken_dated_entity.no_name_date", "No name", FormControl.PrecisionDate)]
    [FormDateEvent(DateEventType.EamsSubmission)]
    public int? NoNameDateId { get; set; }

    [FormPage("no-name-actual")]
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

    [FormPage("procedure")]
    [FormField("coded_entity.procedure_id", "Procedure", FormControl.Radio, Order = 10)]
    [FormOptions(ScanningTestOptions.ProcedureTypes)]
    public int? ProcedureId { get; set; }

    [FormPage("procedure")]
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

    [FormPage("route")]
    [FormField("broken_coded_entity.route_id", "Route", FormControl.Radio, Order = 10)]
    public int? RouteId { get; set; }

    [FormPage("route")]
    [FormField("broken_coded_entity.uncoded", "Uncoded", FormControl.Text, Order = 20)]
    [FormShowWhen("broken_coded_entity.route_id", "route-a")]
    public string? Uncoded { get; set; }

    [FormPage("route")]
    [FormField("broken_coded_entity.procedure_id", "Procedure", FormControl.Radio, Order = 30)]
    public int? ProcedureId { get; set; }

    [FormPage("route")]
    [FormField("broken_coded_entity.blank_code", "Blank code", FormControl.Text, Order = 40)]
    [FormShowWhen("broken_coded_entity.procedure_id", " ")]
    public string? BlankCode { get; set; }

    public RecordRevision? Revision { get; set; }
    public IrpRoute? Route { get; set; }
    public MhraProcedureType? Procedure { get; set; }
}

/// <summary>A YesNoUnknown question that does not choose between the providers of its type.</summary>
[FormEntity]
internal sealed class AmbiguousChoiceEntity
{
    [FormIgnore("Database identity")]
    public int Id { get; set; }

    [FormIgnore("Revision ownership")]
    public int RevisionId { get; set; }

    [FormPage("ambiguous")]
    [FormField("ambiguous_choice_entity.choice", "Choice", FormControl.Radio)]
    public YesNoUnknown? Choice { get; set; }

    public RecordRevision? Revision { get; set; }
}

/// <summary>Has a page in the Vaccine form and a page in the Medicine form.</summary>
[FormEntity]
internal sealed class SplitFormEntity
{
    [FormIgnore("Database identity")]
    public int Id { get; set; }

    [FormIgnore("Revision ownership")]
    public int RevisionId { get; set; }

    [FormPage("split-vaccine")]
    [FormField("split_form_entity.vaccine", "Vaccine", FormControl.Text)]
    public string? Vaccine { get; set; }

    [FormPage("split-medicine")]
    [FormField("split_form_entity.medicine", "Medicine", FormControl.Text)]
    public string? Medicine { get; set; }

    public RecordRevision? Revision { get; set; }
}

/// <summary>The sections, tasks and pages of the test entities, all in the Vaccine form.</summary>
internal sealed class VaccineTestLayout()
    : FormLayout(
        RecordType.Vaccine,
        [
            new FormLayoutSection(
                "valid",
                "Valid",
                10,
                [
                    new FormLayoutTask(
                        "valid-task",
                        "Valid task",
                        10,
                        [
                            new FormLayoutPage("text", "Text", 10),
                            new FormLayoutPage("chapter", "Chapter", 20),
                            new FormLayoutPage("areas", "Areas", 30),
                            new FormLayoutPage("split-vaccine", "Split vaccine", 40),
                            new FormLayoutPage("unplaced-gate", "Gate", 50),
                        ]
                    ),
                ]
            ),
            new FormLayoutSection(
                "broken",
                "Broken",
                20,
                [
                    new FormLayoutTask(
                        "broken-task",
                        "Broken task",
                        10,
                        [
                            new FormLayoutPage("broken", "Broken", 10),
                            new FormLayoutPage("bad-ids", "Bad IDs", 20),
                            new FormLayoutPage("broken-prefixes", "Broken prefixes", 50),
                        ]
                    ),
                ]
            ),
            new FormLayoutSection(
                "rows",
                "Rows",
                30,
                [
                    new FormLayoutTask(
                        "rows-task",
                        "Rows task",
                        10,
                        [new FormLayoutPage("rows", "Rows", 10)]
                    ),
                ]
            ),
            new FormLayoutSection(
                "conditional",
                "Conditional",
                40,
                [
                    new FormLayoutTask(
                        "conditional-task",
                        "Conditional task",
                        10,
                        [
                            new FormLayoutPage("gate", "Gate", 10),
                            new FormLayoutPage("follow-up", "Follow up", 20),
                            new FormLayoutPage("end", "End", 30),
                            new FormLayoutPage("extra", "Extra", 40),
                            new FormLayoutPage("other", "Other", 50),
                        ]
                    ),
                ]
            ),
            new FormLayoutSection(
                "broken-conditions",
                "Broken conditions",
                50,
                [
                    new FormLayoutTask(
                        "broken-conditions-task",
                        "Broken conditions task",
                        10,
                        [
                            new FormLayoutPage("first", "First", 10),
                            new FormLayoutPage("second", "Second", 20),
                        ]
                    ),
                ]
            ),
            new FormLayoutSection(
                "dated",
                "Dated",
                60,
                [
                    new FormLayoutTask(
                        "dated-task",
                        "Dated task",
                        10,
                        [
                            new FormLayoutPage("submission", "Submission", 10),
                            new FormLayoutPage("actual", "Actual", 20),
                        ]
                    ),
                ]
            ),
            new FormLayoutSection(
                "broken-dated",
                "Broken dated",
                70,
                [
                    new FormLayoutTask(
                        "broken-dated-task",
                        "Broken dated task",
                        10,
                        [
                            new FormLayoutPage("no-event", "No event", 10),
                            new FormLayoutPage("text-event", "Text event", 20),
                            new FormLayoutPage("wrong-date-control", "Wrong date control", 40),
                            new FormLayoutPage("same-event", "Same event", 50),
                            new FormLayoutPage("no-name", "No name", 60),
                            new FormLayoutPage("no-name-actual", "No name actual", 70),
                        ]
                    ),
                ]
            ),
            new FormLayoutSection(
                "coded",
                "Coded",
                80,
                [
                    new FormLayoutTask(
                        "coded-task",
                        "Coded task",
                        10,
                        [new FormLayoutPage("procedure", "Procedure", 10)]
                    ),
                ]
            ),
            new FormLayoutSection(
                "broken-coded",
                "Broken coded",
                90,
                [
                    new FormLayoutTask(
                        "broken-coded-task",
                        "Broken coded task",
                        10,
                        [new FormLayoutPage("route", "Route", 10)]
                    ),
                ]
            ),
            new FormLayoutSection(
                "ambiguous",
                "Ambiguous",
                100,
                [
                    new FormLayoutTask(
                        "ambiguous-task",
                        "Ambiguous task",
                        10,
                        [new FormLayoutPage("ambiguous", "Ambiguous", 10)]
                    ),
                ]
            ),
            new FormLayoutSection(
                "grouped",
                "Grouped",
                110,
                [
                    new FormLayoutTask(
                        "group-task",
                        "Group task",
                        10,
                        [new FormLayoutPage("items", "Items", 10)]
                    ),
                ]
            ),
            new FormLayoutSection(
                "broken-group",
                "Broken group",
                120,
                [
                    new FormLayoutTask(
                        "broken-group-task",
                        "Broken group task",
                        10,
                        [new FormLayoutPage("broken-items", "Broken items", 10)]
                    ),
                ]
            ),
            new FormLayoutSection(
                "listed",
                "Listed",
                130,
                [
                    new FormLayoutTask(
                        "listed-task",
                        "Listed task",
                        10,
                        [new FormLayoutPage("listed", "Listed", 10)]
                    ),
                    new FormLayoutTask(
                        "broken-listed-task",
                        "Broken listed task",
                        20,
                        [new FormLayoutPage("broken-listed", "Broken listed", 10)]
                    ),
                ]
            ),
        ]
    );

/// <summary>
/// A Medicine layout holding <see cref="SplitFormEntity"/>'s Medicine page, scanned in place of
/// the real one.
/// </summary>
internal sealed class SplitMedicineTestLayout()
    : FormLayout(
        RecordType.Medicine,
        [
            new FormLayoutSection(
                "split",
                "Split",
                10,
                [
                    new FormLayoutTask(
                        "split-task",
                        "Split task",
                        10,
                        [new FormLayoutPage("split-medicine", "Split medicine", 10)]
                    ),
                ]
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
                    new FormLayoutTask(
                        "Bad Task",
                        " ",
                        10,
                        [
                            new FormLayoutPage("Bad Page", " ", 10),
                            new FormLayoutPage("task-list", "Task list", 20),
                        ]
                    ),
                    new FormLayoutTask(
                        "tied-task",
                        "Tied task",
                        10,
                        [
                            new FormLayoutPage("tied-first", "Tied first", 10),
                            new FormLayoutPage("tied-second", "Tied second", 10),
                        ]
                    ),
                ]
            ),
            new FormLayoutSection(
                "repeated",
                "Repeated",
                10,
                [
                    new FormLayoutTask(
                        "repeated-task",
                        "Repeated task",
                        10,
                        [new FormLayoutPage("repeated-page", "Repeated page", 10)]
                    ),
                ]
            ),
            new FormLayoutSection(
                "repeated",
                "Repeated",
                20,
                [
                    new FormLayoutTask(
                        "repeated-task",
                        "Repeated task",
                        10,
                        [new FormLayoutPage("repeated-page", "Repeated page", 10)]
                    ),
                ]
            ),
            new FormLayoutSection("empty", "Empty", 30, []),
            new FormLayoutSection(
                "no-pages",
                "No pages",
                40,
                [new FormLayoutTask("no-pages-task", "No pages task", 10, [])]
            ),
        ]
    );

/// <summary>Declares no sections. Scanned on its own.</summary>
internal sealed class EmptyTestLayout() : FormLayout(RecordType.Vaccine, []);

/// <summary>Has no parameterless constructor, so the scanner cannot create it.</summary>
internal sealed class NoDefaultConstructorTestLayout(RecordType recordType)
    : FormLayout(recordType, []);

/// <summary>Throws while being created.</summary>
internal sealed class ThrowingTestLayout() : FormLayout(RecordType.Vaccine, Fail())
{
    private static IReadOnlyList<FormLayoutSection> Fail() =>
        throw new InvalidOperationException("Broken layout.");
}

/// <summary>
/// Puts a [FormPageShowWhen] on a page no layout declares, so the condition's page is not placed
/// in any form.
/// </summary>
[FormEntity]
[FormPageShowWhen("unplaced-follow-up", "unplaced_condition_entity.gate", YesNoUnknown.Yes)]
[SuppressMessage("Minor Code Smell", "S1144", Justification = "Read by EF.")]
internal sealed class UnplacedConditionEntity
{
    [FormIgnore("Database identity")]
    public int Id { get; set; }

    [FormIgnore("Revision ownership")]
    public int RevisionId { get; set; }

    [FormPage("unplaced-gate")]
    [FormField("unplaced_condition_entity.gate", "Gate", FormControl.Radio)]
    [FormOptions(OptionKeys.YesNoUnknown)]
    public YesNoUnknown? Gate { get; set; }

    [FormPage("unplaced-follow-up")]
    [FormField("unplaced_condition_entity.detail", "Detail", FormControl.Text)]
    public string? Detail { get; set; }

    public RecordRevision? Revision { get; set; }
}

/// <summary>A valid group whose items hold text, a prefixed number and a text list.</summary>
[FormEntity]
[SuppressMessage("Minor Code Smell", "S1144", Justification = "Read by EF.")]
internal sealed class GroupEntity
{
    [FormIgnore("Database identity")]
    public int Id { get; set; }

    [FormIgnore("Revision ownership")]
    public int RevisionId { get; set; }

    public RecordRevision? Revision { get; set; }

    [FormPage("items")]
    [FormField(
        "group_entity_item",
        "Items",
        FormControl.Group,
        ItemLabel = "Item",
        AddAnotherLabel = "Add another item",
        RemoveLabel = "Remove item",
        ItemKey = "code"
    )]
    [FormRequired("Add at least one item")]
    [FormMaxItems(5, "Add up to 5 items")]
    public ICollection<GroupEntityItem> Items { get; set; } = [];
}

[SuppressMessage("Minor Code Smell", "S1144", Justification = "Read by EF.")]
internal sealed class GroupEntityItem
{
    [FormIgnore("Database identity")]
    public int Id { get; set; }

    [FormIgnore("Parent ownership")]
    public int GroupEntityId { get; set; }

    [FormField("name", "Name", FormControl.Text, Order = 10)]
    [FormRequired("Enter a name")]
    [FormMaxLength(10, "Name is too long")]
    public required string Name { get; set; }

    [FormField("code", "Code", FormControl.PrefixedNumber, Order = 20)]
    [FormValuePrefixes("NCT", Digits = 8)]
    [FormRequired("Enter a code")]
    [FormDistinct("Code already entered")]
    public string? Code { get; set; }

    [FormField(
        "numbers",
        "Numbers",
        FormControl.TextList,
        AddAnotherLabel = "Add another number",
        ItemLabel = "Number",
        Order = 30
    )]
    [FormDistinct("Number already entered")]
    public ICollection<GroupEntityItemNumber> Numbers { get; set; } = [];

    public GroupEntity? GroupEntity { get; set; }
}

internal sealed class GroupEntityItemNumber
{
    public int Id { get; set; }
    public int GroupEntityItemId { get; set; }
    public required string Number { get; set; }
    public int? DisplayOrder { get; set; }
    public GroupEntityItem? Item { get; set; }
}

/// <summary>Breaks the group and text list checks.</summary>
[FormEntity]
[SuppressMessage("Minor Code Smell", "S1144", Justification = "Read by EF.")]
internal sealed class BrokenGroupEntity
{
    [FormIgnore("Database identity")]
    public int Id { get; set; }

    [FormIgnore("Revision ownership")]
    public int RevisionId { get; set; }

    public RecordRevision? Revision { get; set; }

    [FormPage("broken-items")]
    [FormField("broken_group_item", "Items", FormControl.Group, ItemKey = "free_text")]
    public ICollection<BrokenGroupItem> Items { get; set; } = [];

    [FormPage("broken-items")]
    [FormField(
        "broken_group_entity.text",
        "Text",
        FormControl.Text,
        Order = 10,
        ItemLabel = "Not a group",
        RemoveLabel = "Not a list"
    )]
    [FormDistinct("Not a list")]
    public string? Text { get; set; }
}

[SuppressMessage("Minor Code Smell", "S1144", Justification = "Read by EF.")]
internal sealed class BrokenGroupItem
{
    [FormIgnore("Database identity")]
    public int Id { get; set; }

    [FormIgnore("Parent ownership")]
    public int BrokenGroupEntityId { get; set; }

    [FormPage("broken-items")]
    [FormField("broken.dotted", "Dotted", FormControl.Text, Order = 10)]
    public string? Dotted { get; set; }

    [FormField("free_text", "Free text", FormControl.Text, Order = 10)]
    public string? FreeText { get; set; }

    [FormField("choice", "Choice", FormControl.Radio, Order = 20)]
    [FormOptions(OptionKeys.YesNoUnknown)]
    public YesNoUnknown? Choice { get; set; }

    [FormField("required_name", "Required name", FormControl.Text, Order = 30)]
    public required string RequiredName { get; set; }

    [FormField("blank_labels", "Blank labels", FormControl.TextList, ItemLabel = " ", Order = 40)]
    public ICollection<BrokenGroupItemLabel> BlankLabels { get; set; } = [];

    public BrokenGroupEntity? BrokenGroupEntity { get; set; }
}

internal sealed class BrokenGroupItemLabel
{
    public int Id { get; set; }
    public int BrokenGroupItemId { get; set; }
    public required string Label { get; set; }
    public int? DisplayOrder { get; set; }
    public BrokenGroupItem? BrokenGroupItem { get; set; }
}

internal enum ListedNameKind
{
    First = 0,
    Second = 1,
}

/// <summary>Two text lists sharing one collection of rows, told apart by their kind.</summary>
[FormEntity]
[SuppressMessage("Minor Code Smell", "S1144", Justification = "Read by EF.")]
internal sealed class ListedEntity
{
    [FormIgnore("Database identity")]
    public int Id { get; set; }

    [FormIgnore("Revision ownership")]
    public int RevisionId { get; set; }

    public RecordRevision? Revision { get; set; }

    public ICollection<ListedEntityName> Names { get; set; } = [];

    [FormPage("listed")]
    [FormField(
        "listed_entity.first_names",
        "First names",
        FormControl.TextList,
        Order = 10,
        AddAnotherLabel = "Add another first name",
        RemoveLabel = "Remove first name"
    )]
    [FormRequired("Enter a first name")]
    [FormMaxItems(3, "Enter up to 3 first names")]
    [FormDistinct("First name has already been entered")]
    [FormListSource(nameof(Names), ListedNameKind.First)]
    public IEnumerable<string> FirstNames => Names.Select(static n => n.Name);

    [FormPage("listed")]
    [FormField("listed_entity.second_names", "Second names", FormControl.TextList, Order = 20)]
    [FormListSource(nameof(Names), ListedNameKind.Second)]
    public IEnumerable<string> SecondNames => Names.Select(static n => n.Name);
}

[SuppressMessage("Minor Code Smell", "S1144", Justification = "Read by EF.")]
internal sealed class ListedEntityName
{
    public int Id { get; set; }
    public int ListedEntityId { get; set; }
    public required string Name { get; set; }
    public ListedNameKind Kind { get; set; }
    public int? DisplayOrder { get; set; }
    public ListedEntity? ListedEntity { get; set; }
}

/// <summary>Breaks each text list and list source check.</summary>
[FormEntity]
[SuppressMessage("Minor Code Smell", "S1144", Justification = "Read by EF.")]
internal sealed class BrokenListedEntity
{
    [FormIgnore("Database identity")]
    public int Id { get; set; }

    [FormIgnore("Revision ownership")]
    public int RevisionId { get; set; }

    public RecordRevision? Revision { get; set; }

    public ICollection<BrokenListedEntityName> Names { get; set; } = [];

    [FormPage("broken-listed")]
    [FormField("broken_listed_entity.mapped", "Mapped", FormControl.TextList, Order = 10)]
    [FormListSource(nameof(Names), ListedNameKind.First)]
    public string? Mapped { get; set; }

    [FormPage("broken-listed")]
    [FormField("broken_listed_entity.unknown", "Unknown", FormControl.TextList, Order = 20)]
    [FormListSource("NoSuchNavigation", ListedNameKind.First)]
    public IEnumerable<string> Unknown => Names.Select(static n => n.Name);

    [FormPage("broken-listed")]
    [FormField("broken_listed_entity.not_a_list", "Not a list", FormControl.Text, Order = 30)]
    [FormListSource(nameof(Names), ListedNameKind.First)]
    public IEnumerable<string> NotAList => Names.Select(static n => n.Name);

    [FormPage("broken-listed")]
    [FormField("broken_listed_entity.wrong_kind", "Wrong kind", FormControl.TextList, Order = 40)]
    [FormListSource(nameof(Names), YesNoUnknown.Yes)]
    public IEnumerable<string> WrongKind => Names.Select(static n => n.Name);

    [FormPage("broken-listed")]
    [FormField(
        "broken_listed_entity.list_labels_on_text",
        "List labels on text",
        FormControl.Text,
        Order = 50,
        RemoveLabel = "Remove"
    )]
    [FormDistinct("Already entered")]
    public string? ListLabelsOnText { get; set; }
}

[SuppressMessage("Minor Code Smell", "S1144", Justification = "Read by EF.")]
internal sealed class BrokenListedEntityName
{
    public int Id { get; set; }
    public int BrokenListedEntityId { get; set; }
    public required string Name { get; set; }
    public ListedNameKind Kind { get; set; }
    public BrokenListedEntity? BrokenListedEntity { get; set; }
}
