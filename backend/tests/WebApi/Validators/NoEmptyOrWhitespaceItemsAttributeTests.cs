using System.ComponentModel.DataAnnotations;
using Shouldly;
using UKPS.Api.WebApi.Validators;

namespace UKPS.Api.Tests.WebApi.Validators;

public class NoEmptyOrWhitespaceItemsAttributeTests
{
    private readonly NoEmptyOrWhitespaceItemsAttribute _attribute = new();

    public static TheoryData<string?[]> InvalidCollections =>
        new([""], ["   "], ["\n\t"], [null], ["valid", ""], ["valid", " "]);

    [Fact]
    public void IsValid_WhenCollectionIsNull_ReturnsSuccess()
    {
        Validate(null).ShouldBe(ValidationResult.Success);
    }

    [Fact]
    public void IsValid_WhenCollectionIsEmpty_ReturnsSuccess()
    {
        Validate(Array.Empty<string>()).ShouldBe(ValidationResult.Success);
    }

    [Fact]
    public void IsValid_WhenAllItemsHaveValues_ReturnsSuccess()
    {
        string[] items = ["first", "second"];

        Validate(items).ShouldBe(ValidationResult.Success);
    }

    [Theory]
    [MemberData(nameof(InvalidCollections))]
    public void IsValid_WhenAnyItemIsNullEmptyOrWhitespace_ReturnsError(string?[] items)
    {
        ValidationResult? result = Validate(items);

        result.ShouldNotBeNull();
        result.MemberNames.ShouldBe(["Names"]);
        result.ErrorMessage.ShouldBe("Names cannot contain empty or whitespace values.");
    }

    [Fact]
    public void IsValid_WhenCustomErrorMessageProvided_UsesCustomErrorMessage()
    {
        var attribute = new NoEmptyOrWhitespaceItemsAttribute("Custom message.");
        string[] items = [""];

        ValidationResult? result = attribute.GetValidationResult(items, CreateValidationContext());

        result.ShouldNotBeNull();
        result.ErrorMessage.ShouldBe("Custom message.");
    }

    [Fact]
    public void IsValid_WhenValueIsNotAStringCollection_Throws()
    {
        int[] items = [1, 2];

        Should.Throw<InvalidOperationException>(() => Validate(items));
    }

    private ValidationResult? Validate(object? value) =>
        _attribute.GetValidationResult(value, CreateValidationContext());

    private static ValidationContext CreateValidationContext() =>
        new(new object()) { MemberName = "Names", DisplayName = "Names" };
}
