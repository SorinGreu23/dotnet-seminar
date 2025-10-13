using BookStore.Api.Features.Books;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Api.Data;

public class BookDbContext(DbContextOptions<BookDbContext> options) : DbContext(options)
{
    public DbSet<Book> Books { get; set; }
}