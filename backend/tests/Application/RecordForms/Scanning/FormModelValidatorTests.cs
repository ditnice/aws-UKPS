using Microsoft.EntityFrameworkCore.Metadata;
using Shouldly;
using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Application.RecordForms.Options;
using UKPS.Api.Application.RecordForms.Scanning;
using UKPS.Api.Persistence.Entities.ReferenceData;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Tests.Application.RecordForms.Scanning;

public class FormModelValidatorTests
{
    private static readonly IModel _model = ScanningTestDbContext.CreateModel();
    private static readonly OptionProviderRegistry _options = ScanningTestOptions.Create();

    [Fact]
    public void Scan_ValidEntity_BuildsBindingsOfEveryKind()
    {
        // The Vaccine form has no version yet, so only the version check fails.
        IReadOnlyList<string> problems = Problems(typeof(ValidEntity));
        problems.ShouldBe(["The Vaccine form has no entry in FormVersions."]);

        var (entities, scanProblems) = FormModelScanner.Scan(_model, [typeof(ValidEntity)]);
        scanProblems.ShouldBeEmpty();
        FormModel form = FormModelBuilder.Build(entities, _options)[RecordType.Vaccine];

        form.Pages.Select(static p => p.Id).ShouldBe(["text", "chapter", "areas"]);
        FormTask task = form.Sections.Single().Tasks.Single();
        (task.Id, task.Title, task.FirstPage.Id).ShouldBe(("valid-task", "Valid task", "text"));
        task.Pages.ShouldBe(form.Pages);
        form.FindPage("text")!
            .Questions.Select(static q => (q.Id, q.Control))
            .ShouldBe([
                ("valid_entity.text", FormControl.Text),
                ("valid_entity.choice", FormControl.Radio),
            ]);
        form.FindPage("text")!
            .Questions.Select(static q => q.OptionKey)
            .ShouldBe([null, OptionKeys.YesNoUnknown]);
        form.FindPage("chapter")!.Questions.Single().OptionKey.ShouldBe(OptionKeys.BnfChapters);
        form.FindPage("areas")!.Questions.Single().OptionKey.ShouldBe(OptionKeys.TherapeuticAreas);
        form.FindPage("areas")!.Questions.Single().Shape.ShouldBe(AnswerShape.Multi);
        form.FindPage("areas")!.Questions.Single().Entity.Includes.ShouldBe(["Areas"]);
    }

    [Theory]
    [InlineData("BrokenEntity.Id is a mapped column with neither [FormField] nor [FormIgnore].")]
    [InlineData("BrokenEntity.BlankReason has [FormIgnore] with a blank reason.")]
    [InlineData("BrokenEntity.NoPage has [FormField] but no [FormPage].")]
    [InlineData("BrokenEntity.BadIds has a badly formed question ID 'Broken.Bad ID'.")]
    [InlineData(
        "BrokenEntity.BadIds declares page 'Bad Page' with a badly formed ID or a blank title."
    )]
    [InlineData("BrokenEntity.WrongControl uses control Radio which does not suit a Text binding.")]
    [InlineData(
        "BrokenEntity.ChoiceWithoutProvider uses choice control Radio but no Enum provider of UnprovidedChoice is registered; register one or add [FormOptions]."
    )]
    [InlineData(
        "BrokenEntity.WrongEnum needs a Enum provider of YesNoUnknown, but 'paediatric-status' is a Enum provider of IndicationPaediatricStatus."
    )]
    [InlineData(
        "BrokenEntity.WrongEnum has [FormMaxLength], which needs a Text or Textarea control and a positive maximum."
    )]
    [InlineData("BrokenEntity.UnknownKey uses unknown option provider 'no-such-provider'.")]
    [InlineData("BrokenEntity.NoTask has [FormField] but no [FormTask].")]
    [InlineData(
        "BrokenEntity.BadTask declares task 'Bad Task' with a badly formed ID or a blank title."
    )]
    [InlineData(
        "BrokenEntity.ReservedPage declares page 'task-list', which is reserved for the task list."
    )]
    [InlineData(
        "BrokenEntity.PrefixesOnText has [FormValuePrefixes], which only a PrefixedNumber uses."
    )]
    [InlineData(
        "BrokenEntity.PrefixedWithoutPrefixes uses PrefixedNumber, which needs [FormValuePrefixes] with at least one prefix."
    )]
    [InlineData(
        "BrokenEntity.DigitEndingPrefix has a blank prefix, or one ending in a digit, which would make the number ambiguous."
    )]
    [InlineData("BrokenEntity.RepeatedPrefix repeats a prefix in [FormValuePrefixes].")]
    [InlineData(
        "BrokenEntity.OverlappingPrefix has a prefix that starts another, which makes the split ambiguous."
    )]
    public void Validate_BrokenEntity_ReportsTheProblem(string expected)
    {
        Problems(typeof(BrokenEntity)).ShouldContain(expected);
    }

    [Fact]
    public void Validate_ConditionalEntity_HasNoConditionProblems()
    {
        Problems(typeof(ConditionalEntity), typeof(ConditionalExtraEntity))
            .ShouldBe(["The Vaccine form has no entry in FormVersions."]);
    }

    [Theory]
    [InlineData(
        "BrokenConditionEntity.UnknownTrigger [FormShowWhen] names unknown question 'no_such.question'."
    )]
    [InlineData(
        "BrokenConditionEntity.BeforeTrigger [FormShowWhen] names 'broken_condition_entity.after', which must be an earlier question on the same page."
    )]
    [InlineData(
        "BrokenConditionEntity.OtherPage [FormShowWhen] names 'broken_condition_entity.trigger', which must be an earlier question on the same page."
    )]
    [InlineData(
        "BrokenConditionEntity.TextTrigger [FormShowWhen] names 'broken_condition_entity.before_trigger', which must be a Radio, Select or Checkbox on an enum or on reference data with a string Code, or a PrecisionDate."
    )]
    [InlineData(
        "BrokenConditionEntity.WrongValue [FormShowWhen] has value 'Unknown', which is not a YesNoUnknown member."
    )]
    [InlineData("BrokenConditionEntity.NoValues [FormShowWhen] needs at least one value.")]
    [InlineData("BrokenConditionEntity.Repeated [FormShowWhen] repeats a value.")]
    [InlineData(
        "BrokenConditionEntity.NonNullable [FormShowWhen] is on a non-nullable column, which cannot be cleared when the question does not apply."
    )]
    [InlineData(
        "BrokenConditionEntity [FormPageShowWhen] for page 'first' names 'broken_condition_entity.later', which must be on an earlier page."
    )]
    [InlineData(
        "BrokenConditionEntity [FormPageShowWhen] for page 'undeclared' names a page that BrokenConditionEntity does not declare."
    )]
    [InlineData(
        "BrokenConditionEntity [FormPageShowWhen] for page 'first' is on the first page of task 'broken-task', which must always apply."
    )]
    [InlineData("The Vaccine form has more than one [FormPageShowWhen] for page 'second'.")]
    public void Validate_BrokenConditions_ReportsTheProblem(string expected)
    {
        Problems(typeof(BrokenConditionEntity)).ShouldContain(expected);
    }

    [Fact]
    public void Scan_ConditionalEntity_BuildsPageAndQuestionConditions()
    {
        var (entities, _) = FormModelScanner.Scan(_model, [typeof(ConditionalEntity)]);
        FormModel form = FormModelBuilder.Build(entities, _options)[RecordType.Vaccine];

        form.FindPage("gate")!.ShowWhen.ShouldBeNull();
        form.FindPage("follow-up")!
            .ShowWhen!.Description.ShouldBe("AnswerIn conditional_entity.gate [Yes, Unknown]");
        form.FindQuestion("conditional_entity.reason")!
            .ShowWhen!.Description.ShouldBe("AnswerIn conditional_entity.gate [No]");
        form.FindQuestion("conditional_entity.gate")!.ShowWhen.ShouldBeNull();
    }

    [Fact]
    public void Scan_DatedEntity_BuildsDateBindingsAndPrecisionConditions()
    {
        Problems(typeof(DatedEntity)).ShouldBe(["The Vaccine form has no entry in FormVersions."]);

        var (entities, _) = FormModelScanner.Scan(_model, [typeof(DatedEntity)]);
        FormModel form = FormModelBuilder.Build(entities, _options)[RecordType.Vaccine];

        FormQuestion submission = form.FindQuestion("dated_entity.submission_date")!;
        submission.Binding.Description.ShouldBe(
            "regulatory-date:DatedEntity.SubmissionDateId:UkSubmission"
        );
        submission.OptionKey.ShouldBeNull();
        submission.Entity.Includes.ShouldBe(["SubmissionDate", "ActualDate"], ignoreOrder: true);
        form.FindPage("actual")!
            .ShowWhen!.Description.ShouldBe("AnswerIn dated_entity.submission_date [ActualDate]");
    }

    [Theory]
    [InlineData(
        "BrokenDatedEntity.NoEventDateId uses control PrecisionDate, which needs [FormDateEvent]."
    )]
    [InlineData(
        "BrokenDatedEntity.TextEvent has [FormDateEvent], which only a PrecisionDate, ActualMonth or ActualDate uses."
    )]
    [InlineData(
        "BrokenDatedEntity.WrongControlDateId uses control Text which does not suit a RegulatoryDate binding."
    )]
    [InlineData(
        "BrokenDatedEntity.WrongValue [FormShowWhen] has value 'Yes', which is not a DatePrecision member."
    )]
    [InlineData(
        "The Vaccine form uses date event IntlLicence on more than one question: 'broken_dated_entity.wrong_control_date', 'broken_dated_entity.same_event_date'."
    )]
    public void Validate_BrokenDatedEntity_ReportsTheProblem(string expected)
    {
        Problems(typeof(BrokenDatedEntity)).ShouldContain(expected);
    }

    [Fact]
    public void Scan_CodedEntity_BuildsAConditionOnTheCode()
    {
        Problems(typeof(CodedEntity)).ShouldBe(["The Vaccine form has no entry in FormVersions."]);

        var (entities, _) = FormModelScanner.Scan(_model, [typeof(CodedEntity)]);
        FormModel form = FormModelBuilder.Build(entities, _options)[RecordType.Vaccine];

        form.FindQuestion("coded_entity.irp_details")!
            .ShowWhen!.Description.ShouldBe(
                "AnswerIn coded_entity.procedure_id [international-recognition]"
            );
        form.FindQuestion("coded_entity.procedure_id")!
            .ReferenceType.ShouldBe(typeof(MhraProcedureType));
        form.ReferenceTriggerTypes.ShouldBe([typeof(MhraProcedureType)]);
    }

    [Theory]
    [InlineData(
        "BrokenCodedEntity.Uncoded [FormShowWhen] names 'broken_coded_entity.route_id', which must be a Radio, Select or Checkbox on an enum or on reference data with a string Code, or a PrecisionDate."
    )]
    [InlineData(
        "BrokenCodedEntity.BlankCode [FormShowWhen] has value ' ', which is not a MhraProcedureType code."
    )]
    public void Validate_BrokenCodedEntity_ReportsTheProblem(string expected)
    {
        Problems(typeof(BrokenCodedEntity)).ShouldContain(expected);
    }

    [Fact]
    public void Validate_SeveralProvidersOfOneType_AsksForFormOptions()
    {
        OptionProviderRegistry options = OptionProviderRegistry.CreateDefault();
        options.AddEnum("yes-no", (YesNoUnknown.Yes, "Yes"), (YesNoUnknown.No, "No"));

        var (entities, _) = FormModelScanner.Scan(_model, [typeof(ValidEntity)]);

        FormModelValidator
            .Validate(entities, options)
            .ShouldContain(
                "ValidEntity.Choice has several Enum providers of YesNoUnknown ('yes-no', 'yes-no-unknown'); choose one with [FormOptions]."
            );
    }

    [Fact]
    public void Validate_RowChecks_ReportMissingRevisionIdAndRequiredColumns()
    {
        IReadOnlyList<string> problems = Problems(typeof(RowsEntity));

        problems.ShouldContain("RowsEntity has no int RevisionId foreign key to RecordRevision.");
        problems.ShouldContain(
            "RowsEntity uses RowSource FirstSave but has non-nullable columns (RequiredName); use RowSource RecordCreation."
        );
    }

    [Fact]
    public void Validate_Disagreements_AreReported()
    {
        IReadOnlyList<string> problems = Problems(typeof(ValidEntity), typeof(DisagreeingEntity));

        problems.ShouldContain(
            "The Vaccine form declares section 'valid' with different titles or orders."
        );
        problems.ShouldContain(
            "The Vaccine form declares page 'text' with different titles, orders or commercial in confidence flags."
        );
        problems.ShouldContain(
            "The Vaccine form uses question ID 'valid_entity.text' more than once."
        );
    }

    [Fact]
    public void Validate_TaskConflicts_AreReported()
    {
        IReadOnlyList<string> problems = Problems(typeof(ValidEntity), typeof(TaskConflictEntity));

        problems.ShouldContain(
            "The Vaccine form declares task 'valid-task' with different titles, orders or sections."
        );
        problems.ShouldContain("The Vaccine form has page 'text' in more than one task.");
        problems.ShouldContain(
            "The Vaccine form, section 'valid', has task 'valid-task' and task 'other-task' with the same Order 10."
        );
    }

    [Fact]
    public void Validate_PageFlagDisagreement_IsReported()
    {
        IReadOnlyList<string> problems = Problems(
            typeof(ValidEntity),
            typeof(FlagDisagreeingEntity)
        );

        problems.ShouldContain(
            "The Vaccine form declares page 'text' with different titles, orders or commercial in confidence flags."
        );
    }

    [Fact]
    public void Create_InvalidModel_ThrowsWithEveryProblem()
    {
        FormModelException exception = Should.Throw<FormModelException>(() =>
            FormModelFactory.Create(_model, _options, [typeof(BrokenEntity), typeof(RowsEntity)])
        );

        exception.Problems.Count.ShouldBeGreaterThan(10);
        exception.Message.ShouldContain("RowsEntity has no int RevisionId");
    }

    private static IReadOnlyList<string> Problems(params Type[] types)
    {
        var (entities, problems) = FormModelScanner.Scan(_model, types);
        return [.. problems, .. FormModelValidator.Validate(entities, _options)];
    }
}
