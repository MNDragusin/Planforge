using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Planforge.Application.Common.Enums;
using Planforge.Application.Common.Interfaces;
using Planforge.Application.DTOs;
using Planforge.Infrastructure.Identity;
using Planforge.Infrastructure.Persistence;

namespace Planforge.Application.Services;

public class UserAuthService : IUserAuthService
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly IOrganizationService _organizationService;

    public UserAuthService(AppDbContext context, UserManager<ApplicationUser> userManager, IConfiguration configuration, IOrganizationService organizationService)
    {
        _context = context;
        _userManager = userManager;
        _configuration = configuration;
        _organizationService = organizationService;
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
            return ServiceResult<LoginResponse>.Failure("User was deleted", ServiceErrorType.NotFound);
        }

        var memberships = await _context.Memberships.Where(m => m.UserId == user.Id)
            .Select(m => new MembershipDto(m.OrganizationId, m.Role)).ToListAsync();

        var accessToken = await GenerateJwtToken(user);
        var refreshToken = await GenerateAndStoreRefreshToken(user.Id);

        return ServiceResult<LoginResponse>.Success(new LoginResponse(accessToken, refreshToken.Token, memberships));
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

        var organizationResponse = await _organizationService.CreateOrganization(user.DisplayName, user.Id);
        if (!organizationResponse.IsSuccessful)
        {
            return ServiceResult<RegisterResponse>.Failure("Internal Error", ServiceErrorType.InternalError, organizationResponse.Errors);
        }

        await _context.SaveChangesAsync();

        if (!result.Succeeded)
        {
            return ServiceResult<RegisterResponse>.Failure("Bad Request", ServiceErrorType.BadRequest, result.Errors);
        }

        await _userManager.AddToRoleAsync(user, "Admin");
        return ServiceResult<RegisterResponse>.Success(new RegisterResponse(await GenerateJwtToken(user),
            organizationResponse.Result!));
    }

    public async Task<IServiceResult<UserDetails>> GetActiveUser(string emailAddress)
    {
        var user = await _userManager.FindByEmailAsync(emailAddress);
        if (user == null)
        {
            return ServiceResult<UserDetails>.Failure("Not found", ServiceErrorType.NotFound);
        }

        return ServiceResult<UserDetails>.Success(new UserDetails(user.Id, user.DisplayName, user.Email));
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
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);

    }

    private async Task<RefreshToken> GenerateAndStoreRefreshToken(Guid userId)
    {
        var refreshToken = new RefreshToken()
        {
            UserId = userId,
            Token = GenerateRefreshToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(5)
        };

        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();

        return refreshToken;
    }

    public async Task<IServiceResult<bool>> Logout(RefreshRequest request)
    {
        _context.RefreshTokens.FirstOrDefaultAsync(t => t.)
    }

    public async Task<IServiceResult<RefreshResponse>> Refresh(RefreshRequest request)
    {
        RefreshToken? existingToken = await _context.RefreshTokens.Include(t => t.User).FirstOrDefaultAsync(t => t.Token == request.RefreshToken);

        if (existingToken == null)
        {
            return ServiceResult<RefreshResponse>.Failure("Invalid refresh token", ServiceErrorType.Unauthorized);
        }

        if (!existingToken.IsActive)
        {
            //if token is revoked -> revoke all refreshTokens for the current user
            if (existingToken.RevokedAt != null)
            {
                await RevokeAllRefreshTokens(existingToken);
            }

            return ServiceResult<RefreshResponse>.Failure("Refresh token is no longer valid", ServiceErrorType.Unauthorized);
        }

        var user = await _userManager.FindByIdAsync(existingToken.UserId.ToString());

        if (user == null || user.IsDeleted)
        {
            return ServiceResult<RefreshResponse>.Failure("User not found", ServiceErrorType.NotFound);
        }

        //revoke old - issue new
        var newRefreshToken = await GenerateAndStoreRefreshToken(existingToken.UserId);
        existingToken.RevokedAt = DateTime.UtcNow;
        existingToken.ReplacedByToken = newRefreshToken.Token;

        var newAccessToken = await GenerateJwtToken(existingToken.User);

        return ServiceResult<RefreshResponse>.Success(new RefreshResponse(newAccessToken, newRefreshToken.Token));
    }

    private async Task RevokeAllRefreshTokens(RefreshToken existingTokens)
    {
        var tokens = existingTokens.User.RefreshTokens;

        foreach (var t in tokens)
        {
            if (t.RevokedAt != null)
            {
                continue;
            }

            t.RevokedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }
}