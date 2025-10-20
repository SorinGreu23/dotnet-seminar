using BookStore.Api.Features.Books.Shared.Create;
using BookStore.Api.Persistence;
using BookStore.Api.Validators;

namespace BookStore.Api.Features.Books.Create;

public class CreateBookHandler(BookDbContext context, ILogger<CreateBookHandler> logger)
{
    public async Task<IResult> Handle(CreateBookRequest request)
    {
        logger.LogInformation($"Creating a new book with title: {request.Title} and author: {request.Author}");
        var validator = new CreateBookValidator();
        var validationResult = await validator.ValidateAsync(request);
        
        if(!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.Errors);
        }

        var book = new Book
        {
            Title = request.Title,
            Author = request.Author,
            ISBN = request.Isbn,
            Category = BookCategory.Fiction,
            Price = 0m,
            PublishedDate = new DateTime(request.PublicationYear, 1, 1),
            IsAvailable = true,
            StockQuantity = 1,
        };
        
        context.Books.Add(book);
        await context.SaveChangesAsync();
        logger.LogInformation($"Book created successfully with ID: {book.Id}");
        
        return Results.Created($"/books/{book.Id}", book);
    }
}