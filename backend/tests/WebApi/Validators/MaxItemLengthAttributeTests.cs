using System.ComponentModel.DataAnnotations;
using Shouldly;
using UKPS.Api.WebApi.Validators;

namespace UKPS.Api.Tests.WebApi.Validators;

public class MaxItemLengthAttributeTests
{
    private readonly MaxItemLengthAttribute _attribute = new(3);

    public static TheoryData<string?[]?> ValidCollections =>
        new(null, [], [null], ["abc"], ["  abc  "]);

    [Theory]
    [MemberData(nameof(ValidCollections))]
    public void IsValid_WhenEveryItemIsWithinTheLimit_ReturnsSuccess(string?[]? items)
    {
        Validate(items).ShouldBe(ValidationResult.Success);
    }

    [Fact]
    public void IsValid_WhenAnyItemIsTooLong_ReturnsError()
    {
        string[] items = ["abc", "abcd"];

        ValidationResult? result = Validate(items);

        result.ShouldNotBeNull();
        result.MemberNames.ShouldBe(["Names"]);
        result.ErrorMessage.ShouldBe("Names cannot contain values longer than 3 characters.");
    }

    [Fact]
    public void IsValid_WhenValueIsNotAStringCollection_Throws()
    {
        int[] items = [1, 2];

        Should.Throw<InvalidOperationException>(() => Validate(items));
    }

    private ValidationResult? Validate(object? value) =>
        _attribute.GetValidationResult(
            value,
            new ValidationContext(new object()) { MemberName = "Names", DisplayName = "Names" }
        );
}
