using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Planforge.Application.DTOs;
using Planforge.Infrastructure.Persistence;

namespace Planforge.Tests;

public class PlanForgeWebAppFactory: WebApplicationFactory<Program>, IAsyncLifetime
{
    public readonly string TestUser = "test";
    public readonly string TestEmail = "test@test.com";
    public readonly string TestPassword = "iop890IOP*()";
    
    public string Token => LoggedInUser.Token;
    public LoginResponse LoggedInUser { get; set; } = default!;
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
    }
    
    public async Task InitializeAsync()
    {  
        await CleanDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        await CleanDatabaseAsync();
    }
    
    private async Task CleanDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Users.RemoveRange(db.Users);
        await db.SaveChangesAsync();
    }
}