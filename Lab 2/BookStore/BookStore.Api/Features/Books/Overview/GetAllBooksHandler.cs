using BookStore.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Api.Features.Books.Overview;

public class GetAllBooksHandler
{
    private readonly BookDbContext _context;

    public GetAllBooksHandler(BookDbContext context)
    {
        _context = context;
    }

    public async Task<IResult> Handle(GetAllBooksRequest request)
    {
        var books = await _context.Books.ToListAsync();
        return Results.Ok(books);
    }
}

