using Microsoft.EntityFrameworkCore.Metadata;
using Shouldly;
using UKPS.Api.Application.RecordForms.Conditions;
using UKPS.Api.Application.RecordForms.CrossPageRules;
using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Application.RecordForms.Scanning;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Tests.Application.RecordForms.Scanning;

namespace UKPS.Api.Tests.Application.RecordForms.CrossPageRules;

// Over a small test form: Actual applies only when Submission is an actual date, and Gate is a
// Yes/No/Unknown radio used as the exemption. The rule puts Actual on or before Submission, so the
// earlier date can be estimated and the later one is always an exact day.
public class DateOrderRuleTests
{
    private const string Submission = "dated_entity.submission_date";
    private const string Actual = "dated_entity.actual_date";
    private const string Gate = "conditional_entity.gate";
    private const string EarlierMessage = "Actual must be on or before submission";
    private const string LaterMessage = "Submission must be on or after actual";

    private static readonly FormModel _form = BuildForm();

    [Theory]
    [InlineData("2026-08-14", "2026-08-15")]
    [InlineData("2026-08", "2026-08-15")]
    // The docs' example: 15 August is within Q3, so the two may be in order.
    [InlineData("2026-Q3", "2026-08-15")]
    public void Check_DatesThatCanBeInOrder_Pass(string actual, string submission)
    {
        Check(Rule(allowSame: false), (Actual, actual), (Submission, submission)).ShouldBeEmpty();
    }

    [Theory]
    [InlineData("2026-08-16", "2026-08-15")]
    [InlineData("2026-09", "2026-08-31")]
    [InlineData("2026-Q4", "2026-09-30")]
    public void Check_DatesThatCanNotBeInOrder_FailOnBothQuestions(string actual, string submission)
    {
        Check(Rule(allowSame: true), (Actual, actual), (Submission, submission))
            .ShouldBe([(Actual, EarlierMessage), (Submission, LaterMessage)]);
    }

    [Theory]
    [InlineData("2026-08-15", "2026-08-15")]
    // A period can only be on the later date when it starts on it.
    [InlineData("2026-08", "2026-08-01")]
    [InlineData("2026-Q3", "2026-07-01")]
    public void Check_OnlyTheSameDay_PassesOnlyWhenAllowed(string actual, string submission)
    {
        (string, string?)[] answers = [(Actual, actual), (Submission, submission)];

        Check(Rule(allowSame: true), answers).ShouldBeEmpty();
        Check(Rule(allowSame: false), answers).Count.ShouldBe(2);
    }

    [Fact]
    public void Check_ExemptionMet_IsSkipped()
    {
        DateOrderRule rule = Rule(allowSame: true) with
        {
            ExemptWhen = new AnswerInCondition(Gate, ["Yes"]),
        };
        (string, string?)[] outOfOrder = [(Actual, "2026-09-01"), (Submission, "2026-08-15")];

        Check(rule, [.. outOfOrder, (Gate, "Yes")]).ShouldBeEmpty();
        Check(rule, [.. outOfOrder, (Gate, "No")]).Count.ShouldBe(2);
        Check(rule, outOfOrder).Count.ShouldBe(2);
    }

    [Theory]
    // Actual doesn't apply unless Submission is an actual date, so its stored answer is ignored.
    [InlineData("2026-09-01", "2026-Q3")]
    [InlineData("2026-09-01", null)]
    [InlineData(null, "2026-08-15")]
    [InlineData("2026-13", "2026-08-15")]
    public void Check_DateMissingNotApplyingOrInvalid_IsSkipped(string? actual, string? submission)
    {
        Check(Rule(allowSame: true), (Actual, actual), (Submission, submission)).ShouldBeEmpty();
    }

    [Fact]
    public void Problems_QuestionsNotInTheFormOrNotDates_AreReported()
    {
        DateOrderRule rule = Rule(allowSame: true) with
        {
            EarlierQuestionId = "no_such.question",
            LaterQuestionId = Gate,
        };

        rule.Problems(_form)
            .ShouldBe([
                "Cross-page rule 'TEST-01' names question 'no_such.question', which is not in the form.",
                $"Cross-page rule 'TEST-01' compares '{Gate}', which is not a date.",
            ]);
        Rule(allowSame: true).Problems(_form).ShouldBeEmpty();
    }

    private static DateOrderRule Rule(bool allowSame) =>
        new("TEST-01", Actual, Submission, allowSame, EarlierMessage, LaterMessage);

    private static List<(string QuestionId, string Message)> Check(
        DateOrderRule rule,
        params (string QuestionId, string? Value)[] given
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

        Applicability applicability = FormApplicability.Evaluate(_form, answers);
        return [.. rule.Check(_form, answers, applicability)];
    }

    internal static FormModel BuildForm()
    {
        IModel model = ScanningTestDbContext.CreateModel();
        var (entities, _) = FormModelScanner.Scan(
            model,
            [typeof(ConditionalEntity), typeof(DatedEntity)]
        );
        return FormModelBuilder.Build(entities, ScanningTestOptions.Create())[RecordType.Vaccine];
    }
}
