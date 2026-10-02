using System.ComponentModel.DataAnnotations;
using Shouldly;
using UKPS.Api.WebApi.Validators;

namespace UKPS.Api.Tests.WebApi.Validators;

public class NotEmptyOrWhitespaceAttributeTests
{
    private readonly NotEmptyOrWhitespaceAttribute _attribute = new();

    [Fact]
    public void IsValid_WhenValueIsNull_ReturnsSuccess()
    {
        Validate(null).ShouldBe(ValidationResult.Success);
    }

    [Theory]
    [InlineData("value")]
    [InlineData(" value ")]
    public void IsValid_WhenValueHasContent_ReturnsSuccess(string value)
    {
        Validate(value).ShouldBe(ValidationResult.Success);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\n\t")]
    public void IsValid_WhenValueIsEmptyOrWhitespace_ReturnsError(string value)
    {
        ValidationResult? result = Validate(value);

        result.ShouldNotBeNull();
        result.MemberNames.ShouldBe(["Name"]);
        result.ErrorMessage.ShouldBe("Name cannot be empty or whitespace.");
    }

    [Fact]
    public void IsValid_WhenCustomErrorMessageProvided_UsesCustomErrorMessage()
    {
        var attribute = new NotEmptyOrWhitespaceAttribute("Custom message.");

        ValidationResult? result = attribute.GetValidationResult("", CreateValidationContext());

        result.ShouldNotBeNull();
        result.ErrorMessage.ShouldBe("Custom message.");
    }

    [Fact]
    public void IsValid_WhenValueIsNotAString_Throws()
    {
        Should.Throw<InvalidOperationException>(() => Validate(1));
    }

    private ValidationResult? Validate(object? value) =>
        _attribute.GetValidationResult(value, CreateValidationContext());

    private static ValidationContext CreateValidationContext() =>
        new(new object()) { MemberName = "Name", DisplayName = "Name" };
}
