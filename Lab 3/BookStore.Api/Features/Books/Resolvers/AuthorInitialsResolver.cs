using AutoMapper;
using BookStore.Api.Features.Books.DTOs;

namespace BookStore.Api.Features.Books.Resolvers;

public class AuthorInitialsResolver : IValueResolver<Book, BookProfileDto, string>
{
    public string Resolve(Book src, BookProfileDto dest, string destMember, ResolutionContext ctx)
    {
        var parts = (src.Author ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length switch
        {
            0 => "?",
            1 => parts[0][0].ToString().ToUpperInvariant(),
            _ => $"{char.ToUpperInvariant(parts[0][0])}{char.ToUpperInvariant(parts[^1][0])}"
        };
    }
}