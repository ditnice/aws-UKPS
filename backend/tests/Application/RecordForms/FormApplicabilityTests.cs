using Microsoft.EntityFrameworkCore.Metadata;
using Shouldly;
using UKPS.Api.Application.RecordForms.Conditions;
using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Application.RecordForms.Scanning;
using UKPS.Api.Persistence.Entities.ReferenceData;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Tests.Application.RecordForms.Scanning;

namespace UKPS.Api.Tests.Application.RecordForms;

// Which pages and questions apply, and which rows each request needs, over a small test form.
// Gate and Detail are Yes/No/Unknown radios. In page order:
// - gate (ConditionalEntity) asks Gate. If No, Reason is revealed on the same page.
// - follow-up (ConditionalEntity) is shown if Gate is Yes or Unknown, and skipped if No. It asks
//   Detail; if Detail is Yes, Notes is revealed on the same page.
// - end (ConditionalEntity) is always shown.
// - extra (ConditionalExtraEntity) is shown only if Gate is Yes.
// - other (ConditionalExtraEntity) is always shown.
//
// So the route through the form is:
// - Gate No: gate (with Reason), end, other.
// - Gate Unknown: gate, follow-up, end, other.
// - Gate Yes: gate, follow-up, end, extra, other.
//
// Why it is shaped like this:
// - Notes depends on Detail, which is on a page that may be skipped, so skipping follow-up must
//   also hide Notes, whatever Detail holds.
// - extra is in a second entity, so saving gate must load, and may clear, that entity's row.
// - other depends on nothing, so saving it clears nothing; it still reads the gate's entity,
//   because Gate decides where Back and Save and continue go.
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

    [Theory]
    [InlineData("7", true)]
    [InlineData("8", false)]
    [InlineData("9", false)]
    public void Evaluate_ReferenceTrigger_ComparesTheCodeOfTheAnswer(
        string procedureId,
        bool applies
    )
    {
        // The condition names a code; the answer is an ID, and 9 has no loaded code.
        FormModel form = BuildForm(typeof(CodedEntity));
        ReferenceCodes codes = new(
            new Dictionary<Type, IReadOnlyList<(int Id, string Code)>>
            {
                [typeof(MhraProcedureType)] =
                [
                    (7, MhraProcedureTypeCodes.InternationalRecognition),
                    (8, MhraProcedureTypeCodes.National),
                ],
            }
        );
        Dictionary<string, FormAnswer> answers = new(StringComparer.Ordinal)
        {
            ["coded_entity.procedure_id"] = FormAnswer.Single(procedureId),
            ["coded_entity.irp_details"] = FormAnswer.Single(null),
        };

        FormApplicability
            .Evaluate(form, answers, codes)
            .Applies(form.FindQuestion("coded_entity.irp_details")!)
            .ShouldBe(applies);
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

    [Fact]
    public void TriggerEntities_HoldTheEntitiesConditionsRead()
    {
        // Both triggers (gate and detail) are on ConditionalEntity. ConditionalExtraEntity has a page
        // that depends on the gate but holds no trigger, so its rows are not needed.
        _form.TriggerEntities.Select(static e => e.ClrType).ShouldBe([typeof(ConditionalEntity)]);
    }

    private static FormModel BuildForm(params Type[] types)
    {
        IModel model = ScanningTestDbContext.CreateModel();
        var (entities, _) = FormModelScanner.Scan(
            model,
            types.Length > 0 ? types : [typeof(ConditionalEntity), typeof(ConditionalExtraEntity)]
        );
        return FormModelBuilder.Build(entities, ScanningTestOptions.Create())[RecordType.Vaccine];
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
