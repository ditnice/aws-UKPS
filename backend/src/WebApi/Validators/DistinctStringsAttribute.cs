using System.ComponentModel.DataAnnotations;

namespace UKPS.Api.WebApi.Validators;

/// <summary>
/// Validates that every item in a string collection is distinct.
/// </summary>
/// <remarks>
/// Items are trimmed before being compared, so values differing only by leading or trailing
/// whitespace are treated as duplicates. A null collection is treated as valid; combine with
/// <see cref="RequiredAttribute"/> to require a value.
/// </remarks>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
internal sealed class DistinctStringsAttribute : ValidationAttribute
{
    private const string DefaultErrorMessage = "{0} must contain distinct values.";

    private readonly StringComparer _comparer;

    public DistinctStringsAttribute(StringComparison stringComparison)
        : base(DefaultErrorMessage)
    {
        StringComparison = stringComparison;
        _comparer = StringComparer.FromComparison(stringComparison);
    }

    public DistinctStringsAttribute(StringComparison stringComparison, string errorMessage)
        : base(errorMessage: errorMessage)
    {
        StringComparison = stringComparison;
        _comparer = StringComparer.FromComparison(stringComparison);
    }

    /// <summary>
    /// Gets the comparison used to determine whether two items are equal.
    /// </summary>
    public StringComparison StringComparison { get; }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return ValidationResult.Success!;
        }

        if (value is not IEnumerable<string?> items)
        {
            throw new InvalidOperationException(
                $"{nameof(DistinctStringsAttribute)} can only be applied to string collections."
            );
        }

        var seen = new HashSet<string?>(_comparer);
        if (items.All(item => seen.Add(item?.Trim())))
        {
            return ValidationResult.Success!;
        }

        return new ValidationResult(
            FormatErrorMessage(validationContext.DisplayName),
            validationContext.MemberName is null ? null : [validationContext.MemberName]
        );
    }
}
