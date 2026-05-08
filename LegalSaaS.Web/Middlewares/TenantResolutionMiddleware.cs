using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using LegalSaaS.Infrastructure.Data;
using LegalSaaS.Application.Interfaces;
using LegalSaaS.Domain.Entities;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LegalSaaS.Web.Middlewares
{
    public class TenantResolutionMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantResolutionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, AppDbContext db, ITenantContext tenantContext)
        {
            var isSuperAdmin = context.User?.Identity?.IsAuthenticated == true &&
                               context.User.IsInRole("SuperAdmin");

            if (isSuperAdmin)
            {
                tenantContext.SetTenant(0, "LegalSaaS Platform", true);
                await _next(context);
                return;
            }

            var userTenantId = ResolveAuthenticatedTenantId(context);
            var subdomain = ResolveSubdomain(context.Request.Host.Host);
            Tenant? tenant;

            if (userTenantId > 0)
            {
                tenant = await db.Tenants.FirstOrDefaultAsync(x => x.Id == userTenantId);
            }
            else if (!string.IsNullOrWhiteSpace(subdomain))
            {
                tenant = await db.Tenants.FirstOrDefaultAsync(x => x.Subdomain == subdomain);
            }
            else
            {
                tenant = await db.Tenants.FirstOrDefaultAsync(x => x.Id == 1);
            }

            if (tenant is null)
            {
                tenantContext.SetTenant(
                    0,
                    !string.IsNullOrWhiteSpace(subdomain) ? subdomain : "LegalSaaS",
                    isTenantActive: false,
                    tenantBlockReason: "Tenant no encontrado.");

                await _next(context);
                return;
            }

            tenantContext.SetTenant(
                tenant.Id,
                tenant.Name,
                isTenantActive: tenant.IsActive,
                subscriptionEndsAt: tenant.TrialEndsAt,
                tenantBlockReason: ResolveBlockReason(tenant));

            await _next(context);
        }

        private static int ResolveAuthenticatedTenantId(HttpContext context)
        {
            if (context.User?.Identity?.IsAuthenticated != true)
                return 0;

            var value = context.User.FindFirstValue("tenant_id");
            return int.TryParse(value, out var tenantId) ? tenantId : 0;
        }

        private static string? ResolveSubdomain(string rawHost)
        {
            var host = rawHost.Trim().ToLowerInvariant();
            if (IPAddress.TryParse(host, out _))
                return null;

            var parts = host.Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 3 && parts[0] != "www")
                return parts[0];

            if (parts.Length == 2 &&
                parts[1].Equals("localhost", StringComparison.OrdinalIgnoreCase) &&
                parts[0] != "www")
            {
                return parts[0];
            }

            return null;
        }

        private static string? ResolveBlockReason(Tenant tenant)
        {
            if (!tenant.IsActive)
                return "El estudio esta inactivo.";

            if (tenant.TrialEndsAt.HasValue && tenant.TrialEndsAt.Value < DateTime.UtcNow)
                return "La suscripcion anual vencio.";

            return null;
        }
    }
}
