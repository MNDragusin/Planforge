using System.Security.Claims;
using Planforge.Application.Common.Interfaces;

namespace Planforge.Api.Services;

public class CurrentTenantService : ICurrentTenant
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentTenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? CurrentTenantId
    {
        get
        {
            var orgId = _httpContextAccessor.HttpContext?.Request.Headers["OrganizationId"].FirstOrDefault();
            return Guid.TryParse(orgId, out Guid id) ? id : null;
        }
    }

    public Guid? UserId
    {
        get
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userId, out Guid id) ? id : null;
        }
    }
}
