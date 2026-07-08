using Bogus;
using UKPS.Api.Entities.Identity;
using UKPS.Api.Enums;

namespace UKPS.Api.Tests.Fixtures;

internal sealed class UserFaker : Faker<User>
{
    public UserFaker()
    {
        RuleFor(x => x.Id, f => f.IndexFaker + 1);
        RuleFor(x => x.FirstName, f => f.Name.FirstName());
        RuleFor(x => x.LastName, f => f.Name.LastName());
        RuleFor(x => x.Username, (f, u) => f.Internet.UserName(u.FirstName, u.LastName));
        RuleFor(x => x.UserType, f => f.PickRandom<UserType>());
        RuleFor(x => x.Title, f => f.PickRandom("Mr", "Mrs", "Ms", "Miss", "Dr", "Prof"));
        RuleFor(x => x.JobTitle, f => f.Name.JobTitle());
        RuleFor(x => x.WorkTelephone, f => f.Phone.PhoneNumber());
        RuleFor(x => x.WorkEmail, (f, u) => f.Internet.Email(u.FirstName, u.LastName));
    }
}
