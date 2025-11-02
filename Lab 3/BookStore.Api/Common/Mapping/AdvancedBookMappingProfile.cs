using System.Globalization;
using AutoMapper;
using BookStore.Api.Features.Books;
using BookStore.Api.Features.Books.DTOs;
using BookStore.Api.Features.Books.Shared.Create;

namespace BookStore.Api.Common.Mapping;

public class AdvancedBookMappingProfile : Profile
{
    public AdvancedBookMappingProfile()
    {
        // CreateBookProfileRequest -> Book
        CreateMap<CreateBookProfileRequest, Book>()
            .ForMember(d => d.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(d => d.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(d => d.IsAvailable, opt => opt.MapFrom(s => s.StockQuantity > 0))
            .ForMember(d => d.PublishedDate, opt => opt.MapFrom(s => s.PublishedDate))
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore());
        
        // Book -> DTO
        CreateMap<Book, BookProfileDto>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Title))
            .ForMember(d => d.Author, opt => opt.MapFrom(s => s.Author))
            .ForMember(d => d.ISBN, opt => opt.MapFrom(s => s.ISBN))
            .ForMember(d => d.PublishedDate, opt => opt.MapFrom(s => s.PublishedDate))
            .ForMember(d => d.CreatedAt, opt => opt.MapFrom(s => s.CreatedAt))
            .ForMember(d => d.IsAvailable, opt => opt.MapFrom(s => s.IsAvailable))
            .ForMember(d => d.StockQuantity, opt => opt.MapFrom(s => s.StockQuantity))
            // Apply 10% discount for Children's books
            .ForMember(d => d.Price, opt => opt.MapFrom(s => 
                s.Category == BookCategory.Children ? Math.Round(s.Price * 0.9m, 2) : s.Price))
            // Filter out cover image URL for Children's books
            .ForMember(d => d.CoverImageUrl, opt => opt.MapFrom(s => 
                s.Category == BookCategory.Children ? null : s.CoverImageUrl))
            // Category display name - using method to avoid switch expression limitation
            .ForMember(d => d.CategoryDisplayName, opt => opt.MapFrom(s => GetCategoryDisplayName(s.Category)))
            // Formatted price with currency
            .ForMember(d => d.FormattedPrice, opt => opt.MapFrom(s => FormatPrice(s.Price, s.Category)))
            // Published age calculation
            .ForMember(d => d.PublishedAge, opt => opt.MapFrom(s => GetPublishedAge(s.PublishedDate)))
            // Author initials
            .ForMember(d => d.AuthorInitials, opt => opt.MapFrom(s => GetAuthorInitials(s.Author)))
            // Availability status based on stock
            .ForMember(d => d.AvailabilityStatus, opt => opt.MapFrom(s => GetAvailabilityStatus(s.IsAvailable, s.StockQuantity)));
    }

    private static string GetCategoryDisplayName(BookCategory category)
    {
        return category switch
        {
            BookCategory.Fiction => "Fiction & Literature",
            BookCategory.NonFiction => "Non-Fiction",
            BookCategory.Technical => "Technical & Professional",
            BookCategory.Children => "Children's Books",
            _ => "Uncategorized"
        };
    }

    private static string FormatPrice(decimal price, BookCategory category)
    {
        var finalPrice = category == BookCategory.Children ? Math.Round(price * 0.9m, 2) : price;
        return finalPrice.ToString("C2", CultureInfo.CurrentCulture);
    }

    private static string GetPublishedAge(DateTime publishedDate)
    {
        var days = (DateTime.UtcNow.Date - publishedDate.Date).TotalDays;
        return days switch
        {
            < 30 => "New Release",
            < 365 => $"{(int)(days / 30)} months old",
            < 1825 => $"{(int)(days / 365)} years old",
            _ => "Classic"
        };
    }

    private static string GetAuthorInitials(string? author)
    {
        var parts = (author ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length switch
        {
            0 => "?",
            1 => parts[0][0].ToString().ToUpperInvariant(),
            _ => $"{char.ToUpperInvariant(parts[0][0])}{char.ToUpperInvariant(parts[parts.Length - 1][0])}"
        };
    }

    private static string GetAvailabilityStatus(bool isAvailable, int stockQuantity)
    {
        if (!isAvailable) return "Out of Stock";
        
        return stockQuantity switch
        {
            0 => "Unavailable",
            1 => "Last Copy",
            <= 5 => "Limited Stock",
            _ => "In Stock"
        };
    }
}
