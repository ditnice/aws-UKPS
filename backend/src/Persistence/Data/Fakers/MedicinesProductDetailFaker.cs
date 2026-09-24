using System.Globalization;
using Bogus;
using UKPS.Api.Persistence.Entities.MedicinesRevisionContent;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Data.Fakers;

internal sealed class MedicinesProductDetailFaker : Faker<MedicinesProductDetail>
{
    private readonly string[] _presentationOptions =
    [
        "Tablets",
        "Capsules",
        "Oral solution",
        "Film-coated tablets",
        "Prolonged-release tablets",
        "Solution for injection",
        "Powder for oral suspension",
    ];

    public MedicinesProductDetailFaker()
    {
        RuleFor(x => x.RecordTitle, f => f.Lorem.Sentence(1).TrimEnd('.'));
        RuleFor(x => x.BrandedName, f => f.Random.Bool(0.7f) ? f.Commerce.ProductName() : null);
        RuleFor(x => x.Indication, f => f.Lorem.Sentence(8).TrimEnd('.'));
        RuleFor(
            x => x.IndicationIsPaediatric,
            f => f.Random.Bool(0.8f) ? f.PickRandom<IndicationPaediatricStatus>() : null
        );
        RuleFor(
            x => x.IndicationIsCancer,
            f => f.Random.Bool(0.8f) ? f.PickRandom<YesNoUnknown>() : null
        );
        RuleFor(
            x => x.IndicationIsRareDisease,
            f => f.Random.Bool(0.8f) ? f.PickRandom<YesNoUnknown>() : null
        );
        RuleFor(
            x => x.NiceTaDevelopmentId,
            f =>
            {
                if (!f.Random.Bool(0.5f))
                {
                    return null;
                }
                var prefix = f.Random.Bool() ? "GID-TA" : "GID-HST";
                var suffix = f.Random.Int(1000, 9999).ToString(CultureInfo.InvariantCulture);
                return string.Concat(prefix, suffix);
            }
        );
        RuleFor(
            x => x.Presentation,
            f => f.Random.Bool(0.8f) ? f.PickRandom(_presentationOptions) : null
        );

        RuleFor(
            x => x.ActiveSubstances,
            f =>
                [
                    new MedicinesActiveSubstance
                    {
                        Name = f.Lorem.Word(),
                        NameType = SubstanceNameType.DevelopmentName,
                    },
                ]
        );
    }
}
