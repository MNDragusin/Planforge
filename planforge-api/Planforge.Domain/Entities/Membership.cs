using Planforge.Domain.Enums;

namespace Planforge.Domain.Entities;

public class Membership
{   
    public Guid UserId { get; set; }
    public Guid OrganizationId { get; set; }
    public OrganizationRole Role { get; set; }
    
    private Membership(){}
    
    public Membership(Guid userId, Guid organizationId, OrganizationRole role)
    {
        UserId = userId;
        OrganizationId = organizationId;
        Role = role;
    }
}
