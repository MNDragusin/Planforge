using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Planforge.Domain.Entities;
using Planforge.Infrastructure.Identity;

namespace Planforge.Infrastructure.Persistence;

public class AppDbContext: IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public DbSet<Membership> Memberships => Set<Membership>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }
}