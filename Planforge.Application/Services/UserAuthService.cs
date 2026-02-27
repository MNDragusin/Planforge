using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Planforge.Application.Common.Enums;
using Planforge.Application.Common.Interfaces;
using Planforge.Application.DTOs;
using Planforge.Domain.Entities;
using Planforge.Domain.Enums;
using Planforge.Infrastructure.Identity;
using Planforge.Infrastructure.Persistence;

namespace Planforge.Application.Services;

public class UserAuthService : IUserAuthService
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    
    public UserAuthService(AppDbContext context, UserManager<ApplicationUser> userManager, IConfiguration configuration)
    {
        _context = context;
        _userManager = userManager;
        _configuration = configuration;
    }

    public async Task<IServiceResult<LoginResponse>> Login(LoginRequest loginRequest)
    {
        var user = await _userManager.FindByEmailAsync(loginRequest.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, loginRequest.Password))
        {
            return ServiceResult<LoginResponse>.Failure("Invalid username or password", ServiceErrorType.BadRequest);
        }

        if (user.IsDeleted)
        {
            return ServiceResult<LoginResponse>.Failure("User is deleted", ServiceErrorType.NotFound);
        }
        
        return ServiceResult<LoginResponse>.Success(new LoginResponse(await GenerateJwtToken(user)));
    }
    
    public async Task<IServiceResult<RegisterResponse>> Register(RegisterRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user != null)
        {
            return ServiceResult<RegisterResponse>.Failure("User already exists", ServiceErrorType.BadRequest);
        }

        user = new ApplicationUser()
        {
            UserName = request.Email,
            Email = request.Email
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        var org = new Organization(request.Name + "' Workspace");
        _context.Organizations.Add(org);
        
        var membership = new Membership(user.Id, org.Id, OrganizationRole.Owner);
        _context.Memberships.Add(membership);
        
        await _context.SaveChangesAsync();
        
        if (!result.Succeeded)
        {
            return ServiceResult<RegisterResponse>.Failure("Bad Request", ServiceErrorType.BadRequest, result.Errors);
        }
        
        await _userManager.AddToRoleAsync(user, "Admin");
        return ServiceResult<RegisterResponse>.Success(new RegisterResponse(await GenerateJwtToken(user)));
    }
    
    public async Task<IServiceResult<bool>> DeactivateAccount(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return ServiceResult<bool>.Failure("Not found", ServiceErrorType.NotFound);
        }
        
        //soft delete
        user.IsDeleted = true;
        user.DeletedOn = DateTime.UtcNow;
        
        //invalidate login
        user.LockoutEnd = DateTimeOffset.MaxValue;
        
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return ServiceResult<bool>.Failure("Internal Server Error", ServiceErrorType.InternalError, result.Errors);    
        }
        
        return ServiceResult<bool>.Success(true);
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