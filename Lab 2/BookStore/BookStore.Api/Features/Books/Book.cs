namespace BookStore.Api.Features.Books;

public record Book(Guid Id, string Title, string Author, string Isbn, int PublicationYear, DateTime CreatedAt);