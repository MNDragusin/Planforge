using Planforge.Application.DTOs;
using Planforge.Domain.Enums;

namespace Planforge.Application.Common.Interfaces;

public interface IOrganizationService
{
    public Task<IServiceResult<OrganizationDto>> CreateOrganization(string name, Guid ownerId);

    public Task<IServiceResult<MembershipDto>> AddMember(Guid userId, Guid organizationId,
        OrganizationRole role = OrganizationRole.Viewer);
}