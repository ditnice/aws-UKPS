using Bogus;
using UKPS.Api.Persistence.Entities.RecordWorkflow;
using UKPS.Api.Persistence.Enums;

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
    }
}
