using System.Net;
using System.Text.Json;
using FluentValidation;

namespace FileStorage.API.Middleware;

/// <summary>
/// Simple error-handling middleware (basic version).
/// </summary>
public class ProblemDetailsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ProblemDetailsMiddleware> _logger;

    public ProblemDetailsMiddleware(RequestDelegate next, ILogger<ProblemDetailsMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";
        int statusCode = (int)HttpStatusCode.InternalServerError;
        string title = "An unexpected error occurred.";
        object errors = null;

        switch (ex)
        {
            case ValidationException validationEx:
                statusCode = (int)HttpStatusCode.BadRequest;
                title = "Validation failed.";
                errors = validationEx.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );
                break;

            case FileNotFoundException:
                statusCode = (int)HttpStatusCode.NotFound;
                title = "Resource not found.";
                break;

            case UnauthorizedAccessException:
                statusCode = (int)HttpStatusCode.Forbidden;
                title = "Access denied.";
                break;

            case InvalidOperationException:
                statusCode = (int)HttpStatusCode.BadRequest;
                title = "Invalid operation.";
                break;
        }

        context.Response.StatusCode = statusCode;

        _logger.LogError(ex, "Error occurred: {Message}", ex.Message);

        var response = new
        {
            status = statusCode,
            title = title,
            message = ex.Message,
            path = context.Request.Path,
            errors = errors
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
