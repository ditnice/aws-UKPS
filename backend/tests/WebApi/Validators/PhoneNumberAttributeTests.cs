using System.ComponentModel.DataAnnotations;
using Shouldly;
using UKPS.Api.WebApi.Validators;

namespace UKPS.Api.Tests.WebApi.Validators;

public class PhoneNumberAttributeTests
{
    private readonly PhoneNumberAttribute _attribute = new();

    [Fact]
    public void IsValid_WhenValueIsAValidPhoneNumber_ReturnsSuccess()
    {
        Validate("020 1234 5678").ShouldBe(ValidationResult.Success);
    }

    [Theory]
    [InlineData("not a phone number")]
    [InlineData("")]
    [InlineData(null)]
    public void IsValid_WhenValueIsNotAValidPhoneNumber_ReturnsDefaultError(string? value)
    {
        ValidationResult? result = Validate(value);

        result.ShouldNotBeNull();
        result.MemberNames.ShouldBe(["Telephone"]);
        result.ErrorMessage.ShouldBe("Value must be a valid phone number.");
    }

    [Fact]
    public void IsValid_WhenCustomErrorMessageProvided_UsesCustomErrorMessage()
    {
        var attribute = new PhoneNumberAttribute("Custom message.");

        ValidationResult? result = attribute.GetValidationResult(
            "not a phone number",
            CreateValidationContext()
        );

        result.ShouldNotBeNull();
        result.ErrorMessage.ShouldBe("Custom message.");
    }

    private ValidationResult? Validate(object? value) =>
        _attribute.GetValidationResult(value, CreateValidationContext());

    private static ValidationContext CreateValidationContext() =>
        new(new object()) { MemberName = "Telephone", DisplayName = "Telephone" };
}
