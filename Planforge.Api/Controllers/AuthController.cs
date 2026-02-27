using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Planforge.Application.Common.Enums;
using Planforge.Application.Common.Interfaces;
using Planforge.Application.DTOs;

namespace Planforge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController: ControllerBase
{
    private readonly IUserAuthService _userAuthService;

    public AuthController(IUserAuthService userAuthService)
    {
        _userAuthService = userAuthService;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginRequest loginRequest)
    {
        var result = await _userAuthService.Login(loginRequest);
        if (!result.IsSuccessful)
        {
            return MapToErrorActionResult(result);
        }
        
        return Ok(result.Result);
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(RegisterRequest registerRequest)
    {
        var registerRespons = await _userAuthService.Register(registerRequest);
        if (!registerRespons.IsSuccessful)
        {
            return MapToErrorActionResult(registerRespons);
        }
        
        return Ok(registerRespons.Result);
    }

    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpPost("deactivate")]
    [Authorize]
    public async Task<IActionResult> DeactivateAccount()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }
        var result = await _userAuthService.DeactivateAccount(userId);
        
        if (!result.IsSuccessful)
        {
            return MapToErrorActionResult(result);
        }
        
        return Ok("Account has been deleted");
    }

    private IActionResult MapToErrorActionResult<T>(IServiceResult<T> result)
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
