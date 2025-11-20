namespace BookStore.Api.Middleware;

/// <summary>
/// Standard error payload returned by the API when an operation fails.
/// </summary>
public class ErrorResponse()
{
    /// <summary>
    /// Creates an error response with the specified code and message.
    /// </summary>
    /// <param name="errorCode">Machine-readable error code.</param>
    /// <param name="message">Human-readable error message.</param>
    public ErrorResponse(string errorCode, string message) : this()
    {
        ErrorCode = errorCode;
        Message = message;
    }
    
    /// <summary>
    /// Creates an error response with details for validation or business rule failures.
    /// </summary>
    /// <param name="errorCode">Machine-readable error code.</param>
    /// <param name="message">Human-readable error message.</param>
    /// <param name="details">Collection of additional error details.</param>
    public ErrorResponse(string errorCode, string message, List<string> details) : this(errorCode, message)
    {
        Details = details;
    }

    /// <summary>
    /// Optional list of detailed error messages (for example validation errors).
    /// </summary>
    public List<string> Details { get; set; } = [];

    /// <summary>
    /// Human-readable description of the error.
    /// </summary>
    public string Message { get; set; } = default!;

    /// <summary>
    /// Machine-readable error code that can be used by clients.
    /// </summary>
    public string ErrorCode { get; set; } = default!;
    
    /// <summary>
    /// Trace identifier associated with the failing request.
    /// </summary>
    public string TraceId { get; set; } = default!;
}
