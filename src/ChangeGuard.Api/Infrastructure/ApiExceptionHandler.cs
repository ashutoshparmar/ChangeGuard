using ChangeGuard.Application.ChangeRequests;
using ChangeGuard.Domain.ChangeRequests;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ChangeGuard.Api.Infrastructure;

public sealed class ApiExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ApiExceptionHandler> _logger;

    public ApiExceptionHandler(ILogger<ApiExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title, detail) = exception switch
        {
            ChangeRequestNotFoundException => (
                StatusCodes.Status404NotFound,
                "Change request not found.",
                exception.Message),
            DuplicateChangeRequestException => (
                StatusCodes.Status409Conflict,
                "Duplicate change request.",
                exception.Message),
            DomainRuleViolationException => (
                StatusCodes.Status409Conflict,
                "Workflow rule prevented the operation.",
                exception.Message),
            ArgumentException => (
                StatusCodes.Status400BadRequest,
                "The request is invalid.",
                exception.Message),
            _ => (
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.",
                "The server could not complete the request.")
        };

        if (status >= StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(
                exception,
                "Unhandled exception for {Method} {Path}. Trace: {TraceId}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                httpContext.TraceIdentifier);
        }
        else
        {
            _logger.LogWarning(
                exception,
                "Request rejected with status {StatusCode}. Trace: {TraceId}",
                status,
                httpContext.TraceIdentifier);
        }

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };
        problem.Extensions["traceId"] = httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = status;
        httpContext.Response.ContentType = "application/problem+json";
        await httpContext.Response.WriteAsJsonAsync(
            problem,
            cancellationToken);

        return true;
    }
}
