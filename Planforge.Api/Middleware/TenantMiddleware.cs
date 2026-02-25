using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Planforge.Infrastructure.Persistence;

namespace Planforge.Api.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext db)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userIdClaim == null || !context.User.Identity.IsAuthenticated)
        {
            await _next(context);
            return;
        }
        
        if (!context.Request.Headers.TryGetValue("X-Organization-Id", out var orgIdHeader))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Organization header is missing");
            return;
        }

        if (!Guid.TryParse(orgIdHeader, out var orgId) || !Guid.TryParse(userIdClaim, out var userId))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }
        
        var isMember = await db.Memberships.AnyAsync(x => x.OrganizationId == orgId && x.UserId == userId);

        if (!isMember)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Not a member of this organization");
            return;
        }
        
        await _next(context);
    }
}