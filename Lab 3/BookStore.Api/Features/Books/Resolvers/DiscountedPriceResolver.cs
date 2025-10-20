using AutoMapper;
using BookStore.Api.Features.Books.DTOs;

namespace BookStore.Api.Features.Books.Resolvers;

public class DiscountedPriceResolver : IValueResolver<Book, BookProfileDto, decimal>
{
    public decimal Resolve(Book src, BookProfileDto dest, decimal destMember, ResolutionContext ctx)
        => src.Category == BookCategory.Children ? Math.Round(src.Price * 0.9m, 2) : src.Price;
}