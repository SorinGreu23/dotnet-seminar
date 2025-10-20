namespace BookStore.Api.Exceptions;

public class BookNotFoundException : BaseException
{
    protected BookNotFoundException(Guid bookId) : base($"User with ID {bookId} was not found", 404, "BOOK_NOT_FOUND")
    {
    }
}