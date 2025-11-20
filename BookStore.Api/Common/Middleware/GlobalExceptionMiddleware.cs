using System.Net;
using System.Text.Json;
using BookStore.Api.Exceptions;

namespace BookStore.Api.Middleware;

/// <summary>
/// Middleware that converts unhandled exceptions into structured JSON error responses.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    
    /// <summary>
    /// Creates a new instance of <see cref="GlobalExceptionMiddleware"/>.
    /// </summary>
    /// <param name="next">The next request delegate in the pipeline.</param>
    /// <param name="logger">Logger used to record exception details.</param>
    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware and handles any exception thrown by downstream components.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred while processing the request. TraceId: {TraceId}",
                context.TraceIdentifier);
            await HandleExceptionAsync(context, ex);
        }
    }
    
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        ErrorResponse errorResponse;
        int statusCode;

        switch (exception)
        {
            case ValidationException validationEx:
                errorResponse = new ErrorResponse(validationEx.ErrorCode, validationEx.Message, validationEx.Errors)
                {
                    TraceId = context.TraceIdentifier
                };
                statusCode = validationEx.StatusCode;
                break;
                
            case BaseException baseEx:
                errorResponse = new ErrorResponse(baseEx.ErrorCode, baseEx.Message)
                {
                    TraceId = context.TraceIdentifier
                };
                statusCode = baseEx.StatusCode;
                break;
                
            default:
                errorResponse = new ErrorResponse("INTERNAL_SERVER_ERROR", "An unexpected error occurred")
                {
                    TraceId = context.TraceIdentifier
                };
                statusCode = (int)HttpStatusCode.InternalServerError;
                break;
        }

        context.Response.StatusCode = statusCode;
        var response = JsonSerializer.Serialize(errorResponse);
        await context.Response.WriteAsync(response);
    }
}
