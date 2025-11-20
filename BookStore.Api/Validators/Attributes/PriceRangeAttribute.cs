using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace BookStore.Api.Validators.Attributes;

/// <summary>
/// Validates that a price value falls within a configured inclusive range.
/// </summary>
public sealed class PriceRangeAttribute : ValidationAttribute
{
    private readonly decimal _min;
    private readonly decimal _max;

    public PriceRangeAttribute(double min, double max)
    {
        if (max < min)
        {
            throw new ArgumentException("Maximum price must be greater than or equal to minimum price.", nameof(max));
        }

        _min = Convert.ToDecimal(min, CultureInfo.InvariantCulture);
        _max = Convert.ToDecimal(max, CultureInfo.InvariantCulture);

        ErrorMessage = "{0} must be between " + FormatCurrency(_min) + " and " + FormatCurrency(_max) + ".";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return ValidationResult.Success;
        }

        var price = ConvertToDecimal(value);
        if (price is null)
        {
            return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
        }

        if (price.Value < _min || price.Value > _max)
        {
            return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
        }

        return ValidationResult.Success;
    }

    public override string FormatErrorMessage(string name)
    {
        return string.Format(CultureInfo.CurrentCulture, ErrorMessage!, name);
    }

    private static string FormatCurrency(decimal value)
    {
        return value.ToString("C", CultureInfo.CurrentCulture);
    }

    private static decimal? ConvertToDecimal(object value)
    {
        return value switch
        {
            decimal decimalValue => decimalValue,
            double doubleValue => Convert.ToDecimal(doubleValue, CultureInfo.CurrentCulture),
            float floatValue => Convert.ToDecimal(floatValue, CultureInfo.CurrentCulture),
            int intValue => Convert.ToDecimal(intValue, CultureInfo.CurrentCulture),
            long longValue => Convert.ToDecimal(longValue, CultureInfo.CurrentCulture),
            string stringValue when decimal.TryParse(stringValue, NumberStyles.Any, CultureInfo.CurrentCulture, out var parsed) => parsed,
            _ => null
        };
    }
}
