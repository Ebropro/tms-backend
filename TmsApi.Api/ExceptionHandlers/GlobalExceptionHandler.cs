using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace TmsApi.Api.ExceptionHandlers;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken ct)
    {
        
        // Returning false tells the exception-handling middleware we didn't
        // handle it, so it can fall back to its own default behavior.
        if (httpContext.Response.HasStarted)
        {
            logger.LogWarning(
                "The response has already started, cannot write ProblemDetails.");
            return false;
        }

        var (status, title, detail, errors) = exception switch
        {
            ValidationException ve => (
                StatusCodes.Status400BadRequest,
                "Validation failed",
                "One or more fields are invalid. See errors for details.",
                (IDictionary<string, string[]>?)ve.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray())),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Server error",
                $"An unexpected error occurred. Trace ID: {httpContext.TraceIdentifier}",
                null)
        };

        if (status == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                exception,
                "Unhandled exception (trace={TraceId})",
                httpContext.TraceIdentifier);
        }

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        if (errors is not null)
            problem.Extensions["errors"] = errors;

        httpContext.Response.StatusCode = status;
        // way to keep "application/problem+json" with no charset suffix.
        await httpContext.Response.WriteAsJsonAsync(
            problem,
            options: null,
            contentType: "application/problem+json",
            cancellationToken: ct);

        return true;
    }
}
