using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
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

    public async Task<IServiceResult> Login(LoginRequest loginRequest)
    {
        var user = await _userManager.FindByEmailAsync(loginRequest.Email);
        if (user == null || user.IsDeleted || !await _userManager.CheckPasswordAsync(user, loginRequest.Password))
        {
            return new ServiceResult(false);;
        }
        
        return new ServiceResult(true, await GenerateJwtToken(user));
    }

    public async Task<IServiceResult> DeactivateAccount(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new ServiceResult(false);;
        }
        
        //soft delete
        user.IsDeleted = true;
        user.DeletedOn = DateTime.UtcNow;
        
        //invalidate login
        user.LockoutEnd = DateTimeOffset.MaxValue;
        
        var result = await _userManager.UpdateAsync(user);
        return new ServiceResult(result.Succeeded, string.Empty, result.Errors);
    }

    public async Task<IServiceResult> Register(RegisterRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user != null)
        {
            return new ServiceResult(false, "User already exists");
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
            return new ServiceResult(false, null,result.Errors);
        }
        
        await _userManager.AddToRoleAsync(user, "Admin");
        return new ServiceResult(true, await  GenerateJwtToken(user));
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