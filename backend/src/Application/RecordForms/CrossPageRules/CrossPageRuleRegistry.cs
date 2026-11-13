using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Application.RecordForms.Scanning;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.RecordForms.CrossPageRules;

/// <summary>The cross-page rules of each record type's form. A singleton.</summary>
internal sealed class CrossPageRuleRegistry
{
    // Medicine rules are added as the questions they compare are annotated, for example the UK
    // submission, licence and launch dates and the EAMS exemption.
    private static readonly IReadOnlyList<CrossPageRule> _medicineRules = [];

    private readonly IReadOnlyDictionary<RecordType, IReadOnlyList<CrossPageRule>> _rules;

    private CrossPageRuleRegistry(
        IReadOnlyDictionary<RecordType, IReadOnlyList<CrossPageRule>> rules
    )
    {
        _rules = rules;
    }

    public IReadOnlyList<CrossPageRule> For(RecordType recordType) =>
        _rules.GetValueOrDefault(recordType) ?? [];

    public static CrossPageRuleRegistry CreateDefault(FormModelRegistry models) =>
        Create(
            models,
            new Dictionary<RecordType, IReadOnlyList<CrossPageRule>>
            {
                [RecordType.Medicine] = _medicineRules,
            }
        );

    /// <summary>
    /// Checks every rule against its record type's form, so a rule naming a missing or wrong
    /// question stops the API starting.
    /// </summary>
    /// <exception cref="FormModelException">A rule doesn't match its form.</exception>
    public static CrossPageRuleRegistry Create(
        FormModelRegistry models,
        IReadOnlyDictionary<RecordType, IReadOnlyList<CrossPageRule>> rules
    )
    {
        List<string> problems = [];
        foreach (var (recordType, typeRules) in rules)
        {
            if (typeRules.Count == 0)
            {
                continue;
            }

            if (models.Find(recordType) is not FormModel model)
            {
                problems.Add($"Cross-page rules are defined for {recordType}, which has no form.");
                continue;
            }

            problems.AddRange(typeRules.SelectMany(rule => rule.Problems(model)));
            problems.AddRange(
                typeRules
                    .GroupBy(static rule => rule.Id, StringComparer.Ordinal)
                    .Where(static g => g.Count() > 1)
                    .Select(g => $"Cross-page rule ID '{g.Key}' is used more than once.")
            );
        }

        return problems.Count > 0 ? throw new FormModelException(problems) : new(rules);
    }
}
