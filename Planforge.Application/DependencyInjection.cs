using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Planforge.Application.Common.Interfaces;
using Planforge.Application.Services;

namespace Planforge.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IUserAuthService, UserAuthService>();
        return services;
    }
}