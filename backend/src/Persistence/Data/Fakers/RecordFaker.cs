using Bogus;
using UKPS.Api.Persistence.Entities.RecordWorkflow;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Persistence.Entities.MedicinesRevisionContent;

namespace UKPS.Api.Persistence.Data.Fakers;

internal sealed class RecordFaker : Faker<Record>
{
    public RecordFaker()
    {
        RuleFor(x => x.RecordType, f => f.PickRandom<RecordType>());
        RuleFor(x => x.RecordStatus, f => f.PickRandom<RecordStatus>());
        RuleFor(x => x.CreatedAt, f => f.Date.Past(5).ToUniversalTime());
        RuleFor(
            x => x.ReviewedAt,
            (f, o) =>
                f.Random.Bool(0.8f)
                    ? f.Date.Between(o.CreatedAt, DateTime.UtcNow).ToUniversalTime()
                    : null
        );
        RuleFor(
            x => x.Revisions, RecordRevisionFaker.Create().Generate(1)
        );
    }
}

internal static class RecordRevisionFaker
{
    public static Faker<RecordRevision> Create(
        int? basedOnRevisionId = null)
    {
        return new Faker<RecordRevision>()
            .RuleFor(x => x.BasedOnRevisionId, f => basedOnRevisionId)
            .RuleFor(x => x.RevisionNo, f => f.Random.Int(1, 20))
            .RuleFor(x => x.MajorVersion, f => f.Random.Int(1, 5))
            .RuleFor(x => x.MinorVersion, f => f.Random.Int(0, 10))
            .RuleFor(x => x.WorkflowStatus, f => f.PickRandom<WorkflowStatus>())
            .RuleFor(x => x.CreatedAt, f => DateTime.SpecifyKind(f.Date.Past(1), DateTimeKind.Utc))
            .RuleFor(x => x.Record, _ => null)
            .RuleFor(x => x.BasedOnRevision, _ => null)
            .RuleFor(x => x.DerivedRevisions, _ => [])
            .RuleFor(x => x.UpdatedByUser, _ => null)
            .RuleFor(x => x.SubmittedByUser, _ => null)
            .RuleFor(x => x.QaReviews, _ => [])
            .RuleFor(x => x.Events, _ => []);
    }
}


internal static class MedicinesProductDetailFaker
{
    public static Faker<MedicinesProductDetail> Create()
    {
        return new Faker<MedicinesProductDetail>()
            .RuleFor(x => x.RecordTitle, f =>
                f.PickRandom(
                    "Chronic hepatitis C in adults",
                    "Type 2 diabetes in adults",
                    "Moderate to severe asthma",
                    "Advanced breast cancer",
                    "Rheumatoid arthritis",
                    "Chronic kidney disease"
                ))
            .RuleFor(x => x.BrandedName, f =>
                f.Random.Bool(0.7f) ? f.Commerce.ProductName() : null)
            .RuleFor(x => x.Indication, f =>
                f.Lorem.Sentence(8).TrimEnd('.'))
            .RuleFor(x => x.IndicationIsPaediatric, f =>
                f.Random.Bool(0.8f)
                    ? f.PickRandom<IndicationPaediatricStatus>()
                    : null)
            .RuleFor(x => x.IndicationIsCancer, f =>
                f.Random.Bool(0.8f)
                    ? f.PickRandom<YesNoUnknown>()
                    : null)
            .RuleFor(x => x.IndicationIsRareDisease, f =>
                f.Random.Bool(0.8f)
                    ? f.PickRandom<YesNoUnknown>()
                    : null)
            .RuleFor(x => x.NiceTaDevelopmentId, f =>
                f.Random.Bool(0.5f)
                    ? f.Random.Bool()
                        ? $"GID-TA{f.Random.Int(1000, 9999)}"
                        : $"GID-HST{f.Random.Int(1000, 9999)}"
                    : null)
            .RuleFor(x => x.Presentation, f =>
                f.Random.Bool(0.8f)
                    ? f.PickRandom(
                        "Tablets",
                        "Capsules",
                        "Oral solution",
                        "Film-coated tablets",
                        "Prolonged-release tablets",
                        "Solution for injection",
                        "Powder for oral suspension"
                    )
                    : null)
            
            // Navigation properties
            .RuleFor(x => x.Revision, _ => null)
            .RuleFor(x => x.BnfChapter, _ => null)
            .RuleFor(x => x.FormulationType, _ => null)
            .RuleFor(x => x.TherapeuticAreas, _ => [])
            .RuleFor(x => x.ActiveSubstances, _ => [new MedicinesActiveSubstance{Name="TestName", NameType=SubstanceNameType.DevelopmentName}])
            .RuleFor(x => x.RecordStatuses, _ => []);
    }
}