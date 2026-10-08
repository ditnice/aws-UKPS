using System.ComponentModel.DataAnnotations;

namespace UKPS.Api.WebApi.Validators;

/// <summary>
/// Validates that every item in a string collection is not null, empty or whitespace.
/// </summary>
/// <remarks>
/// A null collection is treated as valid; combine with <see cref="RequiredAttribute"/> to
/// require a value.
/// </remarks>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
internal sealed class NoEmptyOrWhitespaceItemsAttribute : ValidationAttribute
{
    private const string DefaultErrorMessage = "{0} cannot contain empty or whitespace values.";

    public NoEmptyOrWhitespaceItemsAttribute()
        : base(DefaultErrorMessage) { }

    public NoEmptyOrWhitespaceItemsAttribute(string errorMessage)
        : base(errorMessage: errorMessage) { }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return ValidationResult.Success!;
        }

        if (value is not IEnumerable<string?> items)
        {
            throw new InvalidOperationException(
                $"{nameof(NoEmptyOrWhitespaceItemsAttribute)} can only be applied to string collections."
            );
        }

        if (!items.Any(string.IsNullOrWhiteSpace))
        {
            return ValidationResult.Success!;
        }

        return new ValidationResult(
            FormatErrorMessage(validationContext.DisplayName),
            validationContext.MemberName is null ? null : [validationContext.MemberName]
        );
    }
}
