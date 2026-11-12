using System.Diagnostics;
using Bogus;
using UKPS.Api.Application.Users.Dtos;
using UKPS.Api.Persistence.Data.Fakers;

namespace UKPS.Api.Tests.Application.Users;

public sealed class UpdateUserDetailsCommandFaker : Faker<UpdateUserDetailsCommand>
{
    public UpdateUserDetailsCommandFaker()
    {
        RuleFor(x => x.Property, f => f.PickRandom<EditableUserDetail>());
        RuleFor(
            x => x.Value,
            (f, o) =>
                o.Property switch
                {
                    EditableUserDetail.FullName => f.Name.FullName(),
                    EditableUserDetail.WorkPhone => new TelephoneNumberFaker().Generate(),
                    _ => throw new UnreachableException(),
                }
        );
    }
}
