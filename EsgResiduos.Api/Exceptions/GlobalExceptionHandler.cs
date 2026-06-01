using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace EsgResiduos.Api.Exceptions;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger = logger;
    private static readonly Action<ILogger, Exception?> UnhandledExceptionLog = LoggerMessage.Define(
        LogLevel.Error,
        new EventId(1, nameof(GlobalExceptionHandler)),
        "Unhandled exception");

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        (int status, string? message) = exception switch
        {
            AppException ex => (ex.StatusCode, ex.Message),
            _ => (500, "An unexpected error occurred.")
        };

        if (status == 500)
        {
            UnhandledExceptionLog(_logger, exception);
        }

        ProblemDetails problem = new()
        {
            Status = status,
            Title = message,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}