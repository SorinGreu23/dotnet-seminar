using BookStore.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Api.Features.Books.Overview;

/// <summary>
/// Handles retrieval of all books from the catalog.
/// </summary>
public class GetAllBooksHandler
{
    private readonly BookDbContext _context;

    /// <summary>
    /// Creates a new instance of the handler.
    /// </summary>
    /// <param name="context">Application database context.</param>
    public GetAllBooksHandler(BookDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves all books and returns them as a minimal API result.
    /// </summary>
    /// <param name="request">Request marker type for getting all books.</param>
    /// <returns>A 200 OK result containing the collection of books.</returns>
    public async Task<IResult> Handle(GetAllBooksRequest request)
    {
        var books = await _context.Books.ToListAsync();
        return Results.Ok(books);
    }
}
