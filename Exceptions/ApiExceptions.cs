using System.Net;

namespace StudentManagement.Exceptions;

/// <summary>
/// Base exception for domain and API errors that map cleanly to HTTP status codes.
/// </summary>
public abstract class ApiException(string message, HttpStatusCode statusCode, string title) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
    public string Title { get; } = title;
}

public sealed class NotFoundException(string message)
    : ApiException(message, HttpStatusCode.NotFound, "Resource Not Found");

public sealed class ConflictException(string message)
    : ApiException(message, HttpStatusCode.Conflict, "Resource Conflict");

public sealed class BadRequestException(string message)
    : ApiException(message, HttpStatusCode.BadRequest, "Invalid Request");

