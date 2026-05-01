using Microsoft.AspNetCore.Mvc;

namespace GeorgeStore.Common;

public abstract class ApiControllerBase : ControllerBase
{
    protected ActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
            return Ok();

        return ProblemFromError(result.Error);
    }
    protected ActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return Ok(result.Value);

        return ProblemFromError(result.Error);
    }

    protected ObjectResult ProblemFromError(Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };

        var result = Problem(
            title: error.Title,
            detail: error.Message,
            statusCode: statusCode,
            type: error.Code
        );

        if (result is ObjectResult objectResult &&
            objectResult.Value is ProblemDetails pd)
        {
            pd.Extensions["code"] = error.Code;
        }

        return result;
    }
}