using LegalSaaS.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LegalSaaS.Application.Interfaces
{
    public interface ILawFirmSettingsService
    {
        Task<LawFirmSettings?> GetCurrentAsync();
        Task SaveAsync(LawFirmSettings model);
    }

    public interface ILawyerProfileService
    {
        Task<LawyerProfile?> GetCurrentAsync();
        Task<LawyerProfile?> GetBySlugAsync(string slug);
        Task<List<LawyerProfile>> GetAllAsync();
        Task SaveAsync(LawyerProfile model);
    }

    public interface IServiceItemService
    {
        Task<List<ServiceItem>> GetAllAsync();
        Task AddAsync(ServiceItem model);
        Task DeleteAsync(int id);
    }
    // ... (Aquí están las interfaces de FirmSettings, LawyerProfile y ServiceItem)

    public interface IEditorialPostService
    {
        Task<List<EditorialPost>> GetAllAsync();
        Task<EditorialPost?> GetByIdAsync(int id);
        Task AddAsync(EditorialPost model);
        Task UpdateAsync(EditorialPost model);
        Task DeleteAsync(int id);
        Task PublishAsync(int id);
    }
    public interface ISeoService { Task<PageSeo?> GetByPageKeyAsync(string pageKey); Task SaveAsync(PageSeo model); }
    public interface IDashboardMetricsService { Task<DashboardMetricsDto> GetAsync(); }

    public interface ITestimonialService
    {
        Task<List<Testimonial>> GetAllAsync();
        Task<List<Testimonial>> GetApprovedAsync();
        Task AddAsync(Testimonial model);
        Task SubmitPublicAsync(Testimonial model);
        Task SetApprovalAsync(int id, bool isApproved);
        Task DeleteAsync(int id);
    }

    public interface ITeamMemberService
    {
        Task<List<TeamMember>> GetAllAsync();
        Task AddAsync(TeamMember model);
        Task DeleteAsync(int id);
    }

    public interface IContactMessageService
    {
        Task<List<ContactMessage>> GetAllAsync();
        Task AddAsync(ContactMessage model);
        Task MarkAsReadAsync(int id);
        Task DeleteAsync(int id);
    }

    public class CreateTenantRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Subdomain { get; set; } = string.Empty;
        public string AdminFullName { get; set; } = string.Empty;
        public string AdminEmail { get; set; } = string.Empty;
        public string? AdminPassword { get; set; }
        public int SubscriptionPlanId { get; set; }
        public int TrialDays { get; set; } = 365;
    }

    public class CreateTenantResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int TenantId { get; set; }
        public string? GeneratedPassword { get; set; }
    }

    public class TenantOperationResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public DateTime? SubscriptionEndsAt { get; set; }
    }

    public class TenantEditRequest
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Subdomain { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? TrialEndsAt { get; set; }
        public string? OwnerUserId { get; set; }
        public string? AdminFullName { get; set; }
        public string? AdminEmail { get; set; }
        public string? NewAdminPassword { get; set; }
    }

    public interface ISuperAdminTenantService
    {
        Task<List<Tenant>> GetTenantsAsync();
        Task<List<TenantSubscription>> GetSubscriptionsAsync();
        Task<List<SubscriptionPlan>> GetPlansAsync();
        Task<CreateTenantResult> CreateTenantAsync(CreateTenantRequest request);
        Task<TenantEditRequest?> GetTenantEditAsync(int tenantId);
        Task<TenantOperationResult> SaveTenantAsync(Tenant tenant);
        Task<TenantOperationResult> SaveTenantAsync(TenantEditRequest tenant);
        Task<TenantOperationResult> RenewAnnualSubscriptionAsync(int tenantId);
    }

    public class CreateAppointmentResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
    }

    public interface IAppointmentService
    {
        Task<List<Appointment>> GetAllAsync();
        Task<List<Appointment>> GetByRangeAsync(DateTime from, DateTime to);
        Task<Appointment?> GetByIdAsync(int id);
        Task<CreateAppointmentResult> CreateAsync(Appointment model);
        Task UpdateStatusAsync(int id, AppointmentStatus status);
        Task DeleteAsync(int id);
    }

    public interface IRolePermissionService
    {
        Task<bool> CanViewAsync(string roleName, string moduleKey);
        Task<bool> CanEditAsync(string roleName, string moduleKey);
        Task SaveMatrixAsync(int tenantId, string roleName, IEnumerable<RoleModulePermission> permissions);
    }
}
