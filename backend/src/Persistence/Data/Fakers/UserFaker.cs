using Bogus;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Data.Fakers;

internal sealed class UserFaker : Faker<User>
{
    public UserFaker()
    {
        RuleFor(x => x.Id, f => f.IndexFaker + 1);
        RuleFor(x => x.IdentityId, f => f.Random.Guid().ToString());
        RuleFor(x => x.FullName, f => f.Name.FullName());
        RuleFor(x => x.UserType, f => f.PickRandom<UserType>());
        RuleFor(x => x.Title, f => f.PickRandom("Mr", "Mrs", "Ms", "Miss", "Dr", "Prof"));
        RuleFor(x => x.JobTitle, f => f.Name.JobTitle());
        RuleFor(x => x.WorkTelephone, f => f.Phone.PhoneNumber());
        RuleFor(x => x.WorkEmail, (f, u) => f.Internet.Email(u.FullName));
        RuleFor(x => x.CreatedAt, f => f.Date.Past(5).ToUniversalTime());
        RuleFor(
            x => x.UpdatedAt,
            (f, u) =>
                f.Random.Bool(0.5f)
                    ? f.Date.Between(u.CreatedAt, DateTime.UtcNow).ToUniversalTime()
                    : null
        );
        RuleFor(
            x => x.LastActive,
            (f, u) =>
                f.Random.Bool(0.8f)
                    ? f.Date.Between(u.CreatedAt, DateTime.UtcNow).ToUniversalTime()
                    : null
        );
    }
}
