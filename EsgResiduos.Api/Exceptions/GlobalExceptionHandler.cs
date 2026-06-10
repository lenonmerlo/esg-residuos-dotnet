using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace EsgResiduos.Api.Exceptions;

// Centraliza erros da API no formato ProblemDetails (RFC 7807), evitando respostas inconsistentes.
public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        int status;
        string message;

        if (exception is AppException appException)
        {
            status = appException.StatusCode;
            message = appException.Message;
        }
        else
        {
            status = 500;
            message = "Ocorreu um erro inesperado.";
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
