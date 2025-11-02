namespace BookStore.Api.Common.Logging;

public static class LoggingExtensions
{
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
