using AutoMapper;
using BookStore.Api.Features.Books;
using BookStore.Api.Features.Books.DTOs;

namespace BookStore.Api.Features.Books.Resolvers;

public class CategoryDisplayResolver : IValueResolver<Book, BookProfileDto, string>
{
    public string Resolve(Book src, BookProfileDto dest, string destMember, ResolutionContext ctx) =>
        src.Category switch
        {
            BookCategory.Fiction     => "Fiction & Literature",
            BookCategory.NonFiction  => "Non-Fiction",
            BookCategory.Technical   => "Technical & Professional",
            BookCategory.Children    => "Children's Books",
            _                        => "Uncategorized"
        };
}