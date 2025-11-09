using BookStore.Api.Common.Logging;
using BookStore.Api.Features.Books;
using BookStore.Api.Features.Books.Shared.Create;
using BookStore.Api.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace BookStore.Api.Validators;

public class CreateBookProfileValidator : AbstractValidator<CreateBookProfileRequest>
{
    private readonly IApplicationContext _dbContext;
    private readonly ILogger<CreateBookProfileValidator> _logger;

    private static readonly HashSet<string> InappropriateWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "inappropriate1", "inappropriate2", "badword"
    };

    private static readonly HashSet<string> RestrictedChildrenWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "violence", "scary", "adult"
    };

    private static readonly HashSet<string> TechnicalKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "technology", "software", "hardware", "programming", "engineering", "development", "cloud", "data", "ai", "machine"
    };

    public CreateBookProfileValidator(IApplicationContext dbContext, ILogger<CreateBookProfileValidator> logger)
    {
        _dbContext = dbContext;
        _logger = logger;

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MinimumLength(1).WithMessage("Title must be at least 1 character.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.")
            .Must(BeValidTitle).WithMessage("Title contains inappropriate content.")
            .MustAsync(BeUniqueTitle).WithMessage("A book with this title and author already exists.");

        RuleFor(x => x.Author)
            .NotEmpty().WithMessage("Author is required.")
            .MinimumLength(2).WithMessage("Author must be at least 2 characters.")
            .MaximumLength(100).WithMessage("Author must not exceed 100 characters.")
            .Must(BeValidAuthorName).WithMessage("Author contains invalid characters. Only letters, spaces, hyphens, apostrophes, and dots are allowed.");

        RuleFor(x => x.ISBN)
            .NotEmpty().WithMessage("ISBN is required.")
            .Must(BeValidISBN).WithMessage("ISBN must be a valid 10 or 13 digit format.")
            .MustAsync(BeUniqueISBN).WithMessage("ISBN already exists in the system.");

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Category must be a valid enum value.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0.")
            .LessThan(10000).WithMessage("Price must be less than $10,000.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(20.00m).WithMessage("Technical books must cost at least $20.00.")
            .When(x => x.Category == BookCategory.Technical);

        RuleFor(x => x.Price)
            .LessThanOrEqualTo(50.00m).WithMessage("Children's books must cost $50.00 or less.")
            .When(x => x.Category == BookCategory.Children);

        RuleFor(x => x.PublishedDate)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Published date cannot be in the future.")
            .GreaterThanOrEqualTo(new DateTime(1400, 1, 1)).WithMessage("Published date cannot be before year 1400.");

        RuleFor(x => x.PublishedDate)
            .Must((_, publishedDate) => BeRecentTechnicalBook(publishedDate))
            .WithMessage("Technical books must be published within the last 5 years.")
            .When(x => x.Category == BookCategory.Technical);

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative.")
            .LessThanOrEqualTo(100000).WithMessage("Stock quantity cannot exceed 100,000.");

        RuleFor(x => x.CoverImageUrl)
            .Must(BeValidImageUrl).WithMessage("Cover image URL must be a valid HTTP/HTTPS URL ending with an image extension (.jpg, .jpeg, .png, .gif, .webp).")
            .When(x => !string.IsNullOrEmpty(x.CoverImageUrl));

        RuleFor(x => x.Title)
            .Must(ContainTechnicalKeywords).WithMessage("Technical books must include technical keywords in the title.")
            .When(x => x.Category == BookCategory.Technical);

        RuleFor(x => x.Title)
            .Must(BeAppropriateForChildren).WithMessage("Children's book titles must be appropriate for children.")
            .When(x => x.Category == BookCategory.Children);

        RuleFor(x => x.Author)
            .MinimumLength(5).WithMessage("Fiction books require an author name of at least 5 characters.")
            .When(x => x.Category == BookCategory.Fiction);

        RuleFor(x => x)
            .Must(HaveLimitedStockForExpensiveBooks)
            .WithMessage("Books priced over $100 must have stock quantities of 20 or less.");

        RuleFor(x => x)
            .MustAsync(PassBusinessRules).WithMessage("Business rules validation failed.");
    }

    private bool BeValidTitle(string title)
    {
        var words = title.Split(new[] { ' ', '-', '.', ',', ';', ':', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
        return !words.Any(word => InappropriateWords.Contains(word));
    }

    private async Task<bool> BeUniqueTitle(CreateBookProfileRequest request, string title, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogEvents.DatabaseOperationStarted, "Checking title uniqueness for Title: {Title}, Author: {Author}", title, request.Author);

        var exists = await _dbContext.Books
            .AnyAsync(b => b.Title.ToLower() == title.ToLower() && b.Author.ToLower() == request.Author.ToLower(), cancellationToken);

        _logger.LogInformation(LogEvents.DatabaseOperationCompleted, "Title uniqueness check completed. Exists: {Exists}", exists);

        return !exists;
    }

    private bool BeValidAuthorName(string author)
    {
        // Allow letters, spaces, hyphens, apostrophes, dots
        var regex = new Regex(@"^[a-zA-Z\s\-'.]+$", RegexOptions.Compiled);
        return regex.IsMatch(author);
    }

    private bool BeValidISBN(string isbn)
    {
        // Remove hyphens for validation
        var cleanIsbn = isbn.Replace("-", "");

        // Check if 10 or 13 digits
        if (cleanIsbn.Length != 10 && cleanIsbn.Length != 13)
            return false;

        // Check if all digits
        return cleanIsbn.All(char.IsDigit);
    }

    private async Task<bool> BeUniqueISBN(CreateBookProfileRequest request, string isbn, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogEvents.ISBNValidationPerformed, "Checking ISBN uniqueness for ISBN: {ISBN}", isbn);

        var exists = await _dbContext.Books
            .AnyAsync(b => b.ISBN == isbn, cancellationToken);

        _logger.LogInformation(LogEvents.DatabaseOperationCompleted, "ISBN uniqueness check completed. Exists: {Exists}", exists);

        return !exists;
    }

    private bool BeValidImageUrl(string? url)
    {
        if (string.IsNullOrEmpty(url))
            return true; // Optional field

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            return false;

        var extension = Path.GetExtension(uri.AbsolutePath).ToLowerInvariant();
        var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        return validExtensions.Contains(extension);
    }

    private bool ContainTechnicalKeywords(string title)
    {
        var words = title.Split(new[] { ' ', '-', '.', ',', ';', ':', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
        return words.Any(word => TechnicalKeywords.Contains(word));
    }

    private bool BeAppropriateForChildren(string title)
    {
        var words = title.Split(new[] { ' ', '-', '.', ',', ';', ':', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
        return !words.Any(word => RestrictedChildrenWords.Contains(word) || InappropriateWords.Contains(word));
    }

    private bool HaveLimitedStockForExpensiveBooks(CreateBookProfileRequest request)
    {
        if (request.Price <= 100)
        {
            return true;
        }

        var isValid = request.StockQuantity <= 20;
        if (!isValid)
        {
            _logger.LogWarning(LogEvents.StockValidationPerformed, "Expensive book stock limit exceeded. Price: {Price}, Stock: {Stock}", request.Price, request.StockQuantity);
        }

        return isValid;
    }

    private bool BeRecentTechnicalBook(DateTime publishedDate)
    {
        var cutoff = DateTime.UtcNow.AddYears(-5);
        return publishedDate >= cutoff;
    }

    private async Task<bool> PassBusinessRules(CreateBookProfileRequest request, CancellationToken cancellationToken)
    {
        // Rule 1: Daily book addition limit (max 500 per day)
        var today = DateTime.UtcNow.Date;
        var todayBooksCount = await _dbContext.Books
            .CountAsync(b => b.CreatedAt.Date == today, cancellationToken);

        if (todayBooksCount >= 500)
        {
            _logger.LogWarning("Daily book addition limit exceeded. Current count: {Count}", todayBooksCount);
            return false;
        }

        // Rule 2: Technical books minimum price ($20.00)
        if (request.Category == BookCategory.Technical && request.Price < 20.00m)
        {
            _logger.LogWarning(LogEvents.BookValidationFailed, "Technical book price too low. Price: {Price}", request.Price);
            return false;
        }

        if (request.Category == BookCategory.Technical && !ContainTechnicalKeywords(request.Title))
        {
            _logger.LogWarning(LogEvents.BookValidationFailed, "Technical book missing technical keywords in title: {Title}", request.Title);
            return false;
        }

        if (request.Category == BookCategory.Technical && !BeRecentTechnicalBook(request.PublishedDate))
        {
            _logger.LogWarning(LogEvents.BookValidationFailed, "Technical book is older than 5 years. Published: {PublishedDate}", request.PublishedDate);
            return false;
        }

        // Rule 3: Children's book content restrictions
        if (request.Category == BookCategory.Children)
        {
            if (!BeAppropriateForChildren(request.Title))
            {
                _logger.LogWarning(LogEvents.BookValidationFailed, "Children's book contains restricted content in title: {Title}", request.Title);
                return false;
            }
        }

        // Rule 4: High-value book stock limit (>$500 = max 10 stock)
        if (request.Price > 500 && request.StockQuantity > 10)
        {
            _logger.LogWarning(LogEvents.StockValidationPerformed, "High-value book stock limit exceeded. Price: {Price}, Stock: {Stock}", request.Price, request.StockQuantity);
            return false;
        }

        return true;
    }
}