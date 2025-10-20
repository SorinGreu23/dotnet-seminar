using AutoMapper;
using BookStore.Api.Features.Books.DTOs;

namespace BookStore.Api.Features.Books.Resolvers;

public class AvailabilityStatusResolver : IValueResolver<Book, BookProfileDto, string>
{
    public string Resolve(Book src, BookProfileDto dest, string destMember, ResolutionContext ctx)
    {
        if (!src.IsAvailable) return "Out of Stock";

        return src.StockQuantity switch
        {
            0 => "Unavailable",
            1 => "Last Copy",
            <= 5 => "Limited Stock",
            _ => "In Stock"
        };
    }
}