using Bogus;
using UKPS.Api.Persistence.Entities.Identity;

namespace UKPS.Api.Persistence.Data.Fakers;

internal sealed class UserOnboardingRecordFaker : Faker<UserOnboardingRecord>
{
    public UserOnboardingRecordFaker()
    {
        UseSeed(15); // Random seed

        RuleFor(x => x.SetupToken, f => f.Random.Guid());
        RuleFor(x => x.CreatedAt, f => f.Date.RecentOffset(30).UtcDateTime);
        RuleFor(x => x.CreatedBy, f => f.Internet.UserName());
    }
}
