using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace UKPS.Api.WebApi.Validators;

/// <summary>
/// Validates that every item in a string collection is at most a number of characters, ignoring
/// leading and trailing whitespace.
/// </summary>
/// <remarks>
/// A null collection or item is treated as valid; combine with <see cref="RequiredAttribute"/> or
/// <see cref="NoEmptyOrWhitespaceItemsAttribute"/> to require values.
/// </remarks>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
internal sealed class MaxItemLengthAttribute(int length)
    : ValidationAttribute("{0} cannot contain values longer than {1} characters.")
{
    public int Length { get; } = length;

    public override string FormatErrorMessage(string name) =>
        string.Format(CultureInfo.CurrentCulture, ErrorMessageString, name, Length);

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return ValidationResult.Success!;
        }

        if (value is not IEnumerable<string?> items)
        {
            throw new InvalidOperationException(
                $"{nameof(MaxItemLengthAttribute)} can only be applied to string collections."
            );
        }

        if (items.All(item => item is null || item.Trim().Length <= Length))
        {
            return ValidationResult.Success!;
        }

        return new ValidationResult(
            FormatErrorMessage(validationContext.DisplayName),
            validationContext.MemberName is null ? null : [validationContext.MemberName]
        );
    }
}
