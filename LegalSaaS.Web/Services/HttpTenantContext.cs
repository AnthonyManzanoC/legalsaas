using System.Security.Claims;
using LegalSaaS.Application.Interfaces;
using LegalSaaS.Domain.Entities;
using LegalSaaS.Infrastructure.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace LegalSaaS.Web.Services
{
    public class HttpTenantContext : ITenantContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly NavigationManager _navigationManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly SemaphoreSlim _resolutionLock = new(1, 1);
        private int? _forcedTenantId;
        private string? _forcedTenantName;
        private bool _forcedSuperAdmin;
        private bool? _forcedTenantActive;
        private DateTime? _forcedSubscriptionEndsAt;
        private string? _forcedTenantBlockReason;
        private bool _resolutionAttempted;

        public HttpTenantContext(
            IHttpContextAccessor httpContextAccessor,
            NavigationManager navigationManager,
            IServiceProvider serviceProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _navigationManager = navigationManager;
            _serviceProvider = serviceProvider;
        }

        public int TenantId
        {
            get
            {
                if (_forcedTenantId.HasValue) return _forcedTenantId.Value;
                if (IsSuperAdmin) return 0;
                var value = _httpContextAccessor.HttpContext?.User?.FindFirstValue("tenant_id");
                return int.TryParse(value, out var id) ? id : 0;
            }
        }

        public string? TenantName => _forcedTenantName ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue("tenant_name");

        public bool IsSuperAdmin =>
            _forcedSuperAdmin ||
            _httpContextAccessor.HttpContext?.User?.IsInRole("SuperAdmin") == true;

        public bool IsTenantActive => _forcedTenantActive ?? true;

        public DateTime? SubscriptionEndsAt => _forcedSubscriptionEndsAt;

        public bool IsSubscriptionExpired =>
            SubscriptionEndsAt.HasValue && SubscriptionEndsAt.Value < DateTime.UtcNow;

        public bool IsTenantBlocked =>
            !IsSuperAdmin &&
            IsTenantResolved &&
            (!IsTenantActive || IsSubscriptionExpired || (_forcedTenantId.HasValue && _forcedTenantId.Value <= 0));

        public string? TenantBlockReason =>
            _forcedTenantBlockReason ??
            (!IsTenantActive
                ? "El estudio esta inactivo."
                : IsSubscriptionExpired
                    ? "La suscripcion anual vencio."
                    : null);

        public bool IsTenantResolved => _forcedTenantId.HasValue || _resolutionAttempted;

        public async Task EnsureTenantResolvedAsync()
        {
            if (IsTenantResolved)
                return;

            await _resolutionLock.WaitAsync();
            try
            {
                if (IsTenantResolved)
                    return;

                await ResolveTenantAsync();
            }
            finally
            {
                _resolutionLock.Release();
            }
        }

        public void SetTenant(
            int tenantId,
            string? tenantName = null,
            bool isSuperAdmin = false,
            bool isTenantActive = true,
            DateTime? subscriptionEndsAt = null,
            string? tenantBlockReason = null)
        {
            _forcedTenantId = tenantId;
            _forcedTenantName = tenantName;
            _forcedSuperAdmin = isSuperAdmin;
            _forcedTenantActive = isTenantActive;
            _forcedSubscriptionEndsAt = subscriptionEndsAt;
            _forcedTenantBlockReason = tenantBlockReason;
            _resolutionAttempted = true;
        }

        private async Task ResolveTenantAsync()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var userTenantId = ResolveAuthenticatedTenantId(httpContext);

                if (httpContext?.User?.Identity?.IsAuthenticated == true &&
                    httpContext.User.IsInRole("SuperAdmin"))
                {
                    SetTenant(0, "LegalSaaS Platform", true);
                    return;
                }

                if (httpContext is not null && !IsBlazorCircuitContext(httpContext))
                {
                    await ResolveTenantByHostAsync(httpContext.Request.Host.Host, userTenantId);
                    return;
                }

                var browserUri = GetBrowserUri();
                if (!string.IsNullOrWhiteSpace(browserUri) &&
                    Uri.TryCreate(browserUri, UriKind.Absolute, out var uri))
                {
                    if (uri.AbsolutePath.StartsWith("/superadmin", StringComparison.OrdinalIgnoreCase))
                    {
                        SetTenant(0, "LegalSaaS Platform", true);
                        return;
                    }

                    await ResolveTenantByHostAsync(uri.Host, userTenantId);
                    return;
                }

                if (userTenantId is > 0)
                {
                    await ResolveTenantByIdAsync(userTenantId.Value);
                    return;
                }

                await ResolveTenantByHostAsync(null, null);
            }
            catch
            {
                SetTenant(
                    0,
                    "LegalSaaS",
                    isTenantActive: false,
                    tenantBlockReason: "No se pudo validar el tenant.");
            }
        }

        private async Task ResolveTenantByHostAsync(string? rawHost, int? fallbackTenantId)
        {
            var subdomain = ResolveSubdomain(rawHost);

            await using var scope = _serviceProvider.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            Tenant? tenant = null;
            if (!string.IsNullOrWhiteSpace(subdomain))
            {
                tenant = await db.Tenants
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Subdomain == subdomain);
            }

            if (tenant is null && fallbackTenantId is > 0)
            {
                tenant = await db.Tenants
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == fallbackTenantId.Value);
            }

            if (tenant is null && string.IsNullOrWhiteSpace(subdomain))
            {
                tenant = await db.Tenants
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == 1);
            }

            if (tenant is null)
            {
                SetTenant(
                    0,
                    !string.IsNullOrWhiteSpace(subdomain) ? subdomain : "LegalSaaS",
                    isTenantActive: false,
                    tenantBlockReason: "Tenant no encontrado.");
                return;
            }

            SetTenant(
                tenant.Id,
                tenant.Name,
                isTenantActive: tenant.IsActive,
                subscriptionEndsAt: tenant.TrialEndsAt,
                tenantBlockReason: ResolveBlockReason(tenant));
        }

        private async Task ResolveTenantByIdAsync(int tenantId)
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var tenant = await db.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == tenantId);

            if (tenant is null)
            {
                SetTenant(
                    0,
                    "LegalSaaS",
                    isTenantActive: false,
                    tenantBlockReason: "Tenant no encontrado.");
                return;
            }

            SetTenant(
                tenant.Id,
                tenant.Name,
                isTenantActive: tenant.IsActive,
                subscriptionEndsAt: tenant.TrialEndsAt,
                tenantBlockReason: ResolveBlockReason(tenant));
        }

        private string? GetBrowserUri()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(_navigationManager.Uri))
                    return _navigationManager.Uri;

                return string.IsNullOrWhiteSpace(_navigationManager.BaseUri)
                    ? null
                    : _navigationManager.BaseUri;
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        private static int? ResolveAuthenticatedTenantId(HttpContext? context)
        {
            if (context?.User?.Identity?.IsAuthenticated != true)
                return null;

            var value = context.User.FindFirstValue("tenant_id");
            return int.TryParse(value, out var tenantId) ? tenantId : null;
        }

        private static bool IsBlazorCircuitContext(HttpContext context)
        {
            var path = context.Request.Path.Value ?? string.Empty;
            return path.StartsWith("/_blazor", StringComparison.OrdinalIgnoreCase) ||
                   context.WebSockets.IsWebSocketRequest ||
                   string.Equals(context.Request.Headers["Upgrade"].ToString(), "websocket", StringComparison.OrdinalIgnoreCase);
        }

        private static string? ResolveSubdomain(string? rawHost)
        {
            var host = rawHost?.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(host))
                return null;

            if (System.Net.IPAddress.TryParse(host, out _))
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
