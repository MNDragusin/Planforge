namespace Planforge.Application.DTOs;

public record LoginRequest(string Email, string Password);
public record LoginResponse(string Token, List<MembershipDto> memberships);
public record RegisterRequest(string Name, string Email, string Password);
public record RegisterResponse(string Token, MembershipDto membership);
