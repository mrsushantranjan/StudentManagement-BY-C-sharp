using System.Diagnostics;

namespace StudentManagement.Middleware;

/// <summary>
/// Logs incoming HTTP requests, completion status codes, and execution duration in milliseconds.
/// </summary>
public sealed class RequestLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var correlationId = context.Items[CorrelationIdMiddleware.ItemKey]?.ToString() ?? context.TraceIdentifier;

        context.Response.OnStarting(() =>
        {
            context.Response.Headers["X-Response-Time-Ms"] = stopwatch.ElapsedMilliseconds.ToString();
            return Task.CompletedTask;
        });

        logger.LogInformation(
            "Handling {Method} {Path}{Query} [CorrelationId: {CorrelationId}]",
            context.Request.Method,
            context.Request.Path,
            context.Request.QueryString,
            correlationId);

        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();
            logger.LogInformation(
                "Completed {Method} {Path} with {StatusCode} in {ElapsedMs}ms [CorrelationId: {CorrelationId}]",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                correlationId);
        }
    }
}

