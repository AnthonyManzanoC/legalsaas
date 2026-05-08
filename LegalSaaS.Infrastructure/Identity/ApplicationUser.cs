using Microsoft.AspNetCore.Identity;

namespace LegalSaaS.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public int TenantId { get; set; }
        public string? FullName { get; set; }
        public string? PhotoUrl { get; set; }
    }
}