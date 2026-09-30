using System.ComponentModel.DataAnnotations;

namespace UKPS.Api.WebApi.Validators;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
internal sealed class DistinctStringsAttribute : ValidationAttribute
{
    public StringComparison StringComparison { get; }

    public DistinctStringsAttribute(StringComparison stringComparison)
    {
        StringComparison = stringComparison;
    }

    /// <inheritdoc />
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not IEnumerable<string> values)
        {
            return ValidationResult.Success;
        }

        var comparer = GetComparer(StringComparison);
        return values.Select(x => x.Trim()).Distinct(comparer).Count() == values.Count()
            ? ValidationResult.Success
            : new ValidationResult(
                $"{validationContext.DisplayName} must contain distinct values."
            );
    }

    private static StringComparer GetComparer(StringComparison stringComparison)
    {
        return stringComparison switch
        {
            StringComparison.Ordinal => StringComparer.Ordinal,
            StringComparison.OrdinalIgnoreCase => StringComparer.OrdinalIgnoreCase,
            StringComparison.InvariantCulture => StringComparer.InvariantCulture,
            StringComparison.InvariantCultureIgnoreCase =>
                StringComparer.InvariantCultureIgnoreCase,
            StringComparison.CurrentCulture => StringComparer.CurrentCulture,
            StringComparison.CurrentCultureIgnoreCase => StringComparer.CurrentCultureIgnoreCase,
            _ => throw new ArgumentOutOfRangeException(nameof(stringComparison)),
        };
    }
}
