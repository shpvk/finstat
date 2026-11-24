using Finstat.Domain.Results;
using Microsoft.AspNetCore.Mvc;

namespace Finstat.Api.Controllers;

public abstract class AppControllerBase : ControllerBase
{
    protected ActionResult Failure(Error error)
    {
        if (error.Type == ErrorType.NotFound)
            return NotFound();
        return Problem(detail: error.Message, statusCode: StatusCodes.Status400BadRequest, title: "Порушено бізнес-правило");
    }
}
