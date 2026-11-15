using System.Text.Json;
using Shouldly;
using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Application.RecordForms.Rules;

namespace UKPS.Api.Tests.Application.RecordForms;

/// <summary>
/// Runs the prefixed-number cases shared with the frontend, so the two implementations cannot
/// drift.
/// </summary>
public class PrefixedNumberCasesTests
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    private static readonly IReadOnlyList<PrefixedNumberCase> _cases = LoadCases();

    public static TheoryData<string> CaseNames() => new(_cases.Select(static c => c.Name));

    [Theory]
    [MemberData(nameof(CaseNames))]
    public void PrefixedNumberCase_GivesTheExpectedValueAndError(string name)
    {
        PrefixedNumberCase prefixedCase = _cases.Single(c =>
            string.Equals(c.Name, name, StringComparison.Ordinal)
        );
        var (answer, parseError) = AnswerParser.Parse(AnswerShape.Single, prefixedCase.Answer);
        parseError.ShouldBeNull();

        var (value, error) = PrefixedNumberAnswer.Evaluate(prefixedCase.Prefixes, answer!.Value);

        error.ShouldBe(prefixedCase.Error);
        value.ShouldBe(prefixedCase.Value);
    }

    private static IReadOnlyList<PrefixedNumberCase> LoadCases()
    {
        string path = Path.Combine(
            AppContext.BaseDirectory,
            "Application",
            "RecordForms",
            "Fixtures",
            "prefixed-number-cases.json"
        );
        PrefixedNumberCaseFile file = JsonSerializer.Deserialize<PrefixedNumberCaseFile>(
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
    private sealed record PrefixedNumberCaseFile(IReadOnlyList<PrefixedNumberCase> Cases);

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1812",
        Justification = "Created by JSON deserialisation."
    )]
    private sealed record PrefixedNumberCase(
        string Name,
        IReadOnlyList<string> Prefixes,
        JsonElement Answer,
        string? Value,
        string? Error
    );
}
