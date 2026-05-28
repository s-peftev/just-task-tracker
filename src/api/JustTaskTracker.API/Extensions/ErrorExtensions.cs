using JustTaskTracker.Domain.Common.Enums;
using JustTaskTracker.Domain.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace JustTaskTracker.API.Extensions;

public static class ErrorExtensions
{
    public static IActionResult CreateErrorResponse(this Error error)
    {
        return error.Type switch
        {
            ErrorType.Validation => new BadRequestObjectResult(error),
            ErrorType.Conflict => new ConflictObjectResult(error),
            ErrorType.Unauthorized => new UnauthorizedObjectResult(error),
            ErrorType.NotFound => new NotFoundObjectResult(error),
            ErrorType.Business => new UnprocessableEntityObjectResult(error),
            ErrorType.Forbidden => new ObjectResult(error) { StatusCode = 403 },
            ErrorType.InternalServerError => new ObjectResult(error) { StatusCode = 500 },
            ErrorType.ServiceUnavailable => new ObjectResult(error) { StatusCode = 503 },
            _ => new ObjectResult(error) { StatusCode = 500 }
        };
    }
}
