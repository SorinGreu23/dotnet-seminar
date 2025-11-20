using AutoMapper;
using BookStore.Api.Features.Books;
using BookStore.Api.Features.Books.DTOs;
using BookStore.Api.Features.Books.Shared.Create;

namespace BookStore.Api.Common.Mapping;

/// <summary>
/// Basic book mapping profile for fundamental entity transformations.
/// Advanced computed & conditional mappings are defined in <see cref="AdvancedBookMappingProfile"/>.
/// </summary>
public class BookMappingProfile : Profile
{
    public BookMappingProfile()
    {
        // Request -> Entity
        CreateMap<CreateBookProfileRequest, Book>()
            .ForMember(d => d.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(d => d.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(d => d.IsAvailable, opt => opt.MapFrom(s => s.StockQuantity > 0))
            .ForMember(d => d.PublishedDate, opt => opt.MapFrom(s => s.PublishedDate))
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore());
    }
}
