using Bogus;
using UKPS.Api.Persistence.Entities.Identity;

namespace UKPS.Api.Persistence.Data.Fakers;

/// <summary>
/// Provides a faker for generating bogus <see cref="UserRegistrationRequest"/> instances for testing.
/// </summary>
internal sealed class UserRegistrationRequestFaker : Faker<UserRegistrationRequest>
{
    /// <summary>
    /// Initialises a new instance of the <see cref="UserRegistrationRequestFaker"/> class
    /// with rules for generating test data for a <see cref="UserRegistrationRequest"/>.
    /// </summary>
    public UserRegistrationRequestFaker()
    {
        RuleFor(x => x.Organisation, f => new OrganisationFaker().Generate());
        RuleFor(x => x.OrganisationId, (f, x) => x.Organisation!.Id);
        RuleFor(x => x.FullName, f => f.Name.FullName());
        RuleFor(x => x.WorkEmail, (f, u) => f.Internet.Email(u.FullName));
        RuleFor(x => x.PhoneNumber, _ => new TelephoneNumberFaker().Generate());

        RuleFor(x => x.RejectedBy, f => null);
        RuleFor(x => x.RejectedAt, f => null);
        RuleFor(x => x.RejectedByUser, f => null);
    }
}
