using Planforge.Application.DTOs;
using Planforge.Domain.Entities;

namespace Planforge.Application.Mapping;

public static class OrganizationMapping
{
    public static OrganizationDto ToDto(this Organization organization)
    {
        return new OrganizationDto()
        {
            
        }
    }

    public static Organization ToEntity(this OrganizationDto dto)
    {
        
    }

    public static Membership ToEntity(this MembershipDto dto, Guid userId)
    {
        return new Membership(userId, dto.OrgId, dto.role);
    }

    public static MembershipDto ToDto(this Membership membership)
    {
        return new MembershipDto(membership.OrganizationId, membership.Role);
    }
}