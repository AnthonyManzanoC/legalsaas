using System;
using System.Collections.Generic;

namespace LegalSaaS.Domain.Entities
{
    public interface ITenantEntity
    {
        int TenantId { get; set; }
    }

    public abstract class BaseEntity : ITenantEntity
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }

    public class Tenant
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Subdomain { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? OwnerUserId { get; set; }
        public DateTime? TrialEndsAt { get; set; }

        public ICollection<LawFirmSettings> LawFirmSettings { get; set; } = new List<LawFirmSettings>();
        public ICollection<LawyerProfile> LawyerProfiles { get; set; } = new List<LawyerProfile>();
        public ICollection<ServiceItem> Services { get; set; } = new List<ServiceItem>();
        public ICollection<EditorialPost> EditorialPosts { get; set; } = new List<EditorialPost>();
        public ICollection<TeamMember> TeamMembers { get; set; } = new List<TeamMember>();
        public ICollection<Testimonial> Testimonials { get; set; } = new List<Testimonial>();
        public ICollection<ContactMessage> ContactMessages { get; set; } = new List<ContactMessage>();
        public ICollection<TenantSubscription> Subscriptions { get; set; } = new List<TenantSubscription>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }

    public class LawFirmSettings : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? LegalName { get; set; }
        public string? LogoUrl { get; set; }
        public string? BannerTitle { get; set; }
        public string? BannerSubtitle { get; set; }
        public string? HeroTitle { get; set; }
        public string? HeroSubtitle { get; set; }
        public string? EditorialBioTitle { get; set; }
        public string? EditorialBioText { get; set; }
        public string? Description { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Phone { get; set; }
        public string? WhatsApp { get; set; }
        public string? Email { get; set; }
        public string? GoogleMapsUrl { get; set; }
        public string? MapEmbedUrl { get; set; }
        public string? FacebookUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? LinkedinUrl { get; set; }
        public string PrimaryColor { get; set; } = "#0B192C";
        public string HeaderBackgroundColor { get; set; } = "#FFFFFF";
    }

    public class LawyerProfile : BaseEntity
    {
        public string FullName { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? PhotoUrl { get; set; }
        public string? Biography { get; set; }
        public string? AcademicBackground { get; set; }
        public string? ExperienceYears { get; set; }
        public string? RegistrationNumber { get; set; }
        public string? WhatsApp { get; set; }
        public string? Email { get; set; }
        public string? PublicSlug { get; set; }
    }

    public class ServiceItem : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string? Slug { get; set; }
        public string? Summary { get; set; }
        public string? Content { get; set; }
        public string? ImageUrl { get; set; }
        public int SortOrder { get; set; }
        public bool ShowOnHome { get; set; }
    }

    public class EditorialPost : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string? Slug { get; set; }
        public string? Summary { get; set; }
        public string? Content { get; set; }
        public string? CoverImageUrl { get; set; }
        public string? AuthorName { get; set; }
        public DateTime? PublishedAt { get; set; }
        public bool IsPublished { get; set; }
    }

    public class TeamMember : BaseEntity
    {
        public string FullName { get; set; } = string.Empty;
        public string? Role { get; set; }
        public string? PhotoUrl { get; set; }
        public string? ShortBio { get; set; }
        public int SortOrder { get; set; }
    }

    public class Testimonial : BaseEntity
    {
        public string ClientName { get; set; } = string.Empty;
        public string? ReviewText { get; set; }
        public string? Content { get; set; }
        public string? Source { get; set; }
        public int Rating { get; set; } = 5;
        public bool IsApproved { get; set; }
    }

    public class ContactMessage : BaseEntity
    {
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? Subject { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
    }

    public class SeoSettings : BaseEntity
    {
        public string? SiteTitle { get; set; }
        public string? MetaDescription { get; set; }
        public string? MetaKeywords { get; set; }
        public string? CanonicalUrl { get; set; }
        public string? OgImageUrl { get; set; }
    }
    public class PageSeo : BaseEntity
    {
        public string PageKey { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Keywords { get; set; }
        public string? ImageUrl { get; set; }
        public string? CanonicalUrl { get; set; }
    }

    public class DashboardMetricsDto
    {
        public int ServicesCount { get; set; }
        public int PublishedPostsCount { get; set; }
        public int UnreadMessagesCount { get; set; }
        public int TeamMembersCount { get; set; }
        public int TestimonialsCount { get; set; }
    }

    public class AuditLog : BaseEntity
    {
        public string EntityName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string? UserId { get; set; }
        public string? Details { get; set; }
    }

    public enum SubscriptionStatus
    {
        Trialing = 0,
        Active = 1,
        PastDue = 2,
        Suspended = 3,
        Cancelled = 4
    }

    public enum AppointmentStatus
    {
        Requested = 0,
        Confirmed = 1,
        Rescheduled = 2,
        Completed = 3,
        Cancelled = 4,
        NoShow = 5
    }

    public enum AppointmentType
    {
        Consultation = 0,
        Hearing = 1,
        Call = 2,
        FollowUp = 3
    }

    public class SubscriptionPlan : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal MonthlyPrice { get; set; }
        public int MaxUsers { get; set; }
        public int MaxLawyers { get; set; }
        public bool IsHighlighted { get; set; }
        public int SortOrder { get; set; }

        public ICollection<SubscriptionPlanFeature> Features { get; set; } = new List<SubscriptionPlanFeature>();
    }

    public class SubscriptionPlanFeature : BaseEntity
    {
        public int SubscriptionPlanId { get; set; }
        public SubscriptionPlan? SubscriptionPlan { get; set; }
        public string FeatureKey { get; set; } = string.Empty;
        public string FeatureName { get; set; } = string.Empty;
        public bool Enabled { get; set; } = true;
    }

    public class TenantSubscription : BaseEntity
    {
        public Tenant? Tenant { get; set; }
        public int SubscriptionPlanId { get; set; }
        public SubscriptionPlan? SubscriptionPlan { get; set; }
        public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Trialing;
        public DateTime StartAt { get; set; } = DateTime.UtcNow;
        public DateTime? EndAt { get; set; }
        public DateTime? TrialEndsAt { get; set; }
        public DateTime? NextBillingAt { get; set; }
        public decimal PriceAtSubscription { get; set; }
        public string? PaymentProvider { get; set; }
        public string? ExternalSubscriptionId { get; set; }
    }

    public class Appointment : BaseEntity
    {
        public string ClientName { get; set; } = string.Empty;
        public string? ClientPhone { get; set; }
        public string? ClientEmail { get; set; }
        public AppointmentType Type { get; set; } = AppointmentType.Consultation;
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Requested;
        public string Subject { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public int? LawyerProfileId { get; set; }
        public LawyerProfile? LawyerProfile { get; set; }
        public string? Location { get; set; }
        public string? CreatedByUserId { get; set; }
    }

    public class RoleModulePermission : BaseEntity
    {
        public string RoleName { get; set; } = string.Empty;
        public string ModuleKey { get; set; } = string.Empty;
        public bool CanView { get; set; } = true;
        public bool CanCreate { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanPublish { get; set; }
    }

    public static class ModuleKeys
    {
        public const string Dashboard = "dashboard";
        public const string Settings = "settings";
        public const string LawyerProfile = "lawyer_profile";
        public const string Services = "services";
        public const string Editorial = "editorial";
        public const string Team = "team";
        public const string Testimonials = "testimonials";
        public const string Contacts = "contacts";
        public const string Appointments = "appointments";
        public const string Subscriptions = "subscriptions";
        public const string Tenants = "tenants";
        public const string Permissions = "permissions";
    }
}
