namespace BookStore.Api.Features.Books.Shared.Create;

public record CreateBookRequest(string Title, string Author, string Isbn, string Publisher, int PublicationYear);