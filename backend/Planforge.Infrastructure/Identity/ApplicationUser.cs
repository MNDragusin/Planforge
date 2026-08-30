namespace Planforge.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
    
public class ApplicationUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
}