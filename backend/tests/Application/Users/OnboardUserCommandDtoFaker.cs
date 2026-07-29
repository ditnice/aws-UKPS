using Bogus;
using UKPS.Api.Application.Users.Dtos;

namespace UKPS.Api.Tests.Application.Users;

internal sealed class OnboardUserCommandDtoFaker : Faker<OnboardUserCommandDto>
{
    public OnboardUserCommandDtoFaker()
    {
        UseSeed(12);
        RuleFor(x => x.NewUserEmail, f => f.Internet.Email());
    }
}
