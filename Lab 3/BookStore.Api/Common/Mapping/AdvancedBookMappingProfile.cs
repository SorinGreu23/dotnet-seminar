using AutoMapper;
using BookStore.Api.Features.Books;
using BookStore.Api.Features.Books.DTOs;
using BookStore.Api.Features.Books.Resolvers;
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
            // Apply 10% discount for Children's books using resolver
            .ForMember(d => d.Price, opt => opt.MapFrom<DiscountedPriceResolver>())
            // Filter out cover image URL for Children's books
            .ForMember(d => d.CoverImageUrl, opt => opt.MapFrom(s => 
                s.Category == BookCategory.Children ? null : s.CoverImageUrl))
            // Category display name using resolver
            .ForMember(d => d.CategoryDisplayName, opt => opt.MapFrom<CategoryDisplayResolver>())
            // Formatted price with currency using resolver
            .ForMember(d => d.FormattedPrice, opt => opt.MapFrom<PriceFormatterResolver>())
            // Published age calculation using resolver
            .ForMember(d => d.PublishedAge, opt => opt.MapFrom<PublishedAgeResolver>())
            // Author initials using resolver
            .ForMember(d => d.AuthorInitials, opt => opt.MapFrom<AuthorInitialsResolver>())
            // Availability status based on stock using resolver
            .ForMember(d => d.AvailabilityStatus, opt => opt.MapFrom<AvailabilityStatusResolver>());
    }
}
