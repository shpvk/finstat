using Finstat.Domain;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Finstat.Api;

public class DomainExceptionHandler(IProblemDetailsService problemDetails) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        var (status, title, detail) = exception switch
        {
            DomainException => (StatusCodes.Status400BadRequest, "Порушено бізнес-правило", exception.Message),
            BadHttpRequestException bad => (bad.StatusCode, "Некоректний запит", "Некоректний запит"),
            _ => (0, null, null),
        };
        if (title is null)
            return false;

        context.Response.StatusCode = status;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = detail,
            },
        });
    }
}
