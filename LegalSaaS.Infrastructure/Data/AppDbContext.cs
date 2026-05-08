using LegalSaaS.Application.Interfaces;
using LegalSaaS.Domain.Entities;
using LegalSaaS.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LegalSaaS.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly ITenantContext _tenantContext;

        public AppDbContext(
            DbContextOptions<AppDbContext> options,
            ITenantContext tenantContext) : base(options)
        {
            _tenantContext = tenantContext;
        }

        public DbSet<PageSeo> PageSeos => Set<PageSeo>();
        public DbSet<Tenant> Tenants => Set<Tenant>();
        public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
        public DbSet<SubscriptionPlanFeature> SubscriptionPlanFeatures => Set<SubscriptionPlanFeature>();
        public DbSet<TenantSubscription> TenantSubscriptions => Set<TenantSubscription>();
        public DbSet<Appointment> Appointments => Set<Appointment>();
        public DbSet<RoleModulePermission> RoleModulePermissions => Set<RoleModulePermission>();
        public DbSet<LawFirmSettings> LawFirmSettings => Set<LawFirmSettings>();
        public DbSet<LawyerProfile> LawyerProfiles => Set<LawyerProfile>();
        public DbSet<ServiceItem> ServiceItems => Set<ServiceItem>();
        public DbSet<EditorialPost> EditorialPosts => Set<EditorialPost>();
        public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
        public DbSet<Testimonial> Testimonials => Set<Testimonial>();
        public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
        public DbSet<SeoSettings> SeoSettings => Set<SeoSettings>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // CORRECCIÓN PARA POSTGRES: Usamos comillas dobles en lugar de corchetes
            builder.Entity<Tenant>()
        .HasIndex(x => x.Subdomain)
        .IsUnique();

            builder.Entity<SubscriptionPlan>()
                .HasIndex(x => x.Code)
                .IsUnique();

            // Ajuste de tipo para Postgres (numeric es mejor para decimales)
            builder.Entity<SubscriptionPlan>()
                .Property(x => x.MonthlyPrice)
                .HasColumnType("numeric(18,2)");

            builder.Entity<SubscriptionPlanFeature>()
                .HasIndex(x => new { x.SubscriptionPlanId, x.FeatureKey })
                .IsUnique();

            builder.Entity<TenantSubscription>()
                .HasIndex(x => new { x.TenantId, x.Status });

            builder.Entity<TenantSubscription>()
                .Property(x => x.PriceAtSubscription)
                .HasColumnType("numeric(18,2)");

            builder.Entity<Appointment>()
                .HasIndex(x => new { x.TenantId, x.StartAt });

            builder.Entity<RoleModulePermission>()
                .HasIndex(x => new { x.TenantId, x.RoleName, x.ModuleKey })
                .IsUnique();

            // Filtros de Tenant (Multi-tenancy)
            builder.Entity<PageSeo>().HasQueryFilter(x => _tenantContext.IsSuperAdmin || x.TenantId == _tenantContext.TenantId);
            builder.Entity<PageSeo>().HasIndex(x => new { x.TenantId, x.PageKey }).IsUnique();

            builder.Entity<LawFirmSettings>().Property(x => x.PrimaryColor).HasMaxLength(20).HasDefaultValue("#0B192C");
            builder.Entity<LawFirmSettings>().Property(x => x.HeaderBackgroundColor).HasMaxLength(20).HasDefaultValue("#FFFFFF");

            builder.Entity<LawFirmSettings>().HasQueryFilter(x => _tenantContext.IsSuperAdmin || x.TenantId == _tenantContext.TenantId);
            builder.Entity<LawyerProfile>().HasQueryFilter(x => _tenantContext.IsSuperAdmin || x.TenantId == _tenantContext.TenantId);
            builder.Entity<ServiceItem>().HasQueryFilter(x => _tenantContext.IsSuperAdmin || x.TenantId == _tenantContext.TenantId);
            builder.Entity<EditorialPost>().HasQueryFilter(x => _tenantContext.IsSuperAdmin || x.TenantId == _tenantContext.TenantId);
            builder.Entity<TeamMember>().HasQueryFilter(x => _tenantContext.IsSuperAdmin || x.TenantId == _tenantContext.TenantId);
            builder.Entity<Testimonial>().HasQueryFilter(x => _tenantContext.IsSuperAdmin || x.TenantId == _tenantContext.TenantId);
            builder.Entity<ContactMessage>().HasQueryFilter(x => _tenantContext.IsSuperAdmin || x.TenantId == _tenantContext.TenantId);
            builder.Entity<SeoSettings>().HasQueryFilter(x => _tenantContext.IsSuperAdmin || x.TenantId == _tenantContext.TenantId);
            builder.Entity<AuditLog>().HasQueryFilter(x => _tenantContext.IsSuperAdmin || x.TenantId == _tenantContext.TenantId);
            builder.Entity<TenantSubscription>().HasQueryFilter(x => _tenantContext.IsSuperAdmin || x.TenantId == _tenantContext.TenantId);
            builder.Entity<Appointment>().HasQueryFilter(x => _tenantContext.IsSuperAdmin || x.TenantId == _tenantContext.TenantId);
            builder.Entity<RoleModulePermission>().HasQueryFilter(x => _tenantContext.IsSuperAdmin || x.TenantId == _tenantContext.TenantId);
        }
    }
}