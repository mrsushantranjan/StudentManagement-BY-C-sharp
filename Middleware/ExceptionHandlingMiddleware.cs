using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Exceptions;

namespace StudentManagement.Middleware;

/// <summary>
/// Global exception handling middleware that translates exceptions into RFC 7807 ProblemDetails JSON responses.
/// </summary>
public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IHostEnvironment environment)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            logger.LogWarning("Request was cancelled by the client: {Method} {Path}",
                context.Request.Method, context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.Items[CorrelationIdMiddleware.ItemKey]?.ToString() ?? context.TraceIdentifier;

        var (statusCode, title, detail) = exception switch
        {
            ApiException apiEx => ((int)apiEx.StatusCode, apiEx.Title, apiEx.Message),
            _ => ((int)HttpStatusCode.InternalServerError,
                "Internal Server Error",
                environment.IsDevelopment()
                    ? exception.Message
                    : "An unexpected error occurred while processing your request.")
        };

        if (statusCode >= 500)
        {
            logger.LogError(exception,
                "Unhandled exception for {Method} {Path} [CorrelationId: {CorrelationId}]",
                context.Request.Method,
                context.Request.Path,
                correlationId);
        }
        else
        {
            logger.LogWarning(
                "Handled API exception ({StatusCode}) for {Method} {Path}: {Message} [CorrelationId: {CorrelationId}]",
                statusCode,
                context.Request.Method,
                context.Request.Path,
                exception.Message,
                correlationId);
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path,
            Type = $"https://httpstatuses.com/{statusCode}"
        };

        problemDetails.Extensions["correlationId"] = correlationId;
        problemDetails.Extensions["timestamp"] = DateTime.UtcNow;

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, JsonOptions));
    }
}

