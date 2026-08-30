using Microsoft.Extensions.Configuration;
using Planforge.Application.Common.Enums;
using Planforge.Application.Common.Interfaces;
using Planforge.Application.DTOs;
using Planforge.Application.Mapping;
using Planforge.Domain.Entities;
using Planforge.Domain.Enums;
using Planforge.Infrastructure.Persistence;

namespace Planforge.Application.Services;

public class OrganizationService : IOrganizationService
{
    private readonly AppDbContext _context;
    private IConfiguration _configuration;

    public OrganizationService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<IServiceResult<MembershipDto>> CreateOrganization(string name, Guid ownerId)
    {
        var newOrganization = new Organization(name + "' Workspace");
        var membership = AddMember_Internal(ownerId, newOrganization.Id, OrganizationRole.Owner);
        newOrganization.Members.Add(membership);

        _context.Organizations.Add(newOrganization);
        var result = await _context.SaveChangesAsync();
        return result == 0
            ? ServiceResult<MembershipDto>.Failure("Internal error", ServiceErrorType.InternalError)
            : ServiceResult<MembershipDto>.Success(membership.ToDto());
    }

    public async Task<IServiceResult<MembershipDto>> AddMember(Guid userId, Guid organizationId,
        OrganizationRole role = OrganizationRole.Viewer)
    {
        var membership = AddMember_Internal(userId, organizationId, role);

        var result = await _context.SaveChangesAsync();
        return result == 0
            ? ServiceResult<MembershipDto>.Failure("Internal error", ServiceErrorType.InternalError)
            : ServiceResult<MembershipDto>.Success(membership.ToDto());
    }

    public async Task<IServiceResult<List<MembershipDto>>> GetAllUser(Guid organizationId)
    {
        var memberships=  _context.Memberships.Where(x => x.OrganizationId == organizationId);
        List<MembershipDto> dtoList = new List<MembershipDto>();
        foreach (var membership in memberships)
        {
            dtoList.Add(membership.ToDto());
        }
        
        return ServiceResult<List<MembershipDto>>.Success(dtoList);
    }

    private Membership AddMember_Internal(Guid userId, Guid organizationId, OrganizationRole role)
    {
        var membership = new Membership(userId, organizationId, role);
        var result = _context.Memberships.Add(membership);

        return result.Entity;
    }
}