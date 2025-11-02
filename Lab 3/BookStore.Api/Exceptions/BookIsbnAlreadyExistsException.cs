namespace BookStore.Api.Exceptions;

public class BookIsbnAlreadyExistsException : ValidationException
{
    public BookIsbnAlreadyExistsException(string isbn) 
        : base($"A book with ISBN '{isbn}' already exists in the system.")
    {
    }
}
