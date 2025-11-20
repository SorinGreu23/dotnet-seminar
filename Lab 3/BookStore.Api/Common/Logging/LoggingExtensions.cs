namespace BookStore.Api.Common.Logging;

/// <summary>
/// Extension methods providing structured logging helpers for book operations & performance metrics.
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Logs a structured summary of a book creation operation including timing and success state.
    /// CorrelationId (if present in scope) will be automatically included by the logging infrastructure.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="metrics">Captured metrics object.</param>
    public static void LogBookCreationMetrics(
        this ILogger logger,
        BookCreationMetrics metrics)
    {
        logger.LogInformation(
            LogEvents.BookCreationCompleted,
            "Book creation operation completed - " +
            "OperationId: {OperationId}, " +
            "Title: {Title}, " +
            "ISBN: {ISBN}, " +
            "Category: {Category}, " +
            "ValidationDuration: {ValidationDuration}ms, " +
            "DatabaseSaveDuration: {DatabaseSaveDuration}ms, " +
            "TotalDuration: {TotalDuration}ms, " +
            "Success: {Success}, " +
            "ErrorReason: {ErrorReason}",
            metrics.OperationId,
            metrics.BookTitle,
            metrics.ISBN,
            metrics.Category,
            metrics.ValidationDuration.TotalMilliseconds,
            metrics.DatabaseSaveDuration.TotalMilliseconds,
            metrics.TotalDuration.TotalMilliseconds,
            metrics.Success,
            metrics.ErrorReason ?? "None"
        );
    }
}
