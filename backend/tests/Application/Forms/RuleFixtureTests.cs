using System.Text.Json.Nodes;
using Shouldly;
using UKPS.Api.Application.Forms;
using UKPS.Api.Application.Forms.Bindings;
using UKPS.Api.Application.Forms.Definitions;
using UKPS.Api.Application.Forms.Options;
using UKPS.Api.Application.Forms.Rules;
using UKPS.Api.Persistence.Entities.MedicinesRevisionContent;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Tests.Application.Forms;

/// <summary>
/// Runs the shared rule fixture (also run by the frontend against its Zod schema) through the
/// server's normalise-then-validate path, so the client and server cannot drift apart.
/// </summary>
public class RuleFixtureTests
{
    private const string QuestionId = "table.field";

    private static readonly JsonArray _cases = JsonNode.Parse(
        File.ReadAllText(
            Path.Combine(
                AppContext.BaseDirectory,
                "Application",
                "Forms",
                "Fixtures",
                "rule-cases.json"
            )
        )
    )!["cases"]!.AsArray();

    public static TheoryData<string> CaseNames =>
        new(_cases.Select(c => c!["name"]!.GetValue<string>()));

    [Theory]
    [MemberData(nameof(CaseNames))]
    public void Case_MatchesTheExpectedError(string name)
    {
        var testCase = _cases.Single(c =>
            string.Equals(c!["name"]!.GetValue<string>(), name, StringComparison.Ordinal)
        )!;
        var type = Enum.Parse<QuestionType>(testCase["type"]!.GetValue<string>());
        var page = CreatePage(type, [.. testCase["rules"]!.AsArray().Select(r => CreateRule(r!))]);
        var value = testCase["value"]?.DeepClone();
        type.IsValidShape(value).ShouldBeTrue("Fixture values must have a valid shape.");

        var answers = new Dictionary<string, JsonNode?>(StringComparer.Ordinal)
        {
            [QuestionId] = type.Normalise(value),
        };
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        PageAnswerValidator.ValidateRules(page, answers, errors);

        var expected = testCase["error"]?.GetValue<string>();
        if (expected is null)
        {
            errors.ShouldBeEmpty();
        }
        else
        {
            errors[QuestionId].ShouldBe([expected]);
        }
    }

    [Fact]
    public void Fixture_CoversEveryRuleKind()
    {
        var covered = _cases
            .SelectMany(c => c!["rules"]!.AsArray())
            .Select(r => Enum.Parse<RuleKind>(r!["kind"]!.GetValue<string>()))
            .ToHashSet();

        covered.ShouldBe(Enum.GetValues<RuleKind>(), ignoreOrder: true);
    }

    private static QuestionRule CreateRule(JsonNode rule)
    {
        var message = rule["message"]!.GetValue<string>();
        return Enum.Parse<RuleKind>(rule["kind"]!.GetValue<string>()) switch
        {
            RuleKind.Required => new RequiredRule(message),
            RuleKind.MaxLength => new MaxLengthRule(rule["value"]!.GetValue<int>(), message),
            RuleKind.MaxItems => new MaxItemsRule(rule["value"]!.GetValue<int>(), message),
            var kind => throw new NotSupportedException(
                $"Add {kind} to {nameof(RuleFixtureTests)}."
            ),
        };
    }

    // The binding and options only need to satisfy the type; rules never touch them.
    private static PageDefinition CreatePage(QuestionType type, IReadOnlyList<QuestionRule> rules)
    {
        IQuestionBinding binding = Bind.Column((MedicinesProductDetail x) => x.Indication);
        QuestionOptions? options = type.HasOptions()
            ? Choices.Enum((YesNoUnknown.Yes, "Yes"))
            : null;
        var question = new QuestionDefinition(
            QuestionId,
            type,
            "Label",
            null,
            null,
            rules,
            options,
            binding
        );
        return new PageDefinition("page", "Page", "section", "Section", [question]);
    }
}
