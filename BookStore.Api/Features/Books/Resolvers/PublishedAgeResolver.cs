using AutoMapper;
using BookStore.Api.Features.Books.DTOs;

namespace BookStore.Api.Features.Books.Resolvers;

public class PublishedAgeResolver : IValueResolver<Book, BookProfileDto, string>
{
    public string Resolve(Book src, BookProfileDto dest, string destMember, ResolutionContext ctx)
    {
        var days = (DateTime.UtcNow.Date - src.PublishedDate.Date).TotalDays;
        return days switch
        {
            < 30 => "New Release",
            < 365 => $"{(int)(days / 30)} months old",
            < 1825 => $"{(int)(days / 365)} years old",
            _ => "Classic"
        };
    }
}