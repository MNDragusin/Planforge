using Microsoft.Extensions.Configuration;
using Planforge.Application.Common.Enums;
using Planforge.Application.Common.Interfaces;
using Planforge.Application.DTOs;
using Planforge.Domain.Entities;
using Planforge.Domain.Enums;
using Planforge.Infrastructure.Persistence;

namespace Planforge.Application.Services;

public class OrganizationService
{
    private readonly AppDbContext _context;
    private IConfiguration _configuration;

    public OrganizationService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<IServiceResult<OrganizationDto>> CreateOrganization(string name, Guid ownerId)
    {
        var newOrganization = new Organization(name + "' Workspace");
        var membership = AddMember_Internal(ownerId, newOrganization.Id, OrganizationRole.Owner);
        newOrganization.Members.Add(membership);

        _context.Organizations.Add(newOrganization);
        var result = await _context.SaveChangesAsync();
        return result == 0
            ? ServiceResult<OrganizationDto>.Failure("Internal error", ServiceErrorType.InternalError)
            : ServiceResult<OrganizationDto>.Success();
    }

    public async Task<IServiceResult<MembershipDto>> AddMember(Guid userId, Guid organizationId,
        OrganizationRole role = OrganizationRole.Viewer)
    {
        var membership = AddMember_Internal(userId, organizationId, role);

        var result = await _context.SaveChangesAsync();
        return result == 0
            ? ServiceResult<MembershipDto>.Failure("Internal error", ServiceErrorType.InternalError)
            : ServiceResult<MembershipDto>.Success();
    }

    private Membership AddMember_Internal(Guid userId, Guid organizationId, OrganizationRole role)
    {
        var membership = new Membership(userId, organizationId, role);
        var result = _context.Memberships.Add(membership);

        return result.Entity;
    }
}