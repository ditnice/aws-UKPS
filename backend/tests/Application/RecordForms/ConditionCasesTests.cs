using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Shouldly;
using UKPS.Api.Application.RecordForms.Conditions;
using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Application.RecordForms.Rules;

namespace UKPS.Api.Tests.Application.RecordForms;

/// <summary>
/// Runs the condition cases shared with the frontend, so the two condition implementations
/// cannot drift.
/// </summary>
public class ConditionCasesTests
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private static readonly IReadOnlyList<ConditionCase> _cases = LoadCases();

    public static TheoryData<string> CaseNames() => new(_cases.Select(static c => c.Name));

    [Theory]
    [MemberData(nameof(CaseNames))]
    public void ConditionCase_IsMetAsExpected(string name)
    {
        ConditionCase conditionCase = _cases.Single(c =>
            string.Equals(c.Name, name, StringComparison.Ordinal)
        );
        AnswerShape shape =
            conditionCase.Answer.ValueKind == JsonValueKind.Array
                ? AnswerShape.Multi
                : AnswerShape.Single;
        var (answer, parseError) = AnswerParser.Parse(shape, conditionCase.Answer);
        parseError.ShouldBeNull();

        FormCondition condition = conditionCase.Condition.Kind switch
        {
            ConditionKind.AnswerIn => new AnswerInCondition(
                conditionCase.Condition.QuestionId,
                conditionCase.Condition.Values
            ),
            _ => throw new InvalidOperationException(
                $"Unsupported condition kind {conditionCase.Condition.Kind}."
            ),
        };

        condition.IsMet(_ => answer!).ShouldBe(conditionCase.Met);
    }

    [Fact]
    public void Fixture_CoversEveryConditionKind()
    {
        foreach (ConditionKind kind in Enum.GetValues<ConditionKind>())
        {
            _cases.ShouldContain(
                c => c.Condition.Kind == kind && c.Met,
                $"condition-cases.json has no met case for {kind}."
            );
            _cases.ShouldContain(
                c => c.Condition.Kind == kind && !c.Met,
                $"condition-cases.json has no unmet case for {kind}."
            );
        }
    }

    private static IReadOnlyList<ConditionCase> LoadCases()
    {
        string path = Path.Combine(
            AppContext.BaseDirectory,
            "Application",
            "RecordForms",
            "Fixtures",
            "condition-cases.json"
        );
        return JsonSerializer
            .Deserialize<ConditionCaseFile>(File.ReadAllText(path), _jsonOptions)!
            .Cases;
    }

    [SuppressMessage("Performance", "CA1812", Justification = "Created by JSON deserialisation.")]
    private sealed record ConditionCaseFile(IReadOnlyList<ConditionCase> Cases);

    [SuppressMessage("Performance", "CA1812", Justification = "Created by JSON deserialisation.")]
    private sealed record ConditionCase(
        string Name,
        CaseCondition Condition,
        JsonElement Answer,
        bool Met
    );

    [SuppressMessage("Performance", "CA1812", Justification = "Created by JSON deserialisation.")]
    private sealed record CaseCondition(
        ConditionKind Kind,
        string QuestionId,
        IReadOnlyList<string> Values
    );
}
