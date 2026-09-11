using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Planforge.Infrastructure.Persistence;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        string connectionString = "Host=localhost;Port=5432;Database=planforge-postgres;Username={usr};Password=postgres";
        if (OperatingSystem.IsMacOS())
        {
            connectionString = connectionString.Replace("{usr}", Environment.UserName);
        }
        else
        {
            connectionString = connectionString.Replace("{usr}", "postgres");
        }

        optionsBuilder.UseNpgsql(connectionString);
        return new AppDbContext(optionsBuilder.Options);
    }
}