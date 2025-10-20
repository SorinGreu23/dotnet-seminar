namespace BookStore.Api.Features.Books.Shared.Create;

public record CreateBookProfileRequest(
    string Title,
    string Author,
    string ISBN,
    BookCategory Category,
    decimal Price,
    DateTime PublishedDate,
    string? CoverImageUrl = null,
    int StockQuantity = 1
);