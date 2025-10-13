using BookStore.Api.Data;
using BookStore.Api.Features.Books;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Api.Features.Books.Delete;

public class DeleteBookHandler
{
    private readonly BookDbContext _context;

    public DeleteBookHandler(BookDbContext context)
    {
        _context = context;
    }
    
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
