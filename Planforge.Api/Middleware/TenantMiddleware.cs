namespace Planforge.Api.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity.IsAuthenticated)
        {
            var orgId = context.Request.Headers["X-Organization-Id"].FirstOrDefault();
            if (!string.IsNullOrEmpty(orgId))
            {
                context.Items["OrganizationId"] = Guid.Parse(orgId);
            }
        }
        
        await _next(context);
    }
}