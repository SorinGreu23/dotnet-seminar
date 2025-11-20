using AutoMapper;
using BookStore.Api.Features.Books;
using BookStore.Api.Features.Books.DTOs;
using BookStore.Api.Features.Books.Resolvers;
using BookStore.Api.Features.Books.Shared.Create;

namespace BookStore.Api.Common.Mapping;

/// <summary>
/// Advanced mapping profile responsible for projecting <see cref="Book"/> entities to rich
/// <see cref="BookProfileDto"/> models with computed, formatted and conditional fields.
/// NOTE: Request -> Entity mapping lives in <see cref="BookMappingProfile"/> to avoid duplicate type maps.
/// </summary>
public class AdvancedBookMappingProfile : Profile
{
    public AdvancedBookMappingProfile()
    {
        CreateMap<Book, BookProfileDto>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Title))
            .ForMember(d => d.Author, opt => opt.MapFrom(s => s.Author))
            .ForMember(d => d.ISBN, opt => opt.MapFrom(s => s.ISBN))
            .ForMember(d => d.PublishedDate, opt => opt.MapFrom(s => s.PublishedDate))
            .ForMember(d => d.CreatedAt, opt => opt.MapFrom(s => s.CreatedAt))
            .ForMember(d => d.IsAvailable, opt => opt.MapFrom(s => s.IsAvailable))
            .ForMember(d => d.StockQuantity, opt => opt.MapFrom(s => s.StockQuantity))
            .ForMember(d => d.Price, opt => opt.MapFrom<DiscountedPriceResolver>())
            .ForMember(d => d.CoverImageUrl, opt => opt.MapFrom(s => s.Category == BookCategory.Children ? null : s.CoverImageUrl))
            .ForMember(d => d.CategoryDisplayName, opt => opt.MapFrom<CategoryDisplayResolver>())
            .ForMember(d => d.FormattedPrice, opt => opt.MapFrom<PriceFormatterResolver>())
            .ForMember(d => d.PublishedAge, opt => opt.MapFrom<PublishedAgeResolver>())
            .ForMember(d => d.AuthorInitials, opt => opt.MapFrom<AuthorInitialsResolver>())
            .ForMember(d => d.AvailabilityStatus, opt => opt.MapFrom<AvailabilityStatusResolver>());
    }
}
