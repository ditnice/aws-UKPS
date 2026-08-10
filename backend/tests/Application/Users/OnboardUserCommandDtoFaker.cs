using Bogus;
using UKPS.Api.Application.Users.Dtos;

namespace UKPS.Api.Tests.Application.Users;

public sealed class OnboardUserCommandDtoFaker : Faker<OnboardUserCommandDto>
{
    public OnboardUserCommandDtoFaker()
    {
        RuleFor(x => x.FullName, f => f.Name.FullName());
        RuleFor(x => x.ContactNumber, f => f.Phone.PhoneNumber());
        RuleFor(x => x.NewUserEmail, f => f.Internet.Email());
    }
}
