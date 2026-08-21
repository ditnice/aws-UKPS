using Bogus;
using UKPS.Api.Application.Users.Dtos;

namespace UKPS.Api.Tests.Application.Users;

public sealed class UpdateUserDetailsCommandFaker : Faker<UpdateUserDetailsCommand>
{
    public UpdateUserDetailsCommandFaker()
    {
        RuleFor(x => x.FullName, f => f.Name.FullName());
        RuleFor(x => x.WorkEmail, f => f.Internet.Email());
        RuleFor(x => x.WorkTelephone, f => f.Phone.PhoneNumber());
    }
}
