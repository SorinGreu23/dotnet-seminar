namespace BookStore.Api.Features.Books.DTOs;

public class BookProfileDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Author { get; set; } = null!;
    public string ISBN { get; set; } = null!;
    public string CategoryDisplayName { get; set; } = null!;
    public decimal Price { get; set; }
    public string FormattedPrice { get; set; } = null!;
    public DateTime PublishedDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CoverImageUrl { get; set; }
    public bool IsAvailable { get; set; }
    public int StockQuantity { get; set; }
    public string PublishedAge { get; set; } = null!;
    public string AuthorInitials { get; set; } = null!;
    public string AvailabilityStatus { get; set; } = null!;
}
