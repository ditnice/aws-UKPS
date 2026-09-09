using Bogus;

namespace UKPS.Api.Persistence.Data.Fakers;

internal sealed class TelephoneNumberFaker
{
    private static readonly string[] _validUkTelephoneNumbers =
    [
        "020 1234 5678", // UK landline, no country code.
        "07911 123456", // UK mobile, no country code.
        "+44 121 234 5678", // UK, with country code.
        "(020) 1234 5678", // UK, parenthesised area code.
        "020-1234-5678", // UK, hyphenated.
        "020 1234 5678 ext 123", // UK, with extension.
    ];

    private static readonly string[] _validForeignTelephoneNumbers =
    [
        "+1 (212) 555-0123", // USA.
        "+33 1 42 68 53 00", // France.
        "+49 30 83050", // Germany.
        "+34 91 123 45 67", // Spain.
        "+39 02 3661 8300", // Italy.
        "+31 20 794 0100", // Netherlands.
        "+353 1 234 5678", // Ireland.
        "+351 21 123 4567", // Portugal.
        "+32 470 12 34 56", // Belgium.
        "+46 8 123 456 00", // Sweden.
        "+48 22 123 45 67", // Poland.
        "+45 32 12 34 56", // Denmark.
        "+358 9 123 4567", // Finland.
        "+41 44 668 18 00", // Switzerland.
        "+43 1 234 5678", // Austria.
        "+47 22 12 34 56", // Norway.
    ];

    private readonly Faker _faker = new Faker();

    public string Generate()
    {
        return _faker.Random.Bool(0.7f)
            ? _faker.PickRandom(_validUkTelephoneNumbers)
            : _faker.PickRandom(_validForeignTelephoneNumbers);
    }
}
