using Microsoft.EntityFrameworkCore.Metadata;
using Shouldly;
using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Application.RecordForms.Options;
using UKPS.Api.Application.RecordForms.Scanning;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Tests.Application.RecordForms.Scanning;

public class FormModelValidatorTests
{
    private static readonly IModel _model = ScanningTestDbContext.CreateModel();
    private static readonly OptionProviderRegistry _options =
        OptionProviderRegistry.CreateDefault();

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
    public void Validate_BrokenEntity_ReportsTheProblem(string expected)
    {
        Problems(typeof(BrokenEntity)).ShouldContain(expected);
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
            "The Vaccine form declares page 'text' with different titles or orders."
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
