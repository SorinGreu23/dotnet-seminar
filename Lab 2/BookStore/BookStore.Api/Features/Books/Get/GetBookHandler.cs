using BookStore.Api.Data;
using BookStore.Api.Features.Books;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Api.Features.Books.Get;

public class GetBookHandler
{
    private readonly BookDbContext _context;

    public GetBookHandler(BookDbContext context)
    {
        _context = context;
    }
    
    public async Task<IResult> Handle(GetBookRequest request)
    {
        var book = await _context.Set<Book>().FirstOrDefaultAsync(b => b.Id == request.Id);
        
        return book == null ? Results.NotFound($"Book with ID {request.Id} not found") : Results.Ok(book);
    }
}
