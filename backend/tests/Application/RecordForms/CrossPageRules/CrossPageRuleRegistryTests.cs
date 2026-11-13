using Shouldly;
using UKPS.Api.Application.RecordForms;
using UKPS.Api.Application.RecordForms.CrossPageRules;
using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Application.RecordForms.Options;
using UKPS.Api.Application.RecordForms.Scanning;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Tests.Application.RecordForms.CrossPageRules;

public class CrossPageRuleRegistryTests
{
    private const string Submission = "dated_entity.submission_date";
    private const string Actual = "dated_entity.actual_date";

    private static readonly FormModelRegistry _models = new(
        new Dictionary<RecordType, FormModel>
        {
            [RecordType.Vaccine] = DateOrderRuleTests.BuildForm(),
        }
    );

    [Fact]
    public void CreateDefault_TheMedicineRules_MatchTheMedicineForm()
    {
        FormModelRegistry models = FormModelFactory.Create(OptionProviderRegistry.CreateDefault());

        Should.NotThrow(() => CrossPageRuleRegistry.CreateDefault(models));
    }

    [Fact]
    public void Create_ValidRules_AreReturnedForTheirRecordType()
    {
        CrossPageRule rule = Rule("TEST-01", Actual);

        CrossPageRuleRegistry registry = Create([rule]);

        registry.For(RecordType.Vaccine).ShouldBe([rule]);
        registry.For(RecordType.Medicine).ShouldBeEmpty();
    }

    [Fact]
    public void Create_RuleNamingAMissingQuestion_Throws()
    {
        FormModelException exception = Should.Throw<FormModelException>(() =>
            Create([Rule("TEST-01", "no_such.question")])
        );

        exception.Problems.ShouldBe([
            "Cross-page rule 'TEST-01' names question 'no_such.question', which is not in the form.",
        ]);
    }

    [Fact]
    public void Create_DuplicateRuleIds_Throws()
    {
        FormModelException exception = Should.Throw<FormModelException>(() =>
            Create([Rule("TEST-01", Actual), Rule("TEST-01", Actual)])
        );

        exception.Problems.ShouldBe(["Cross-page rule ID 'TEST-01' is used more than once."]);
    }

    [Fact]
    public void Create_RulesForARecordTypeWithNoForm_Throws()
    {
        FormModelException exception = Should.Throw<FormModelException>(() =>
            CrossPageRuleRegistry.Create(
                _models,
                new Dictionary<RecordType, IReadOnlyList<CrossPageRule>>
                {
                    [RecordType.Medicine] = [Rule("TEST-01", Actual)],
                }
            )
        );

        exception.Problems.ShouldBe([
            "Cross-page rules are defined for Medicine, which has no form.",
        ]);
    }

    private static CrossPageRuleRegistry Create(IReadOnlyList<CrossPageRule> rules) =>
        CrossPageRuleRegistry.Create(
            _models,
            new Dictionary<RecordType, IReadOnlyList<CrossPageRule>>
            {
                [RecordType.Vaccine] = rules,
            }
        );

    private static DateOrderRule Rule(string id, string earlier) =>
        new(id, earlier, Submission, AllowSame: true, "Earlier", "Later");
}
