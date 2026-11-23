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

        var (declarations, scanProblems) = FormModelScanner.Scan(
            _model,
            [typeof(VaccineTestLayout), typeof(ValidEntity)]
        );
        scanProblems.ShouldBeEmpty();
        FormModel form = FormModelBuilder.Build(declarations, _options)[RecordType.Vaccine];

        form.Pages.Select(static p => p.Id).ShouldBe(["text", "chapter", "areas"]);
        FormTask task = form.Sections.Single().Tasks.Single();
        (task.Id, task.Title, task.FirstPage.Id).ShouldBe(("valid-task", "Valid task", "text"));
        task.Hint.ShouldBe("Valid task hint");
        form.Sections.Single().SubText.ShouldBe("Valid sub text");
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
    [InlineData(
        "BrokenEntity.UnknownPage declares page 'unknown-page', which no FormLayout declares."
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
        "BrokenEntity.NegativeDigits has a negative Digits in [FormValuePrefixes]; use 0 for up to 5 digits."
    )]
    [InlineData(
        "BrokenEntity.OverlappingPrefix has a prefix that starts another, which makes the split ambiguous."
    )]
    [InlineData("BrokenEntity.WidthOnTextarea sets Width, which only a Text or a TextList uses.")]
    [InlineData("BrokenEntity.BlankNumberName has a blank Name in [FormValuePrefixes].")]
    public void Validate_BrokenEntity_ReportsTheProblem(string expected)
    {
        Problems(typeof(BrokenEntity)).ShouldContain(expected);
    }

    [Fact]
    public void Scan_GroupEntity_BuildsTheGroupAndItsQuestions()
    {
        Problems(typeof(GroupEntity)).ShouldBe(["The Vaccine form has no entry in FormVersions."]);

        var (declarations, _) = FormModelScanner.Scan(
            _model,
            [typeof(VaccineTestLayout), typeof(GroupEntity)]
        );
        FormModel form = FormModelBuilder.Build(declarations, _options)[RecordType.Vaccine];
        FormQuestion group = form.FindPage("items")!.Questions.Single();

        (group.Id, group.Control, group.Shape).ShouldBe(
            ("group_entity_item", FormControl.Group, AnswerShape.Group)
        );
        (group.ItemLabel, group.AddAnotherLabel, group.RemoveLabel, group.ItemKey).ShouldBe(
            ("Item", "Add another item", "Remove item", "code")
        );
        group.Rules.Select(static r => r.Kind).ShouldBe([RuleKind.Required, RuleKind.MaxItems]);
        group.Entity.Includes.ShouldBe(["Items", "Items.Numbers"]);
        group
            .Children.Select(static c => (c.Id, c.Control, c.Shape))
            .ShouldBe([
                ("name", FormControl.Text, AnswerShape.Single),
                ("code", FormControl.PrefixedNumber, AnswerShape.Single),
                ("numbers", FormControl.TextList, AnswerShape.List),
            ]);
        group.Children[1].PrefixDigits.ShouldBe(8);
        group
            .Children[1]
            .Rules.Select(static r => r.Kind)
            .ShouldBe([RuleKind.Required, RuleKind.Distinct]);
        group.Children[2].Rules.Single().Kind.ShouldBe(RuleKind.Distinct);
        group.Children[2].ItemLabel.ShouldBe("Number");
        form.FindQuestion("name").ShouldBeNull("a group's questions are not form questions");
    }

    [Theory]
    [InlineData("BrokenGroupEntity.Items uses Group, which needs an ItemLabel.")]
    [InlineData(
        "BrokenGroupEntity.Items has ItemKey 'free_text', which must name a single-value question of the group with [FormRequired] and [FormDistinct]."
    )]
    [InlineData("BrokenGroupEntity.Text sets ItemLabel, which only a Group or a TextList uses.")]
    [InlineData("BrokenGroupItem.BlankLabels sets a blank ItemLabel.")]
    [InlineData("BrokenGroupEntity.Text sets RemoveLabel, which only a Group or a TextList uses.")]
    [InlineData(
        "BrokenGroupEntity.Text has [FormDistinct], which only a TextList or a single-value question of a group uses."
    )]
    [InlineData(
        "BrokenGroupItem.Dotted is a question of group BrokenGroupEntity.Items, so it takes the group's page and must not declare [FormPage]."
    )]
    [InlineData(
        "BrokenGroupItem.Dotted is a question of group BrokenGroupEntity.Items, so its ID 'broken.dotted' must be lowercase words joined by underscores, with no dotted prefix."
    )]
    [InlineData(
        "BrokenGroupItem.Choice uses control Radio, which a group's question cannot use; use Text, Textarea, PrefixedNumber or TextList."
    )]
    [InlineData(
        "BrokenGroupItem.RequiredName is bound to a non-nullable column, so it needs [FormRequired]."
    )]
    [InlineData("BrokenGroupEntity.Items has more than one question with Order 10.")]
    public void Validate_BrokenGroup_ReportsTheProblem(string expected)
    {
        Problems(typeof(BrokenGroupEntity)).ShouldContain(expected);
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
        "BrokenConditionEntity [FormPageShowWhen] for page 'first' is on the first page of task 'broken-conditions-task', which must always apply unless every other page of the task has a condition too."
    )]
    [InlineData("The Vaccine form has more than one [FormPageShowWhen] for page 'second'.")]
    public void Validate_BrokenConditions_ReportsTheProblem(string expected)
    {
        Problems(typeof(BrokenConditionEntity)).ShouldContain(expected);
    }

    [Fact]
    public void Scan_ConditionalEntity_BuildsPageAndQuestionConditions()
    {
        var (declarations, _) = FormModelScanner.Scan(
            _model,
            [typeof(VaccineTestLayout), typeof(ConditionalEntity)]
        );
        FormModel form = FormModelBuilder.Build(declarations, _options)[RecordType.Vaccine];

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

        var (declarations, _) = FormModelScanner.Scan(
            _model,
            [typeof(VaccineTestLayout), typeof(DatedEntity)]
        );
        FormModel form = FormModelBuilder.Build(declarations, _options)[RecordType.Vaccine];

        FormQuestion submission = form.FindQuestion("dated_entity.submission_date")!;
        submission.Binding.Description.ShouldBe(
            "regulatory-date:DatedEntity.SubmissionDateId:UkSubmission"
        );
        submission.OptionKey.ShouldBeNull();
        submission.DateName.ShouldBe("submission");
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
        "BrokenDatedEntity.NoNameDateId uses control PrecisionDate, whose [FormDateEvent] needs a Name for its error messages."
    )]
    [InlineData(
        "BrokenDatedEntity.NoNameActualDateId uses control ActualDate, whose [FormDateEvent] needs a Name for its error messages."
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

        var (declarations, _) = FormModelScanner.Scan(
            _model,
            [typeof(VaccineTestLayout), typeof(CodedEntity)]
        );
        FormModel form = FormModelBuilder.Build(declarations, _options)[RecordType.Vaccine];

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
        Problems(typeof(AmbiguousChoiceEntity))
            .ShouldContain(
                "AmbiguousChoiceEntity.Choice has several Enum providers of YesNoUnknown ('clinical-trials-recruiting-in-uk', 'yes-no-unknown'); choose one with [FormOptions]."
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
    public void Validate_RepeatedQuestionId_IsReported()
    {
        IReadOnlyList<string> problems = Problems(typeof(ValidEntity), typeof(DisagreeingEntity));

        problems.ShouldContain(
            "The Vaccine form uses question ID 'valid_entity.text' more than once."
        );
    }

    [Theory]
    [InlineData(
        "BrokenTestLayout declares section 'Bad Section' with a badly formed ID or a blank title."
    )]
    [InlineData(
        "BrokenTestLayout declares task 'Bad Task' with a badly formed ID or a blank title."
    )]
    [InlineData("BrokenTestLayout declares section 'Bad Section' with blank sub text.")]
    [InlineData("BrokenTestLayout declares task 'Bad Task' with a blank hint.")]
    [InlineData(
        "BrokenTestLayout, section 'Bad Section', has task 'Bad Task' and task 'tied-task' with the same Order 10."
    )]
    [InlineData("BrokenTestLayout declares section 'repeated' more than once.")]
    [InlineData(
        "BrokenTestLayout has section 'Bad Section' and section 'repeated' with the same Order 10."
    )]
    [InlineData("Task ID 'repeated-task' is declared more than once in the form layouts.")]
    [InlineData("BrokenTestLayout declares section 'empty' with no tasks.")]
    [InlineData("BrokenTestLayout declares task 'no-pages-task' with no pages.")]
    [InlineData(
        "BrokenTestLayout declares page 'Bad Page' with a badly formed ID or a blank title."
    )]
    [InlineData("BrokenTestLayout declares page 'task-list', which is reserved for the task list.")]
    [InlineData(
        "BrokenTestLayout, task 'tied-task', has page 'tied-first' and page 'tied-second' with the same Order 10."
    )]
    [InlineData("Page ID 'repeated-page' is declared more than once in the form layouts.")]
    public void Validate_BrokenLayout_ReportsTheProblem(string expected)
    {
        var (declarations, problems) = FormModelScanner.Scan(_model, [typeof(BrokenTestLayout)]);

        problems.ShouldBeEmpty();
        FormModelValidator.Validate(declarations, _options).ShouldContain(expected);
    }

    [Fact]
    public void Validate_LayoutWithNoSections_IsReported()
    {
        var (declarations, _) = FormModelScanner.Scan(_model, [typeof(EmptyTestLayout)]);

        FormModelValidator
            .Validate(declarations, _options)
            .ShouldContain("EmptyTestLayout declares no sections.");
    }

    [Fact]
    public void Scan_LayoutWithNoParameterlessConstructor_IsReported()
    {
        var (_, problems) = FormModelScanner.Scan(_model, [typeof(NoDefaultConstructorTestLayout)]);

        problems.ShouldBe([
            "NoDefaultConstructorTestLayout is a FormLayout with no parameterless constructor.",
        ]);
    }

    [Fact]
    public void Scan_LayoutThatThrows_IsReported()
    {
        var (_, problems) = FormModelScanner.Scan(_model, [typeof(ThrowingTestLayout)]);

        problems.ShouldBe(["ThrowingTestLayout threw while being created: Broken layout."]);
    }

    [Fact]
    public void Validate_PageConditionOnUnplacedPage_ReportsTheUnknownPage()
    {
        IReadOnlyList<string> problems = Problems(typeof(UnplacedConditionEntity));

        problems.ShouldBe(
            [
                "UnplacedConditionEntity.Detail declares page 'unplaced-follow-up', which no FormLayout declares.",
                "The Vaccine form has no entry in FormVersions.",
            ],
            ignoreOrder: true
        );
    }

    [Fact]
    public void Validate_TwoLayoutsForOneForm_AreReported()
    {
        var (declarations, _) = FormModelScanner.Scan(
            _model,
            [typeof(VaccineTestLayout), typeof(VaccineTestLayout)]
        );

        FormModelValidator
            .Validate(declarations, _options)
            .ShouldContain(
                "More than one FormLayout declares the Vaccine form (VaccineTestLayout, VaccineTestLayout)."
            );
    }

    [Fact]
    public void Validate_EntityInTwoForms_IsReported()
    {
        var (declarations, _) = FormModelScanner.Scan(
            _model,
            [typeof(VaccineTestLayout), typeof(SplitMedicineTestLayout), typeof(SplitFormEntity)]
        );

        FormModelValidator
            .Validate(declarations, _options)
            .ShouldContain(
                "SplitFormEntity has pages in more than one form (Medicine, Vaccine); an entity belongs to one form."
            );
    }

    [Fact]
    public void Create_InvalidModel_ThrowsWithEveryProblem()
    {
        FormModelException exception = Should.Throw<FormModelException>(() =>
            FormModelFactory.Create(
                _model,
                _options,
                [typeof(VaccineTestLayout), typeof(BrokenEntity), typeof(RowsEntity)]
            )
        );

        exception.Problems.Count.ShouldBeGreaterThan(10);
        exception.Message.ShouldContain("RowsEntity has no int RevisionId");
    }

    [Fact]
    public void Scan_TextListsSharingACollection_EachReadTheirOwnKind()
    {
        Problems(typeof(ListedEntity)).ShouldBe(["The Vaccine form has no entry in FormVersions."]);

        var (declarations, _) = FormModelScanner.Scan(
            _model,
            [typeof(VaccineTestLayout), typeof(ListedEntity)]
        );
        FormModel form = FormModelBuilder.Build(declarations, _options)[RecordType.Vaccine];
        FormQuestion[] questions = [.. form.FindPage("listed")!.Questions];

        questions
            .Select(static q => (q.Id, q.Shape))
            .ShouldBe([
                ("listed_entity.first_names", AnswerShape.List),
                ("listed_entity.second_names", AnswerShape.List),
            ]);
        questions[0]
            .Rules.Select(static r => r.Kind)
            .ShouldBe([RuleKind.Required, RuleKind.MaxItems, RuleKind.Distinct]);
        (questions[0].AddAnotherLabel, questions[0].RemoveLabel).ShouldBe(
            ("Add another first name", "Remove first name")
        );
        questions[0].Entity.Includes.ShouldBe(["Names"]);
        questions[0]
            .Binding.Description.ShouldBe(
                "text-list:ListedEntity.Names:ListedEntityName by DisplayOrder where Kind = First value Name"
            );

        ListedEntity row = new()
        {
            Names =
            [
                new ListedEntityName
                {
                    Id = 1,
                    Name = "b",
                    Kind = ListedNameKind.First,
                    DisplayOrder = 2,
                },
                new ListedEntityName
                {
                    Id = 2,
                    Name = "x",
                    Kind = ListedNameKind.Second,
                    DisplayOrder = 1,
                },
                new ListedEntityName
                {
                    Id = 3,
                    Name = "a",
                    Kind = ListedNameKind.First,
                    DisplayOrder = 1,
                },
            ],
        };
        questions[0].Binding.Read(row).ShouldBe(FormAnswer.List(["a", "b"]));
        questions[1].Binding.Read(row).ShouldBe(FormAnswer.List(["x"]));
    }

    [Theory]
    [InlineData(
        "BrokenListedEntity.Mapped has [FormListSource] but is mapped by EF; put it on an unmapped property."
    )]
    [InlineData(
        "BrokenListedEntity.Unknown has [FormListSource] naming 'NoSuchNavigation', which is not a navigation of BrokenListedEntity."
    )]
    [InlineData("BrokenListedEntity.NotAList has [FormListSource], which only a TextList uses.")]
    [InlineData(
        "BrokenListedEntity.WrongKind has [FormListSource] kind Yes, but BrokenListedEntityName does not have exactly one non-nullable YesNoUnknown enum column to hold it."
    )]
    [InlineData(
        "BrokenListedEntity.ListLabelsOnText has [FormDistinct], which only a TextList or a single-value question of a group uses."
    )]
    [InlineData(
        "BrokenListedEntity.ListLabelsOnText sets RemoveLabel, which only a Group or a TextList uses."
    )]
    [InlineData(
        "BrokenListedEntity.FixedWidthList sets Width on a TextList, which only supports a share of the container such as Full."
    )]
    public void Validate_BrokenListedEntity_ReportsTheProblem(string expected)
    {
        Problems(typeof(BrokenListedEntity)).ShouldContain(expected);
    }

    private static IReadOnlyList<string> Problems(params Type[] types)
    {
        var (declarations, problems) = FormModelScanner.Scan(
            _model,
            [typeof(VaccineTestLayout), .. types]
        );
        return [.. problems, .. FormModelValidator.Validate(declarations, _options)];
    }
}
