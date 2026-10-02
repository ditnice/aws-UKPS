using Bogus;
using UKPS.Api.Persistence.Entities.VaccinesRevisionContent;

namespace UKPS.Api.Persistence.Data.Fakers;

internal sealed class VaccinesProductDetailFaker : Faker<VaccinesProductDetail>
{
    public VaccinesProductDetailFaker(int? revisionId = null)
    {
        RuleFor(x => x.RevisionId, f => revisionId ?? f.Random.Int(1, 1000));
        RuleFor(
            x => x.RecordTitle,
            f =>
                $"{f.PickRandom("RSV", "COVID-19", "Influenza", "Pneumococcal")}"
                + $" — {f.PickRandom("adults 60+", "adults 18+", "children", "at-risk adults")}"
        );
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
    }
}
