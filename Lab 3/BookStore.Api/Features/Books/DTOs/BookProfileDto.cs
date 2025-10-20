namespace BookStore.Api.Features.Books.DTOs;

public record BookProfileDto(
    Guid Id,
    string Title,
    string Author,
    string ISBN,
    string CategoryDisplayName,
    decimal Price,
    string FormattedPrice,
    DateTime PublishedDate,
    DateTime CreatedAt,
    string? CoverImageUrl,
    bool IsAvailable,
    int StockQuantity,
    string PublishedAge,
    string AuthorInitials,
    string AvailabilityStatus
);
