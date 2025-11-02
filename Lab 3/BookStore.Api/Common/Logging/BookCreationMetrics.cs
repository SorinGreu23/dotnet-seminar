using BookStore.Api.Features.Books;

namespace BookStore.Api.Common.Logging;

/// <summary>
/// Captures comprehensive metrics for book creation operations
/// </summary>
public record BookCreationMetrics(
    string OperationId,
    string BookTitle,
    string ISBN,
    BookCategory Category,
    TimeSpan ValidationDuration,
    TimeSpan DatabaseSaveDuration,
    TimeSpan TotalDuration,
    bool Success,
    string? ErrorReason = null
);
