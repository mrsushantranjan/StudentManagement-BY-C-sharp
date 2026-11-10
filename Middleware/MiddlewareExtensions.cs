namespace StudentManagement.Middleware;

/// <summary>
/// Extension methods for registering the custom middleware pipeline in order.
/// </summary>
public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseApiPipelineMiddlewares(this IApplicationBuilder app)
    {
        // Order matters: correlation ID first, then logging, then exception handling.
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<RequestLoggingMiddleware>();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        return app;
    }
}

