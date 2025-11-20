using BookStore.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Api.Features.Books.Delete;

/// <summary>
/// Handles deletion of books from the catalog.
/// </summary>
public class DeleteBookHandler
{
    private readonly BookDbContext _context;

    /// <summary>
    /// Creates a new instance of the handler.
    /// </summary>
    /// <param name="context">Application database context.</param>
    public DeleteBookHandler(BookDbContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// Deletes the specified book and returns an appropriate minimal API result.
    /// </summary>
    /// <param name="request">Request containing the identifier of the book to delete.</param>
    /// <returns>
    /// A 204 NoContent result when the book is deleted or a 404 NotFound result if it does not exist.
    /// </returns>
    public async Task<IResult> Handle(DeleteBookRequest request)
    {
        var book = await _context.Set<Book>().FirstOrDefaultAsync(b => b.Id == request.Id);
        
        if (book == null)
        {
            return Results.NotFound($"Book with ID {request.Id} not found");
        }

        _context.Remove(book);
        await _context.SaveChangesAsync();
        
        return Results.NoContent();
    }
}
