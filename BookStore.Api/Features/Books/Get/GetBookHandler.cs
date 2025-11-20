using BookStore.Api.Features.Books;
using BookStore.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Api.Features.Books.Get;

/// <summary>
/// Handles retrieval of a single book by its identifier.
/// </summary>
public class GetBookHandler
{
    private readonly BookDbContext _context;

    /// <summary>
    /// Creates a new instance of the handler.
    /// </summary>
    /// <param name="context">Application database context.</param>
    public GetBookHandler(BookDbContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// Retrieves a single book by ID and returns a minimal API result.
    /// </summary>
    /// <param name="request">Request containing the target book identifier.</param>
    /// <returns>
    /// A 200 OK result with the book when found or a 404 NotFound result when it does not exist.
    /// </returns>
    public async Task<IResult> Handle(GetBookRequest request)
    {
        var book = await _context.Set<Book>().FirstOrDefaultAsync(b => b.Id == request.Id);
        
        return book == null ? Results.NotFound($"Book with ID {request.Id} not found") : Results.Ok(book);
    }
}
