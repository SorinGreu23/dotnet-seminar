using BookStore.Api.Data;
using BookStore.Api.Validators;

namespace BookStore.Api.Features.Books.Shared.Create;

public class CreateBookHandler
{
    private readonly BookDbContext _context;

    public CreateBookHandler(BookDbContext context)
    {
        _context = context;
    }
    
    public async Task<IResult> Handle(CreateBookRequest request)
    {
        var validator = new CreateBookValidator();
        var validationResult = await validator.ValidateAsync(request);
        
        if(!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.Errors);
        }

        var book = new Book(Guid.NewGuid(), request.Title,request.Title, request.Isbn, request.PublicationYear, CreatedAt: DateTime.UtcNow);
        _context.Books.Add(book);
        await _context.SaveChangesAsync();
        
        return Results.Created($"/books/{book.Id}", book);
    }
}