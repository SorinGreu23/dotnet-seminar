using System.Diagnostics;
using AutoMapper;
using BookStore.Api.Common.Logging;
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
    private const string CacheKey = "all_books";

    public async Task<IResult> Handle(CreateBookProfileRequest request)
    {
        var operationStartTime = Stopwatch.GetTimestamp();
        var operationId = Guid.NewGuid().ToString()[..8];

        using var scope = CreateLoggingScope(operationId);

        LogOperationStart(operationId, request);

        var validationDuration = TimeSpan.Zero;
        var databaseSaveDuration = TimeSpan.Zero;

        try
        {
            validationDuration = await ValidateBookAsync(request);
            var book = mapper.Map<Book>(request);
            databaseSaveDuration = await SaveBookAsync(book, request.Title);
            InvalidateCache();

            var totalDuration = Stopwatch.GetElapsedTime(operationStartTime);
            LogSuccessMetrics(operationId, request, validationDuration, databaseSaveDuration, totalDuration);

            var bookDto = mapper.Map<BookProfileDto>(book);
            return Results.Created($"/books/{book.Id}", bookDto);
        }
        catch (Exception ex)
        {
            var totalDuration = Stopwatch.GetElapsedTime(operationStartTime);
            LogErrorMetrics(operationId, request, validationDuration, databaseSaveDuration, totalDuration, ex.Message);
            throw;
        }
    }

    private IDisposable? CreateLoggingScope(string operationId) =>
        logger.BeginScope(new Dictionary<string, object>
        {
            ["OperationId"] = operationId,
            ["Operation"] = "BookCreation"
        });

    private void LogOperationStart(string operationId, CreateBookProfileRequest request) =>
        logger.LogInformation(
            LogEvents.BookCreationStarted,
            "Book creation operation started - OperationId: {OperationId}, Title: {Title}, Author: {Author}, Category: {Category}, ISBN: {ISBN}",
            operationId, request.Title, request.Author, request.Category, request.ISBN);

    private async Task<TimeSpan> ValidateBookAsync(CreateBookProfileRequest request)
    {
        var validationStartTime = Stopwatch.GetTimestamp();

        logger.LogDebug(LogEvents.ISBNValidationPerformed, "Performing ISBN validation for ISBN: {ISBN}", request.ISBN);

        var isbnExists = await context.Books.AnyAsync(b => b.ISBN == request.ISBN);
        if (isbnExists)
        {
            var duration = Stopwatch.GetElapsedTime(validationStartTime);
            logger.LogWarning(
                LogEvents.BookValidationFailed,
                "Book validation failed - ISBN {ISBN} already exists. ValidationDuration: {ValidationDuration}ms",
                request.ISBN, duration.TotalMilliseconds);

            throw new BookIsbnAlreadyExistsException(request.ISBN);
        }

        logger.LogDebug(LogEvents.StockValidationPerformed, "Stock validation - StockQuantity: {StockQuantity}", request.StockQuantity);

        var validationDuration = Stopwatch.GetElapsedTime(validationStartTime);
        logger.LogDebug("Validation phase completed - Duration: {ValidationDuration}ms", validationDuration.TotalMilliseconds);

        return validationDuration;
    }

    private async Task<TimeSpan> SaveBookAsync(Book book, string title)
    {
        var dbStartTime = Stopwatch.GetTimestamp();

        logger.LogDebug(LogEvents.DatabaseOperationStarted, "Starting database save operation for book: {Title}", title);

        context.Books.Add(book);
        await context.SaveChangesAsync();

        var databaseSaveDuration = Stopwatch.GetElapsedTime(dbStartTime);
        logger.LogInformation(
            LogEvents.DatabaseOperationCompleted,
            "Database save operation completed - BookId: {BookId}, Duration: {DatabaseDuration}ms",
            book.Id, databaseSaveDuration.TotalMilliseconds);

        return databaseSaveDuration;
    }

    private void InvalidateCache()
    {
        cache.Remove(CacheKey);
        logger.LogDebug(LogEvents.CacheOperationPerformed, "Cache invalidated for key: {CacheKey}", CacheKey);
    }

    private void LogSuccessMetrics(
        string operationId,
        CreateBookProfileRequest request,
        TimeSpan validationDuration,
        TimeSpan databaseSaveDuration,
        TimeSpan totalDuration)
    {
        var metrics = new BookCreationMetrics(
            operationId,
            request.Title,
            request.ISBN,
            request.Category,
            validationDuration,
            databaseSaveDuration,
            totalDuration,
            Success: true);

        logger.LogBookCreationMetrics(metrics);
    }

    private void LogErrorMetrics(
        string operationId,
        CreateBookProfileRequest request,
        TimeSpan validationDuration,
        TimeSpan databaseSaveDuration,
        TimeSpan totalDuration,
        string errorReason)
    {
        var errorMetrics = new BookCreationMetrics(
            operationId,
            request.Title,
            request.ISBN,
            request.Category,
            validationDuration,
            databaseSaveDuration,
            totalDuration,
            Success: false,
            errorReason);

        logger.LogBookCreationMetrics(errorMetrics);
    }
}