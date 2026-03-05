using Planforge.Domain.Enums;

namespace Planforge.Application.DTOs;

public record OrganizationDto();
public record MembershipDto(Guid OrgId, OrganizationRole role);