namespace Planforge.Application.Common.Interfaces;

public class ICurrentTenant
{
    public Guid? OrganizationId { get; }
    public Guid? UserId { get; }
}