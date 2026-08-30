using Microsoft.AspNetCore.Mvc;
using Planforge.Application.Common.Enums;
using Planforge.Application.Common.Interfaces;

namespace Planforge.Api.Controllers;

public class BaseCustomController: ControllerBase
{
    protected IActionResult MapToErrorActionResult<T>(IServiceResult<T> result)
    {
        switch (result.ErrorType)
        {
            case ServiceErrorType.BadRequest:
                return BadRequest(result.Errors);
            case ServiceErrorType.NotFound:
                return NotFound(result.Message);
                break;
            case ServiceErrorType.Unauthorized:
                return Unauthorized(result.Message);
                break;
            case ServiceErrorType.InternalError:
                return ValidationProblem(result.Message);
                break;
            default:
                break;
        }

        throw new NotImplementedException();
    }
}