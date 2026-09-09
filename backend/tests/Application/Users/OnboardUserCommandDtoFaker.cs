using Bogus;
using UKPS.Api.Application.Users.Dtos;
using UKPS.Api.Persistence.Data.Fakers;

namespace UKPS.Api.Tests.Application.Users;

public sealed class OnboardUserCommandDtoFaker : Faker<OnboardUserCommandDto>
{
    public OnboardUserCommandDtoFaker()
    {
        RuleFor(x => x.FullName, f => f.Name.FullName());
        RuleFor(x => x.ContactNumber, _ => new TelephoneNumberFaker().Generate());
        RuleFor(x => x.NewUserEmail, f => f.Internet.Email());
    }
}
