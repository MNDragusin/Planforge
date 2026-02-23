namespace Planforge.Application.DTOs;

public record LoginRequest(string Email, string Password);
public record LoginResponse(string Token);
public record RegisterRequest(string Name, string Email, string Password);
public record RegisterResponse(string Token);