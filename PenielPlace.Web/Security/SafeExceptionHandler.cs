using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace PenielPlace.Web.Security;

public sealed class SafeExceptionHandler : IExceptionHandler
{
    private readonly ILogger<SafeExceptionHandler> _logger;

    public SafeExceptionHandler(ILogger<SafeExceptionHandler> logger) => _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unhandled server error. RequestId: {RequestId}", httpContext.TraceIdentifier);

        if (httpContext.Response.HasStarted)
            return false;

        httpContext.Response.Clear();
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred.",
            Detail = "Please try again. If the problem continues, contact the system administrator.",
            Instance = httpContext.Request.Path
        };
        problem.Extensions["requestId"] = httpContext.TraceIdentifier;

        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}
