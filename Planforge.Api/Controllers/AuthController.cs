using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Planforge.Application.Common.Interfaces;
using Planforge.Application.DTOs;
using Planforge.Infrastructure.Identity;
using Planforge.Infrastructure.Persistence;

namespace Planforge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController: ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IUserAuthService _userAuthService;

    public AuthController(UserManager<ApplicationUser> userManager, IConfiguration configuration,  AppDbContext context, IUserAuthService userAuthService)
    {
        _userManager = userManager;
        _configuration = configuration;
        _context = context;
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
            return Unauthorized();
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
            if (registerRespons.Errors != null)
            {
                return BadRequest(registerRespons.Errors);
            }
            
            return BadRequest(registerRespons.Result);
        }
        return Ok(new  RegisterResponse(registerRespons.Result!));
    }

    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpPost("deactivate")]
    [Authorize]
    public async Task<IActionResult> DeactivateAccount()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }
        var result = await _userAuthService.DeactivateAccount(userId);
        
        if (!result.IsSuccessful)
        {
            if (result.Errors != null)
            {
                return BadRequest(result.Errors);
            }
            
            return NotFound();
        }
        
        return Ok("Account has been deleted");
    }
}
