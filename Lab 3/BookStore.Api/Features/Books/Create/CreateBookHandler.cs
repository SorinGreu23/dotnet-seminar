using AutoMapper;
using BookStore.Api.Exceptions;
using BookStore.Api.Features.Books.DTOs;
using BookStore.Api.Features.Books.Shared.Create;
using BookStore.Api.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace BookStore.Api.Features.Books.Create;

public class CreateBookHandler(
    BookDbContext context, 
    ILogger<CreateBookHandler> logger,
    IMapper mapper,
    IMemoryCache cache)
{
    public async Task<IResult> Handle(CreateBookProfileRequest request)
    {
        logger.LogInformation(
            "Creating a new book - Title: {Title}, Author: {Author}, Category: {Category}, ISBN: {ISBN}",
            request.Title, request.Author, request.Category, request.ISBN);

        // Check ISBN uniqueness
        var isbnExists = await context.Books.AnyAsync(b => b.ISBN == request.ISBN);
        if (isbnExists)
        {
            logger.LogWarning("Book creation failed - ISBN {ISBN} already exists", request.ISBN);
            throw new BookIsbnAlreadyExistsException(request.ISBN);
        }

        // Map request to entity using AutoMapper
        var book = mapper.Map<Book>(request);
        
        context.Books.Add(book);
        await context.SaveChangesAsync();
        
        logger.LogInformation(
            "Book created successfully - ID: {BookId}, Title: {Title}, Category: {Category}", 
            book.Id, book.Title, book.Category);

        // Invalidate cache
        cache.Remove("all_books");
        logger.LogDebug("Cache invalidated for key: all_books");

        // Map to DTO using AutoMapper with all resolvers
        var bookDto = mapper.Map<BookProfileDto>(book);
        
        return Results.Created($"/books/{book.Id}", bookDto);
    }
}