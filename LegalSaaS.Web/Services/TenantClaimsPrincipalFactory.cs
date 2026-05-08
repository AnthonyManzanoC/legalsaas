using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using LegalSaaS.Infrastructure.Data;
using LegalSaaS.Infrastructure.Identity;
using System.Threading.Tasks;

namespace LegalSaaS.Web.Services
{
    public class TenantClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
    {
        private readonly AppDbContext _db;

        public TenantClaimsPrincipalFactory(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor,
            AppDbContext db)
            : base(userManager, roleManager, optionsAccessor)
        {
            _db = db;
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);
            identity.AddClaim(new Claim("tenant_id", user.TenantId.ToString()));

            var tenantName = user.TenantId == 0
                ? "LegalSaaS Platform"
                : await _db.Tenants.IgnoreQueryFilters()
                    .Where(x => x.Id == user.TenantId)
                    .Select(x => x.Name)
                    .FirstOrDefaultAsync();

            if (!string.IsNullOrWhiteSpace(tenantName))
                identity.AddClaim(new Claim("tenant_name", tenantName));

            if (!string.IsNullOrWhiteSpace(user.FullName))
                identity.AddClaim(new Claim("full_name", user.FullName));

            return identity;
        }
    }
}
