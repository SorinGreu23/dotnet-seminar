using System.ComponentModel.DataAnnotations;
using System.Linq;
using BookStore.Api.Features.Books;

namespace BookStore.Api.Validators.Attributes;

/// <summary>
/// Restricts a category property to a defined set of <see cref="BookCategory"/> values.
/// </summary>
public sealed class BookCategoryAttribute : ValidationAttribute
{
    private readonly HashSet<BookCategory> _allowedCategories;
    private readonly string _allowedCategoriesText;

    public BookCategoryAttribute(params BookCategory[] allowedCategories)
    {
        ArgumentNullException.ThrowIfNull(allowedCategories);

        if (allowedCategories.Length == 0)
        {
            throw new ArgumentException("At least one category must be specified.", nameof(allowedCategories));
        }

        _allowedCategories = new HashSet<BookCategory>(allowedCategories);
        _allowedCategoriesText = string.Join(", ", _allowedCategories.OrderBy(c => c).Select(c => c.ToString()));

        ErrorMessage = "{0} must be one of the following categories: " + _allowedCategoriesText + ".";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return ValidationResult.Success;
        }

        if (value is BookCategory category && _allowedCategories.Contains(category))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
    }

    public override string FormatErrorMessage(string name)
    {
        return string.Format(ErrorMessage!, name);
    }
}
