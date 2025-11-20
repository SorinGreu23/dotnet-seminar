using System.Text.Json;
using AutoMapper;
using BookStore.Api.Common.Logging;
using BookStore.Api.Common.Mapping;
using BookStore.Api.Exceptions;
using BookStore.Api.Features.Books;
using BookStore.Api.Features.Books.Create;
using BookStore.Api.Features.Books.DTOs;
using BookStore.Api.Features.Books.Resolvers;
using BookStore.Api.Features.Books.Shared.Create;
using BookStore.Api.Persistence;
using BookStore.Api.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using AppValidationException = BookStore.Api.Exceptions.ValidationException;

namespace BookStore.Tests.Features.Books.Create;

public class CreateBookHandlerIntegrationTests : IDisposable
{
    private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
    private readonly IMapper _mapper;

    public CreateBookHandlerIntegrationTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddTransient<AuthorInitialsResolver>();
        services.AddTransient<AvailabilityStatusResolver>();
        services.AddTransient<CategoryDisplayResolver>();
        services.AddTransient<DiscountedPriceResolver>();
        services.AddTransient<PriceFormatterResolver>();
        services.AddTransient<PublishedAgeResolver>();
        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<BookMappingProfile>();
            cfg.AddProfile<AdvancedBookMappingProfile>();
        });
        var provider = services.BuildServiceProvider();
        _mapper = provider.GetRequiredService<IMapper>();
    }

    public void Dispose()
    {
        _cache.Dispose();
    }

    [Fact]
    public async Task Handle_ValidTechnicalBookRequest_CreatesBookWithCorrectMappings()
    {
        using var context = CreateContext();
        var handlerLogger = new TestLogger<CreateBookHandler>();
        var validatorLogger = LoggerMoqExtensions.CreateLoggerMock<CreateBookProfileValidator>();
        var validator = new CreateBookProfileValidator(context, validatorLogger.Object);
        var handler = new CreateBookHandler(context, handlerLogger, _mapper, _cache, validator);

        var request = new CreateBookProfileRequest(
            Title: "Modern cloud programming guide",
            Author: "Jane Doe",
            ISBN: "1234567890",
            Category: BookCategory.Technical,
            Price: 59.99m,
            PublishedDate: DateTime.UtcNow.AddYears(-2),
            CoverImageUrl: "http://example.com/cover.jpg",
            StockQuantity: 12);

        var result = await handler.Handle(request);
        var dto = await ExecuteResultAsync(result);

        Assert.Equal("Technical & Professional", dto.CategoryDisplayName);
        Assert.Equal("JD", dto.AuthorInitials);
        Assert.Contains("years", dto.PublishedAge);
        Assert.StartsWith(System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol, dto.FormattedPrice);
        Assert.Equal("In Stock", dto.AvailabilityStatus);

        Assert.Contains(LogEvents.BookCreationStarted, handlerLogger.Events);
    }

    [Fact]
    public async Task Handle_DuplicateISBN_ThrowsValidationExceptionWithLogging()
    {
        using var context = CreateContext();
        var existing = new Book
        {
            Title = "Existing cloud programming guide",
            Author = "Jane Doe",
            ISBN = "1111111111",
            Category = BookCategory.Technical,
            Price = 49.99m,
            PublishedDate = DateTime.UtcNow.AddYears(-1),
            StockQuantity = 5
        };
        context.Books.Add(existing);
        await context.SaveChangesAsync();

        var handlerLogger = new TestLogger<CreateBookHandler>();
        var validatorLogger = LoggerMoqExtensions.CreateLoggerMock<CreateBookProfileValidator>();
        var validator = new CreateBookProfileValidator(context, validatorLogger.Object);
        var handler = new CreateBookHandler(context, handlerLogger, _mapper, _cache, validator);

        var duplicateRequest = new CreateBookProfileRequest(
            Title: "Another cloud programming guide",
            Author: "John Smith",
            ISBN: existing.ISBN,
            Category: BookCategory.Technical,
            Price: 39.99m,
            PublishedDate: DateTime.UtcNow.AddYears(-1),
            CoverImageUrl: null,
            StockQuantity: 3);

        var ex = await Assert.ThrowsAsync<AppValidationException>(() => handler.Handle(duplicateRequest));
        Assert.Contains("already exists", string.Join(";", ex.Errors), StringComparison.OrdinalIgnoreCase);

        validatorLogger.VerifyLog(LogLevel.Information, LogEvents.ISBNValidationPerformed, Times.Once());
        validatorLogger.VerifyLog(LogLevel.Information, LogEvents.DatabaseOperationCompleted, Times.AtLeastOnce());
        Assert.Contains(LogEvents.BookCreationStarted, handlerLogger.Events);
        Assert.Contains(LogEvents.BookValidationFailed, handlerLogger.Events);
    }

    [Fact]
    public async Task Handle_ChildrensBookRequest_AppliesDiscountAndConditionalMapping()
    {
        using var context = CreateContext();
        var handlerLogger = new TestLogger<CreateBookHandler>();
        var validatorLogger = LoggerMoqExtensions.CreateLoggerMock<CreateBookProfileValidator>();
        var validator = new CreateBookProfileValidator(context, validatorLogger.Object);
        var handler = new CreateBookHandler(context, handlerLogger, _mapper, _cache, validator);

        var request = new CreateBookProfileRequest(
            Title: "Happy adventures programming",
            Author: "Alice Wonderland",
            ISBN: "2222222222",
            Category: BookCategory.Children,
            Price: 40.00m,
            PublishedDate: DateTime.UtcNow.AddMonths(-3),
            CoverImageUrl: "http://example.com/child-book.jpg",
            StockQuantity: 7);

        var result = await handler.Handle(request);
        var dto = await ExecuteResultAsync(result);

        Assert.Equal("Children's Books", dto.CategoryDisplayName);
        Assert.Equal(36.00m, dto.Price);
        Assert.Null(dto.CoverImageUrl);
        Assert.StartsWith(System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol, dto.FormattedPrice);

        Assert.Contains(LogEvents.BookCreationStarted, handlerLogger.Events);
    }

    private static BookDbContext CreateContext() => new(new DbContextOptionsBuilder<BookDbContext>()
        .UseInMemoryDatabase(databaseName: $"bookstore_tests_{Guid.NewGuid()}_")
        .Options);

    private static async Task<BookProfileDto> ExecuteResultAsync(IResult result)
    {
        var ctx = new DefaultHttpContext();
        ctx.RequestServices = new ServiceCollection().AddLogging().BuildServiceProvider();
        ctx.Response.Body = new MemoryStream();
        await result.ExecuteAsync(ctx);
        Assert.Equal(StatusCodes.Status201Created, ctx.Response.StatusCode);
        ctx.Response.Body.Seek(0, SeekOrigin.Begin);
        var dto = await JsonSerializer.DeserializeAsync<BookProfileDto>(ctx.Response.Body, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        Assert.NotNull(dto);
        return dto!;
    }
}

internal static class LoggerMoqExtensions
{
    internal static Mock<ILogger<T>> CreateLoggerMock<T>()
    {
        var mock = new Mock<ILogger<T>>();
        mock.Setup(l => l.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        return mock;
    }

    public static void VerifyLog(this Mock<ILogger> logger, LogLevel level, int eventId, Times times)
        => logger.Verify(x => x.Log(
            It.Is<LogLevel>(l => l == level),
            It.Is<EventId>(e => e.Id == eventId),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), times);

    public static void VerifyLog<T>(this Mock<ILogger<T>> logger, LogLevel level, int eventId, Times times)
        => logger.Verify(x => x.Log(
            It.Is<LogLevel>(l => l == level),
            It.Is<EventId>(e => e.Id == eventId),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), times);
}

internal class TestLogger<T> : ILogger<T>
{
    public List<int> Events { get; } = new();

    public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;
    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Events.Add(eventId.Id);
    }

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();
        public void Dispose() { }
    }
}
