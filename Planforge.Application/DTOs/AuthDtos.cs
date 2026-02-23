namespace Planforge.Application.DTOs;

public record LoginRequest(string Email, string Password);
public record LoginResponse(string Token);
public record RegisterRequest(string Email, string Password);
public record RegisterResponse(string Token);