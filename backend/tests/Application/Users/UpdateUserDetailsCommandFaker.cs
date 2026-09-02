using Bogus;
using UKPS.Api.Application.Users.Dtos;
using UKPS.Api.Persistence.Data.Fakers;

namespace UKPS.Api.Tests.Application.Users;

public sealed class UpdateUserDetailsCommandFaker : Faker<UpdateUserDetailsCommand>
{
    public UpdateUserDetailsCommandFaker()
    {
        RuleFor(x => x.FullName, f => f.Name.FullName());
        RuleFor(x => x.WorkEmail, f => f.Internet.Email());
        RuleFor(x => x.WorkTelephone, _ => new TelephoneNumberFaker().Generate());
    }
}
