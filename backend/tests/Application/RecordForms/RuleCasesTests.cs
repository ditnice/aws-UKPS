using System.Text.Json;
using Shouldly;
using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Application.RecordForms.Rules;

namespace UKPS.Api.Tests.Application.RecordForms;

/// <summary>
/// Runs the rule cases shared with the frontend, so the two rule implementations cannot drift.
/// </summary>
public class RuleCasesTests
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
    };

    private static readonly IReadOnlyList<RuleCase> _cases = LoadCases();

    public static TheoryData<string> CaseNames() => new(_cases.Select(static c => c.Name));

    [Theory]
    [MemberData(nameof(CaseNames))]
    public void RuleCase_GivesTheExpectedError(string name)
    {
        RuleCase ruleCase = _cases.Single(c =>
            string.Equals(c.Name, name, StringComparison.Ordinal)
        );
        AnswerShape shape = ruleCase.Multiple ? AnswerShape.Multi : AnswerShape.Single;

        var (answer, parseError) = AnswerParser.Parse(shape, ruleCase.Answer);

        parseError.ShouldBeNull();
        RuleEvaluator.FirstFailure(ruleCase.Rules, answer!).ShouldBe(ruleCase.Error);
    }

    [Fact]
    public void Fixture_CoversEveryRuleKind()
    {
        RuleKind[] covered = _cases
            .SelectMany(static c => c.Rules)
            .Select(static r => r.Kind)
            .Distinct()
            .ToArray();

        foreach (RuleKind kind in Enum.GetValues<RuleKind>())
        {
            covered.ShouldContain(kind, $"rule-cases.json has no case for {kind}.");
            _cases.ShouldContain(
                c => c.Error != null && c.Rules.Any(r => r.Kind == kind && r.Message == c.Error),
                $"rule-cases.json has no failing case for {kind}."
            );
        }
    }

    private static IReadOnlyList<RuleCase> LoadCases()
    {
        string path = Path.Combine(
            AppContext.BaseDirectory,
            "Application",
            "RecordForms",
            "Fixtures",
            "rule-cases.json"
        );
        RuleCaseFile file = JsonSerializer.Deserialize<RuleCaseFile>(
            File.ReadAllText(path),
            _jsonOptions
        )!;
        return file.Cases;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1812",
        Justification = "Created by JSON deserialisation."
    )]
    private sealed record RuleCaseFile(IReadOnlyList<RuleCase> Cases);

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1812",
        Justification = "Created by JSON deserialisation."
    )]
    private sealed record RuleCase(
        string Name,
        bool Multiple,
        IReadOnlyList<FormRule> Rules,
        JsonElement Answer,
        string? Error
    );
}
