namespace LegalSaaS.Application.Interfaces
{
    public interface ITenantContext
    {
        int TenantId { get; }
        string? TenantName { get; }
        bool IsSuperAdmin { get; }
        bool IsTenantActive { get; }
        DateTime? SubscriptionEndsAt { get; }
        bool IsSubscriptionExpired { get; }
        bool IsTenantBlocked { get; }
        string? TenantBlockReason { get; }
        bool IsTenantResolved { get; }

        Task EnsureTenantResolvedAsync();

        void SetTenant(
            int tenantId,
            string? tenantName = null,
            bool isSuperAdmin = false,
            bool isTenantActive = true,
            DateTime? subscriptionEndsAt = null,
            string? tenantBlockReason = null);
    }
}
