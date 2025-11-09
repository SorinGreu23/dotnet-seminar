using BookStore.Api.Features.Books;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Api.Persistence;

public interface IApplicationContext
{
    DbSet<Book> Books { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
