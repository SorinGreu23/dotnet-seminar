using BookStore.Api.Features.Books.Shared.Create;
using FluentValidation;

namespace BookStore.Api.Validators;

public class CreateBookValidator : AbstractValidator<CreateBookRequest>
{
    public CreateBookValidator()
    {
        RuleFor(request => request.Title)
            .NotNull()
            .NotEmpty()
            .WithMessage("Title is required.")
            .MinimumLength(5)
            .WithMessage("Title must be at least 5 characters long.");

        RuleFor(request => request.Author)
            .NotNull()
            .NotEmpty()
            .WithMessage("Author is required.")
            .MinimumLength(5)
            .WithMessage("Author must be at least 5 characters long.");

        RuleFor(request => request.Publisher)
            .NotNull()
            .NotEmpty()
            .WithMessage("Publisher is required.")
            .MinimumLength(5)
            .WithMessage("Publisher must be at least 5 characters long.");

        RuleFor(request => request.Isbn)
            .NotNull()
            .NotEmpty()
            .WithMessage("ISBN is required.");

        RuleFor(request => request.PublicationYear)
            .NotNull()
            .NotEmpty()
            .WithMessage("Publication year is required.")
            .InclusiveBetween(1900, DateTime.UtcNow.Year)
            .WithMessage($"Publication year must be between 1900 and {DateTime.UtcNow.Year}.");
    }
}