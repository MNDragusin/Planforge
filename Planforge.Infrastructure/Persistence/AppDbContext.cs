using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Planforge.Domain.Entities;
using Planforge.Infrastructure.Identity;

namespace Planforge.Infrastructure.Persistence;

public class AppDbContext: IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public DbSet<Membership> Memberships => Set<Membership>();
    public DbSet<Organization> Organizations => Set<Organization>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Membership>(entity =>
        {
            // Composite primary key
            entity.HasKey(x => new { x.UserId, x.OrganizationId });
            
            // relation Membership -> ApplicationUser (Identity)
            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // relation Membership -> Organization
            entity.HasOne<Organization>()
                .WithMany(x => x.Members)
                .HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}