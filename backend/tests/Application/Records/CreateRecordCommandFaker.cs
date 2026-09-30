using Bogus;
using UKPS.Api.Application.Records.Dtos;

namespace UKPS.Api.Tests.Application.Records;

internal sealed class CreateRecordCommandFaker : Faker<CreateRecordCommand>
{
    public CreateRecordCommandFaker()
    {
        RuleFor(x => x.OrganisationId, f => f.Random.Int(1, 1000));
        RuleFor(
            x => x.DevelopmentNames,
            f =>
                Enumerable
                    .Range(0, f.Random.Int(1, 3))
                    .Select(_ => f.Commerce.ProductName())
                    .ToArray()
        );
        RuleFor(x => x.BrandedName, f => f.Commerce.ProductName());
        RuleFor(
            x => x.GenericNames,
            f =>
                Enumerable
                    .Range(0, f.Random.Int(1, 3))
                    .Select(_ => f.Commerce.ProductName())
                    .ToArray()
        );
        RuleFor(x => x.RecordTitle, f => f.Lorem.Sentence());
    }
}
