using Bogus;
using UKPS.Api.Persistence.Entities.SharedRevisionContent;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Data.Fakers;

internal sealed class RecordProductDetailFaker : Faker<RecordProductDetail>
{
    public RecordProductDetailFaker(RecordType recordType, int? revisionId = null)
    {
        RuleFor(x => x.RevisionId, f => revisionId ?? f.Random.Int(1, 1000));

        if (recordType == RecordType.Vaccine)
        {
            RuleFor(
                x => x.CompanyCode,
                f => f.PickRandom("mRNA-1273", "BNT162b2", "V116", "AZD1222", "NVX-CoV2373")
            );
            RuleFor(
                x => x.BrandedName,
                f =>
                    f.Random.Bool()
                        ? f.PickRandom("Spikevax", "Comirnaty", "Nuvaxovid", "Arexvy")
                        : null
            );
            RuleFor(
                x => x.RecordTitle,
                f =>
                    $"{f.PickRandom("RSV", "COVID-19", "Influenza", "Pneumococcal")}"
                    + $" — {f.PickRandom("adults 60+", "adults 18+", "children", "at-risk adults")}"
            );
        }
        else
        {
            RuleFor(x => x.CompanyCode, f => f.Lorem.Word());
            RuleFor(x => x.BrandedName, f => f.Random.Bool(0.7f) ? f.Commerce.ProductName() : null);
            RuleFor(x => x.RecordTitle, f => f.Lorem.Sentence(1).TrimEnd('.'));
        }
    }
}
