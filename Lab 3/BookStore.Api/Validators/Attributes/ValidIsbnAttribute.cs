using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Linq;

namespace BookStore.Api.Validators.Attributes;

/// <summary>
/// Server and client-side validation for ISBN values (10 or 13 digits).
/// </summary>
public sealed class ValidIsbnAttribute : ValidationAttribute, IClientModelValidator
{
    private const string ClientRuleName = "validisbn";

    public ValidIsbnAttribute()
        : base("{0} must be a valid ISBN containing 10 or 13 digits.")
    {
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return ValidationResult.Success;
        }

        if (value is not string isbnValue)
        {
            return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
        }

        var normalized = NormalizeIsbn(isbnValue);
        if (IsValidLength(normalized) && normalized.All(char.IsDigit))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
    }

    public void AddValidation(ClientModelValidationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        MergeAttribute(context.Attributes, "data-val", "true");
        MergeAttribute(context.Attributes, $"data-val-{ClientRuleName}", FormatErrorMessage(GetDisplayName(context)));
    }

    private static string NormalizeIsbn(string isbn)
    {
        return new string(isbn.Where(c => !char.IsWhiteSpace(c) && c != '-').ToArray());
    }

    private static bool IsValidLength(string isbn)
    {
        return isbn.Length is 10 or 13;
    }

    private static bool MergeAttribute(IDictionary<string, string> attributes, string key, string value)
    {
        if (attributes.ContainsKey(key))
        {
            return false;
        }

        attributes[key] = value;
        return true;
    }

    private static string GetDisplayName(ClientModelValidationContext context)
    {
        return context.ModelMetadata?.DisplayName
            ?? context.ModelMetadata?.Name
            ?? "Value";
    }
}
