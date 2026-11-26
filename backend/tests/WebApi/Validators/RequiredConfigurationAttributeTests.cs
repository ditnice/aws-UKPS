using System.ComponentModel.DataAnnotations;
using Shouldly;
using UKPS.Api.WebApi.Validators;

namespace UKPS.Api.Tests.WebApi.Validators;

public class RequiredConfigurationAttributeTests
{
    private readonly RequiredConfigurationAttribute _attribute = new();

    [Theory]
    [InlineData("value")]
    [InlineData("https://example.com")]
    [InlineData("FROM_ENVIRONMENT_VARIABLE_SUFFIX")]
    public void IsValid_WhenValueIsReal_ReturnsSuccess(string value)
    {
        Validate(value).ShouldBe(ValidationResult.Success);
    }

    [Fact]
    public void IsValid_WhenValueIsNonStringObject_ReturnsSuccess()
    {
        Validate(42).ShouldBe(ValidationResult.Success);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsValid_WhenValueIsMissing_ReturnsRequiredError(string? value)
    {
        ValidationResult? result = Validate(value);

        result.ShouldNotBeNull();
        result.MemberNames.ShouldBe(["Name"]);
        result.ErrorMessage.ShouldBe("The Name field is required.");
    }

    [Theory]
    [InlineData("FROM_ENVIRONMENT_VARIABLE", "FROM_ENVIRONMENT_VARIABLE")]
    [InlineData("FROM_AWS_SECRETS", "FROM_AWS_SECRETS")]
    [InlineData(" from_aws_secrets ", "FROM_AWS_SECRETS")]
    public void IsValid_WhenValueIsPlaceholder_ReturnsPlaceholderError(
        string value,
        string expectedPlaceholder
    )
    {
        ValidationResult? result = Validate(value);

        result.ShouldNotBeNull();
        result.MemberNames.ShouldBe(["Name"]);
        result.ErrorMessage.ShouldBe(
            $"Name is set to the placeholder value '{expectedPlaceholder}'. Provide a real value in configuration."
        );
    }

    [Fact]
    public void IsValid_WhenUriIsPlaceholder_ReturnsPlaceholderError()
    {
        var uri = new Uri("FROM_ENVIRONMENT_VARIABLE", UriKind.Relative);

        Validate(uri).ShouldNotBeNull();
    }

    private ValidationResult? Validate(object? value) =>
        _attribute.GetValidationResult(value, CreateValidationContext());

    private static ValidationContext CreateValidationContext() =>
        new(new object()) { MemberName = "Name", DisplayName = "Name" };
}
