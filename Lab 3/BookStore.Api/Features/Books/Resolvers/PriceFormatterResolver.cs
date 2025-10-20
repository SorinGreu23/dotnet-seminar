using System.Globalization;
using AutoMapper;
using BookStore.Api.Features.Books.DTOs;

namespace BookStore.Api.Features.Books.Resolvers;

public class PriceFormatterResolver : IValueResolver<Book, BookProfileDto, string>
{
    public string Resolve(Book src, BookProfileDto dest, string destMember, ResolutionContext ctx)
    {
        var culture = CultureInfo.CurrentCulture;
        var price = src.Category == BookCategory.Children ? Math.Round(src.Price * 0.9m, 2) : src.Price;
        return price.ToString("C2", culture);
    }
}
