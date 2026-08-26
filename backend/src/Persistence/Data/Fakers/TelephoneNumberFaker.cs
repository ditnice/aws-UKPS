using Bogus;

namespace UKPS.Api.Persistence.Data.Fakers;

internal sealed class TelephoneNumberFaker
{
    private static readonly string[] _validTelephoneNumbers =
    [
        "020 1234 5678", // UK landline, no country code.
        "07911 123456", // UK mobile, no country code.
        "+44 121 234 5678", // UK, with country code.
        "+33 1 42 68 53 00", // France, with country code.
        "+1 (212) 555-0123", // USA, with country code.
    ];

    private readonly Faker _faker = new Faker();

    public string Generate()
    {
        return _faker.PickRandom(_validTelephoneNumbers);
    }
}
