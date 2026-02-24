namespace Planforge.Application.Common.Interfaces;

public class ICurrentTenant
{
    Guid? OrganizationId { get; }
    Guid? UserId { get; }
}