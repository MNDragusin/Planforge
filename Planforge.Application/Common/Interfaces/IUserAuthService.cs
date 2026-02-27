using Planforge.Application.DTOs;

namespace Planforge.Application.Common.Interfaces;

public interface IUserAuthService
{
    Task<IServiceResult<LoginResponse>> Login(LoginRequest loginRequest);
    Task<IServiceResult<bool>> DeactivateAccount(string userId);
    Task<IServiceResult<RegisterResponse>> Register(RegisterRequest request);
}