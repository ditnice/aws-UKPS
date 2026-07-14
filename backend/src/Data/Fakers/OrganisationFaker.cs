using Bogus;
using UKPS.Api.Entities.Identity;
using UKPS.Api.Enums;

namespace UKPS.Api.Data.Fakers;

internal sealed class OrganisationFaker : Faker<Organisation>
{
    public OrganisationFaker()
    {
        RuleFor(x => x.Id, f => f.IndexFaker + 1);
        RuleFor(x => x.OrganisationName, f => f.Company.CompanyName());
        RuleFor(x => x.OrganisationType, f => f.PickRandom<OrganisationType>());
        RuleFor(x => x.AllowedPharmaceuticalEntity, f => f.PickRandom<PharmaceuticalEntity>());
        RuleFor(x => x.CountryOrRegion, f => f.Address.Country());
        RuleFor(x => x.HeadOfficeAddress, f => f.Address.FullAddress());
        RuleFor(x => x.HeadOfficeTelephone, f => f.Phone.PhoneNumber());
        RuleFor(
            x => x.HeadOfficeEmail,
            (f, o) =>
                f.Internet.Email(
                    o.OrganisationName.Replace(" ", ".", StringComparison.CurrentCulture)
                )
        );
        RuleFor(x => x.Status, f => f.PickRandom<UserOrgStatus>());
        RuleFor(x => x.CreatedAt, f => f.Date.Past(5).ToUniversalTime());
        RuleFor(
            x => x.LastActive,
            (f, o) =>
                f.Random.Bool(0.8f)
                    ? f
                        .Date.Between(o.CreatedAt ?? DateTime.UtcNow.AddYears(-1), DateTime.UtcNow)
                        .ToUniversalTime()
                    : null
        );
    }
}
