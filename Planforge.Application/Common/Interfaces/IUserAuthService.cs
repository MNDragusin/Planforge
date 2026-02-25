using Planforge.Application.DTOs;

namespace Planforge.Application.Common.Interfaces;

public interface IUserAuthService
{
    Task<IServiceResult> Login(LoginRequest loginRequest);
    Task<IServiceResult> DeactivateAccount(string userId);
    Task<IServiceResult> Register(RegisterRequest request);
}