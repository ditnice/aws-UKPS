using Microsoft.EntityFrameworkCore.Metadata;
using Shouldly;
using UKPS.Api.Application.RecordForms.Conditions;
using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Application.RecordForms.Options;
using UKPS.Api.Application.RecordForms.Scanning;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Tests.Application.RecordForms.Scanning;

namespace UKPS.Api.Tests.Application.RecordForms;

/// <summary>
/// Applicability over <see cref="ConditionalEntity"/> and <see cref="ConditionalExtraEntity"/>:
/// a reveal on the gate page; a follow-up page shown when the gate is Yes or Unknown, with a
/// reveal of its own; an end page; an extra page in the other entity shown when the gate is Yes;
/// and an other page that depends on nothing.
/// </summary>
public class FormApplicabilityTests
{
    private const string Gate = "conditional_entity.gate";
    private const string Reason = "conditional_entity.reason";
    private const string Detail = "conditional_entity.detail";
    private const string Notes = "conditional_entity.notes";
    private const string End = "conditional_entity.end";
    private const string Extra = "conditional_extra_entity.extra";
    private const string Other = "conditional_extra_entity.other";

    private static readonly FormModel _form = BuildForm();

    [Fact]
    public void Evaluate_NothingAnswered_OnlyUnconditionalPagesAndQuestionsApply()
    {
        Applicability applicability = FormApplicability.Evaluate(_form, Answers());

        AppliedPages(applicability).ShouldBe(["gate", "end", "other"]);
        AppliedQuestions(applicability).ShouldBe([Gate, End, Other]);
    }

    [Fact]
    public void Evaluate_RevealConditionMet_TheQuestionApplies()
    {
        Applicability applicability = FormApplicability.Evaluate(_form, Answers((Gate, "No")));

        AppliedQuestions(applicability).ShouldBe([Gate, Reason, End, Other]);
    }

    [Theory]
    [InlineData("Yes", new[] { "gate", "follow-up", "end", "extra", "other" })]
    [InlineData("Unknown", new[] { "gate", "follow-up", "end", "other" })]
    public void Evaluate_PageConditions_ApplyThePagesTheyName(string gate, string[] pages)
    {
        Applicability applicability = FormApplicability.Evaluate(
            _form,
            Answers((Gate, gate), (Detail, "Yes"))
        );

        AppliedPages(applicability).ShouldBe(pages);
        AppliedQuestions(applicability).ShouldContain(Notes);
    }

    [Fact]
    public void Evaluate_TriggerOnAPageThatDoesNotApply_HidesWhatItReveals()
    {
        Applicability applicability = FormApplicability.Evaluate(
            _form,
            Answers((Gate, "No"), (Detail, "Yes"))
        );

        AppliedQuestions(applicability).ShouldNotContain(Detail);
        AppliedQuestions(applicability).ShouldNotContain(Notes);
    }

    [Fact]
    public void Applies_TriggerAnswerNotLoaded_Throws()
    {
        Dictionary<string, FormAnswer> answers = Answers();
        answers.Remove(Gate);
        Applicability applicability = FormApplicability.Evaluate(_form, answers);

        Should.Throw<InvalidOperationException>(() =>
            applicability.Applies(_form.FindPage("follow-up")!)
        );
    }

    [Fact]
    public void Applies_OnlyReadsTheAnswersItDependsOn()
    {
        // Only the end and other pages' answers: neither depends on a trigger.
        Dictionary<string, FormAnswer> answers = new(StringComparer.Ordinal)
        {
            [End] = FormAnswer.Single("End"),
            [Other] = FormAnswer.Single("Other"),
        };
        Applicability applicability = FormApplicability.Evaluate(_form, answers);

        applicability.Applies(_form.FindPage("other")!).ShouldBeTrue();
        applicability.Applies(_form.FindQuestion(End)!).ShouldBeTrue();
    }

    [Fact]
    public void ClearInapplicable_ClearsTheQuestionsGivenThatDoNotApply()
    {
        Dictionary<string, FormAnswer> answers = Answers(
            (Gate, "No"),
            (Reason, "Because"),
            (Detail, "Yes"),
            (Notes, "Notes"),
            (End, "End"),
            (Extra, "Extra")
        );

        Dictionary<string, FormAnswer> cleared = FormApplicability.ClearInapplicable(
            _form.AffectedBy(_form.FindPage("gate")!),
            answers,
            FormApplicability.Evaluate(_form, answers)
        );

        Answered(cleared).ShouldBe([(End, "End"), (Gate, "No"), (Reason, "Because")]);
    }

    [Fact]
    public void ClearInapplicable_LeavesQuestionsNotGivenAlone()
    {
        // The follow-up page does not apply, but saving the end page cannot have changed that.
        Dictionary<string, FormAnswer> answers = Answers((Gate, "No"), (Detail, "Yes"));

        Dictionary<string, FormAnswer> cleared = FormApplicability.ClearInapplicable(
            _form.AffectedBy(_form.FindPage("end")!),
            answers,
            FormApplicability.Evaluate(_form, answers)
        );

        Answered(cleared).ShouldBe([(Detail, "Yes"), (Gate, "No")]);
    }

    [Fact]
    public void NextAndPreviousPage_SkipPagesThatDoNotApply()
    {
        FormPage gate = _form.FindPage("gate")!;
        FormPage end = _form.FindPage("end")!;
        Applicability hidden = FormApplicability.Evaluate(_form, Answers((Gate, "No")));
        Applicability shown = FormApplicability.Evaluate(_form, Answers((Gate, "Yes")));

        _form.NextPage(gate, hidden).ShouldBe(end);
        _form.PreviousPage(end, hidden).ShouldBe(gate);
        _form.NextPage(end, hidden).ShouldNotBeNull().Id.ShouldBe("other");
        _form.NextPage(gate, shown).ShouldNotBeNull().Id.ShouldBe("follow-up");
        _form.PreviousPage(end, shown).ShouldNotBeNull().Id.ShouldBe("follow-up");
        _form.NextPage(end, shown).ShouldNotBeNull().Id.ShouldBe("extra");
        _form.PreviousPage(gate, shown).ShouldBeNull();
    }

    [Fact]
    public void AffectedBy_HoldsThePagesQuestionsAndEverythingThatDependsOnThem()
    {
        AffectedBy("gate").ShouldBe([Gate, Reason, Detail, Notes, Extra]);
        AffectedBy("follow-up").ShouldBe([Detail, Notes]);
        AffectedBy("end").ShouldBe([End]);
        AffectedBy("other").ShouldBe([Other]);
    }

    [Fact]
    public void EntitiesToReadAndSave_AddOnlyWhatConditionsAcrossPagesNeed()
    {
        // Every page reads the gate's entity, because it decides where Back and Save and continue
        // go; only saving the gate page needs the extra page's entity, whose answer it may clear.
        EntitiesToRead("other")
            .ShouldBe([typeof(ConditionalExtraEntity), typeof(ConditionalEntity)]);
        EntitiesToSave("other")
            .ShouldBe([typeof(ConditionalExtraEntity), typeof(ConditionalEntity)]);
        EntitiesToRead("gate").ShouldBe([typeof(ConditionalEntity)]);
        EntitiesToSave("gate")
            .ShouldBe([typeof(ConditionalEntity), typeof(ConditionalExtraEntity)]);
        EntitiesToSave("end").ShouldBe([typeof(ConditionalEntity)]);
    }

    private static FormModel BuildForm()
    {
        IModel model = ScanningTestDbContext.CreateModel();
        var (entities, _) = FormModelScanner.Scan(
            model,
            [typeof(ConditionalEntity), typeof(ConditionalExtraEntity)]
        );
        return FormModelBuilder.Build(entities, OptionProviderRegistry.CreateDefault())[
            RecordType.Vaccine
        ];
    }

    /// <summary>Every question unanswered, apart from those given.</summary>
    private static Dictionary<string, FormAnswer> Answers(
        params (string QuestionId, string Value)[] given
    )
    {
        Dictionary<string, FormAnswer> answers = _form.Questions.ToDictionary(
            static q => q.Id,
            static q => FormAnswer.Empty(q.Shape),
            StringComparer.Ordinal
        );
        foreach (var (questionId, value) in given)
        {
            answers[questionId] = FormAnswer.Single(value);
        }

        return answers;
    }

    private static (string, string?)[] Answered(Dictionary<string, FormAnswer> answers) =>
        answers
            .Where(static a => !a.Value.IsEmpty)
            .Select(static a => (a.Key, a.Value.Value))
            .OrderBy(static a => a.Key, StringComparer.Ordinal)
            .ToArray();

    private static string[] AppliedPages(Applicability applicability) =>
        _form.Pages.Where(applicability.Applies).Select(static p => p.Id).ToArray();

    private static string[] AppliedQuestions(Applicability applicability) =>
        _form.Questions.Where(applicability.Applies).Select(static q => q.Id).ToArray();

    private static string[] AffectedBy(string pageId) =>
        _form.AffectedBy(_form.FindPage(pageId)!).Select(static q => q.Id).ToArray();

    private static Type[] EntitiesToRead(string pageId) =>
        _form.EntitiesToRead(_form.FindPage(pageId)!).Select(static e => e.ClrType).ToArray();

    private static Type[] EntitiesToSave(string pageId) =>
        _form.EntitiesToSave(_form.FindPage(pageId)!).Select(static e => e.ClrType).ToArray();
}
