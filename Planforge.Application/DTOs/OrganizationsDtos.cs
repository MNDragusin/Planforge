namespace Planforge.Application.DTOs;

public record OrganizationDto();
public record MembershipDto(Guid OrgId, string role);