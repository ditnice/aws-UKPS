using System.ComponentModel.DataAnnotations;
using Shouldly;
using UKPS.Api.WebApi.Validators;

namespace UKPS.Api.Tests.WebApi.Validators;

public class DistinctStringsAttributeTests
{
    private readonly DistinctStringsAttribute _attribute = new(StringComparison.OrdinalIgnoreCase);

    public static TheoryData<string?[]> DuplicateCollections =>
        new(
            ["first", "first"],
            ["first", "FIRST"],
            ["first", " first "],
            ["first", "second", "First"],
            [null, null]
        );

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
    public void IsValid_WhenAllItemsAreDistinct_ReturnsSuccess()
    {
        string[] items = ["first", "second"];

        Validate(items).ShouldBe(ValidationResult.Success);
    }

    [Theory]
    [MemberData(nameof(DuplicateCollections))]
    public void IsValid_WhenItemsAreDuplicated_ReturnsError(string?[] items)
    {
        ValidationResult? result = Validate(items);

        result.ShouldNotBeNull();
        result.MemberNames.ShouldBe(["Names"]);
        result.ErrorMessage.ShouldBe("Names must contain distinct values.");
    }

    [Fact]
    public void IsValid_WhenComparisonIsCaseSensitive_TreatsDifferentCasesAsDistinct()
    {
        var attribute = new DistinctStringsAttribute(StringComparison.Ordinal);
        string[] items = ["first", "FIRST"];

        attribute
            .GetValidationResult(items, CreateValidationContext())
            .ShouldBe(ValidationResult.Success);
    }

    [Fact]
    public void IsValid_WhenCustomErrorMessageProvided_UsesCustomErrorMessage()
    {
        var attribute = new DistinctStringsAttribute(
            StringComparison.OrdinalIgnoreCase,
            "Custom message."
        );
        string[] items = ["first", "first"];

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
