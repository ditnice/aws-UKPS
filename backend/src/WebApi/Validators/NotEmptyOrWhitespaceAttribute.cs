using System.ComponentModel.DataAnnotations;

namespace UKPS.Api.WebApi.Validators;

/// <summary>
/// Validates that a string value is not empty or whitespace.
/// </summary>
/// <remarks>
/// A null value is treated as valid, which makes this suitable for optional fields; combine
/// with <see cref="RequiredAttribute"/> to require a value.
/// </remarks>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
internal sealed class NotEmptyOrWhitespaceAttribute : ValidationAttribute
{
    private const string DefaultErrorMessage = "{0} cannot be empty or whitespace.";

    public NotEmptyOrWhitespaceAttribute()
        : base(DefaultErrorMessage) { }

    public NotEmptyOrWhitespaceAttribute(string errorMessage)
        : base(errorMessage: errorMessage) { }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return ValidationResult.Success!;
        }

        if (value is not string str)
        {
            throw new InvalidOperationException(
                $"{nameof(NotEmptyOrWhitespaceAttribute)} can only be applied to string properties."
            );
        }

        if (!string.IsNullOrWhiteSpace(str))
        {
            return ValidationResult.Success!;
        }

        return new ValidationResult(
            FormatErrorMessage(validationContext.DisplayName),
            validationContext.MemberName is null ? null : [validationContext.MemberName]
        );
    }
}
