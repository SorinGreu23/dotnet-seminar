namespace BookStore.Api.Middleware;

/// <summary>
/// Middleware that ensures every request has a correlation identifier and flows it via logging scope.
/// </summary>
public class CorrelationMiddleware
{
    private const string CorrelationIdHeaderName = "X-Correlation-ID";
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationMiddleware> _logger;

    /// <summary>
    /// Creates a new instance of <see cref="CorrelationMiddleware"/>.
    /// </summary>
    /// <param name="next">The next request delegate in the pipeline.</param>
    /// <param name="logger">Logger used to record diagnostic information.</param>
    public CorrelationMiddleware(RequestDelegate next, ILogger<CorrelationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware to enrich the request with a correlation ID and logging scope.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrCreateCorrelationId(context);
        
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId
        }))
        {
            context.Response.Headers[CorrelationIdHeaderName] = correlationId;
            
            _logger.LogDebug("Processing request with CorrelationId: {CorrelationId}", correlationId);
            
            await _next(context);
        }
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var correlationId) 
            && !string.IsNullOrWhiteSpace(correlationId))
        {
            return correlationId.ToString();
        }

        return Guid.NewGuid().ToString();
    }
}
