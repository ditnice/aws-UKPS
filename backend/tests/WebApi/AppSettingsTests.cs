using System.Reflection;
using System.Text.Json;
using Shouldly;
using UKPS.Api.Application.Authentication;
using UKPS.Api.Application.InternalServices.Communication;
using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Data.Seeding;
using UKPS.Api.WebApi.InternalServices.Authentication;

namespace UKPS.Api.Tests.WebApi;

public sealed class AppSettingsTests
{
    [Theory]
    [InlineData("appsettings.json")]
    [InlineData("appsettings.Example.json")]
    public void AppSettings_ShouldContainEveryWritableOption_WithoutUnsupportedKeys(string filename)
    {
        using var document = JsonDocument.Parse(
            File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Configuration", filename))
        );

        (string Section, Type OptionsType)[] sections =
        [
            (CognitoOptions.SectionName, typeof(CognitoOptions)),
            (DatabaseOptions.SectionName, typeof(DatabaseOptions)),
            (DevAuthenticationOptions.SectionName, typeof(DevAuthenticationOptions)),
            (EmailOptions.SectionName, typeof(EmailOptions)),
            (SeedingOptions.SectionName, typeof(SeedingOptions)),
            (UserOnboardingOptions.SectionName, typeof(UserOnboardingOptions)),
        ];

        foreach (var (section, optionsType) in sections)
        {
            var settings = document
                .RootElement.EnumerateObject()
                .Single(property =>
                    string.Equals(property.Name, section, StringComparison.OrdinalIgnoreCase)
                );
            var actualKeys = settings.Value.EnumerateObject().Select(property => property.Name);
            var expectedKeys = optionsType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(property => property.SetMethod is not null)
                .Select(property => property.Name);

            actualKeys
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray()
                .ShouldBe(expectedKeys.OrderBy(name => name, StringComparer.Ordinal).ToArray());
        }
    }
}
