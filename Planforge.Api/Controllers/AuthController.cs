using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Planforge.Application.DTOs;
using Planforge.Infrastructure.Identity;

namespace Planforge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController: ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;

    public AuthController(UserManager<ApplicationUser> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest loginRequest)
    {
        var user = await _userManager.FindByEmailAsync(loginRequest.Email);
        if (user == null || user.IsDeleted || !await _userManager.CheckPasswordAsync(user, loginRequest.Password))
        {
            return Unauthorized();
        }
        
        return Ok(new LoginResponse(await GenerateJwtToken(user)));
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest registerRequest)
    {
        var user = await _userManager.FindByEmailAsync(registerRequest.Email);
        if (user != null)
        {
            return BadRequest("User already exists");
        }

        user = new ApplicationUser()
        {
            UserName = registerRequest.Email,
            Email = registerRequest.Email
        };

        var result = await _userManager.CreateAsync(user, registerRequest.Password);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }
        
        await _userManager.AddToRoleAsync(user, "Admin");
        return Ok(new  RegisterResponse(await  GenerateJwtToken(user)));
    }

    [HttpPost("delete")]
    public async Task<IActionResult> Delete()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
        if (userId == null)
        {
            return Unauthorized();
        }
        
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }
        
        //soft delete
        user.IsDeleted = true;
        user.DeletedOn = DateTime.UtcNow;
        
        //invalidate login
        user.LockoutEnd = DateTimeOffset.MaxValue;
        
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }
        
        return Ok("Account has been deleted");
    }

    private async Task<string> GenerateJwtToken(ApplicationUser user)
    {
        var jwtKey = _configuration["Jwt:Key"];
        var jwtIssuer = _configuration["Jwt:Issuer"];

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        
        var roles = await _userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: null,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds);
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
