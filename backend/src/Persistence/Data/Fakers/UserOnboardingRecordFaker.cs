using Bogus;
using UKPS.Api.Persistence.Entities.Identity;

namespace UKPS.Api.Persistence.Data.Fakers;

internal sealed class UserOnboardingRecordFaker : Faker<UserOnboardingRecord>
{
    public UserOnboardingRecordFaker()
    {
        UseSeed(15); // Random seed

        RuleFor(x => x.SetupToken, f => Guid.NewGuid());
        RuleFor(x => x.UserEmail, f => f.Internet.Email());
        RuleFor(x => x.CreatedAt, f => f.Date.Recent());
        RuleFor(x => x.CreatedBy, f => f.Internet.Email());
    }
}
