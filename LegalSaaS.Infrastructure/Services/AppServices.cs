using LegalSaaS.Application.Interfaces;
using LegalSaaS.Domain.Entities;
using LegalSaaS.Infrastructure.Data;
using LegalSaaS.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace LegalSaaS.Infrastructure.Services
{
    public class LawFirmSettingsService : ILawFirmSettingsService
    {
        private readonly AppDbContext _db;
        private readonly ITenantContext _tenant;

        public LawFirmSettingsService(AppDbContext db, ITenantContext tenant)
        {
            _db = db;
            _tenant = tenant;
        }

        public async Task<LawFirmSettings?> GetCurrentAsync()
        {
            await _tenant.EnsureTenantResolvedAsync();
            return await _db.LawFirmSettings.FirstOrDefaultAsync(x => x.TenantId == _tenant.TenantId);
        }

        public async Task SaveAsync(LawFirmSettings model)
        {
            await _tenant.EnsureTenantResolvedAsync();
            var existing = await _db.LawFirmSettings.FirstOrDefaultAsync(x => x.TenantId == _tenant.TenantId);
            if (existing is null)
            {
                model.TenantId = _tenant.TenantId;
                model.CreatedAt = DateTime.UtcNow;
                model.PrimaryColor = string.IsNullOrWhiteSpace(model.PrimaryColor) ? "#0B192C" : model.PrimaryColor;
                model.HeaderBackgroundColor = string.IsNullOrWhiteSpace(model.HeaderBackgroundColor) ? "#FFFFFF" : model.HeaderBackgroundColor;
                _db.LawFirmSettings.Add(model);
            }
            else
            {
                existing.Name = model.Name;
                existing.LegalName = model.LegalName;
                existing.LogoUrl = model.LogoUrl;
                existing.BannerTitle = model.BannerTitle;
                existing.BannerSubtitle = model.BannerSubtitle;
                existing.HeroTitle = model.HeroTitle;
                existing.HeroSubtitle = model.HeroSubtitle;
                existing.EditorialBioTitle = model.EditorialBioTitle;
                existing.EditorialBioText = model.EditorialBioText;
                existing.Description = model.Description;
                existing.Address = model.Address;
                existing.City = model.City;
                existing.Phone = model.Phone;
                existing.WhatsApp = model.WhatsApp;
                existing.Email = model.Email;
                existing.GoogleMapsUrl = model.GoogleMapsUrl;
                existing.MapEmbedUrl = model.MapEmbedUrl;
                existing.FacebookUrl = model.FacebookUrl;
                existing.InstagramUrl = model.InstagramUrl;
                existing.LinkedinUrl = model.LinkedinUrl;
                existing.PrimaryColor = string.IsNullOrWhiteSpace(model.PrimaryColor) ? "#0B192C" : model.PrimaryColor;
                existing.HeaderBackgroundColor = string.IsNullOrWhiteSpace(model.HeaderBackgroundColor) ? "#FFFFFF" : model.HeaderBackgroundColor;
                existing.UpdatedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
        }
    }

    public class LawyerProfileService : ILawyerProfileService
    {
        private readonly AppDbContext _db;
        private readonly ITenantContext _tenant;

        public LawyerProfileService(AppDbContext db, ITenantContext tenant)
        {
            _db = db;
            _tenant = tenant;
        }

        public async Task<LawyerProfile?> GetCurrentAsync()
        {
            await _tenant.EnsureTenantResolvedAsync();
            return await _db.LawyerProfiles.OrderBy(x => x.Id).FirstOrDefaultAsync(x => x.TenantId == _tenant.TenantId);
        }

        public async Task<LawyerProfile?> GetBySlugAsync(string slug)
        {
            await _tenant.EnsureTenantResolvedAsync();
            return await _db.LawyerProfiles.FirstOrDefaultAsync(x => x.PublicSlug == slug);
        }

        public async Task<List<LawyerProfile>> GetAllAsync()
        {
            await _tenant.EnsureTenantResolvedAsync();
            return await _db.LawyerProfiles.OrderBy(x => x.FullName).ToListAsync();
        }

        public async Task SaveAsync(LawyerProfile model)
        {
            await _tenant.EnsureTenantResolvedAsync();
            var existing = model.Id > 0
                ? await _db.LawyerProfiles.FirstOrDefaultAsync(x => x.Id == model.Id)
                : await _db.LawyerProfiles.FirstOrDefaultAsync(x => x.TenantId == _tenant.TenantId);

            if (existing is null)
            {
                model.TenantId = _tenant.TenantId;
                model.PublicSlug = string.IsNullOrWhiteSpace(model.PublicSlug) ? SlugHelpers.Slugify(model.FullName) : SlugHelpers.Slugify(model.PublicSlug);
                model.CreatedAt = DateTime.UtcNow;
                _db.LawyerProfiles.Add(model);
            }
            else
            {
                existing.FullName = model.FullName;
                existing.Title = model.Title;
                existing.PhotoUrl = model.PhotoUrl;
                existing.Biography = model.Biography;
                existing.AcademicBackground = model.AcademicBackground;
                existing.ExperienceYears = model.ExperienceYears;
                existing.RegistrationNumber = model.RegistrationNumber;
                existing.WhatsApp = model.WhatsApp;
                existing.Email = model.Email;
                existing.PublicSlug = string.IsNullOrWhiteSpace(model.PublicSlug) ? SlugHelpers.Slugify(model.FullName) : SlugHelpers.Slugify(model.PublicSlug);
                existing.UpdatedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
        }
    }

    public class ServiceItemService : IServiceItemService
    {
        private readonly AppDbContext _db;
        private readonly ITenantContext _tenant;

        public ServiceItemService(AppDbContext db, ITenantContext tenant)
        {
            _db = db;
            _tenant = tenant;
        }

        public async Task<List<ServiceItem>> GetAllAsync()
        {
            await _tenant.EnsureTenantResolvedAsync();
            return await _db.ServiceItems.OrderBy(x => x.SortOrder).ThenBy(x => x.Title).ToListAsync();
        }

        public async Task AddAsync(ServiceItem model)
        {
            await _tenant.EnsureTenantResolvedAsync();
            model.TenantId = _tenant.TenantId;
            model.Slug = string.IsNullOrWhiteSpace(model.Slug) ? SlugHelpers.Slugify(model.Title) : SlugHelpers.Slugify(model.Slug);
            model.CreatedAt = DateTime.UtcNow;
            _db.ServiceItems.Add(model);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            await _tenant.EnsureTenantResolvedAsync();
            var item = await _db.ServiceItems.FirstOrDefaultAsync(x => x.Id == id);
            if (item is null) return;

            _db.ServiceItems.Remove(item);
            await _db.SaveChangesAsync();
        }
    }

    public class EditorialPostService : IEditorialPostService
    {
        private readonly AppDbContext _db;
        private readonly ITenantContext _tenant;

        public EditorialPostService(AppDbContext db, ITenantContext tenant)
        {
            _db = db;
            _tenant = tenant;
        }

        public async Task<List<EditorialPost>> GetAllAsync()
        {
            await _tenant.EnsureTenantResolvedAsync();
            return await _db.EditorialPosts.OrderByDescending(x => x.CreatedAt).ToListAsync();
        }

        public async Task<EditorialPost?> GetByIdAsync(int id)
        {
            await _tenant.EnsureTenantResolvedAsync();
            return await _db.EditorialPosts.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task AddAsync(EditorialPost model)
        {
            await _tenant.EnsureTenantResolvedAsync();
            model.TenantId = _tenant.TenantId;
            model.Slug = SlugHelpers.Slugify(model.Title);
            model.CreatedAt = DateTime.UtcNow;
            model.PublishedAt = model.IsPublished ? DateTime.UtcNow : null;
            _db.EditorialPosts.Add(model);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(EditorialPost model)
        {
            await _tenant.EnsureTenantResolvedAsync();
            var existing = await _db.EditorialPosts.FirstOrDefaultAsync(x => x.Id == model.Id);
            if (existing is null) return;

            existing.Title = model.Title;
            existing.Slug = SlugHelpers.Slugify(model.Title);
            existing.Summary = model.Summary;
            existing.Content = model.Content;
            existing.CoverImageUrl = model.CoverImageUrl;
            existing.AuthorName = model.AuthorName;
            existing.IsPublished = model.IsPublished;
            existing.PublishedAt = model.IsPublished ? (model.PublishedAt ?? DateTime.UtcNow) : null;
            existing.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            await _tenant.EnsureTenantResolvedAsync();
            var post = await _db.EditorialPosts.FirstOrDefaultAsync(x => x.Id == id);
            if (post is null) return;

            _db.EditorialPosts.Remove(post);
            await _db.SaveChangesAsync();
        }

        public async Task PublishAsync(int id)
        {
            await _tenant.EnsureTenantResolvedAsync();
            var post = await _db.EditorialPosts.FirstOrDefaultAsync(x => x.Id == id);
            if (post is null) return;

            post.IsPublished = true;
            post.PublishedAt = DateTime.UtcNow;
            post.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }

    public class TestimonialService : ITestimonialService
    {
        private readonly AppDbContext _db;
        private readonly ITenantContext _tenant;

        public TestimonialService(AppDbContext db, ITenantContext tenant)
        {
            _db = db;
            _tenant = tenant;
        }

        public async Task<List<Testimonial>> GetAllAsync()
        {
            await _tenant.EnsureTenantResolvedAsync();
            return await _db.Testimonials.OrderByDescending(x => x.CreatedAt).ToListAsync();
        }

        public async Task<List<Testimonial>> GetApprovedAsync()
        {
            await _tenant.EnsureTenantResolvedAsync();
            return await _db.Testimonials
                .Where(x => x.IsApproved)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task AddAsync(Testimonial model)
        {
            await _tenant.EnsureTenantResolvedAsync();
            Normalize(model);
            model.TenantId = _tenant.TenantId;
            model.CreatedAt = DateTime.UtcNow;
            _db.Testimonials.Add(model);
            await _db.SaveChangesAsync();
        }

        public async Task SubmitPublicAsync(Testimonial model)
        {
            await _tenant.EnsureTenantResolvedAsync();
            Normalize(model);
            model.TenantId = _tenant.TenantId;
            model.IsApproved = false;
            model.CreatedAt = DateTime.UtcNow;
            _db.Testimonials.Add(model);
            await _db.SaveChangesAsync();
        }

        public async Task SetApprovalAsync(int id, bool isApproved)
        {
            await _tenant.EnsureTenantResolvedAsync();
            var item = await _db.Testimonials.FirstOrDefaultAsync(x => x.Id == id);
            if (item is null) return;

            item.IsApproved = isApproved;
            item.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            await _tenant.EnsureTenantResolvedAsync();
            var item = await _db.Testimonials.FirstOrDefaultAsync(x => x.Id == id);
            if (item is null) return;

            _db.Testimonials.Remove(item);
            await _db.SaveChangesAsync();
        }

        private static void Normalize(Testimonial model)
        {
            model.Rating = Math.Clamp(model.Rating, 1, 5);

            if (string.IsNullOrWhiteSpace(model.ReviewText) && !string.IsNullOrWhiteSpace(model.Content))
                model.ReviewText = model.Content;

            if (string.IsNullOrWhiteSpace(model.Content) && !string.IsNullOrWhiteSpace(model.ReviewText))
                model.Content = model.ReviewText;
        }
    }

    public class TeamMemberService : ITeamMemberService
    {
        private readonly AppDbContext _db;
        private readonly ITenantContext _tenant;

        public TeamMemberService(AppDbContext db, ITenantContext tenant)
        {
            _db = db;
            _tenant = tenant;
        }

        public async Task<List<TeamMember>> GetAllAsync()
        {
            await _tenant.EnsureTenantResolvedAsync();
            return await _db.TeamMembers.OrderBy(x => x.SortOrder).ThenBy(x => x.FullName).ToListAsync();
        }

        public async Task AddAsync(TeamMember model)
        {
            await _tenant.EnsureTenantResolvedAsync();
            model.TenantId = _tenant.TenantId;
            model.CreatedAt = DateTime.UtcNow;
            _db.TeamMembers.Add(model);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            await _tenant.EnsureTenantResolvedAsync();
            var item = await _db.TeamMembers.FirstOrDefaultAsync(x => x.Id == id);
            if (item is null) return;

            _db.TeamMembers.Remove(item);
            await _db.SaveChangesAsync();
        }
    }

    public class ContactMessageService : IContactMessageService
    {
        private readonly AppDbContext _db;
        private readonly ITenantContext _tenant;

        public ContactMessageService(AppDbContext db, ITenantContext tenant)
        {
            _db = db;
            _tenant = tenant;
        }

        public async Task<List<ContactMessage>> GetAllAsync()
        {
            await _tenant.EnsureTenantResolvedAsync();
            return await _db.ContactMessages.OrderByDescending(x => x.CreatedAt).ToListAsync();
        }

        public async Task AddAsync(ContactMessage model)
        {
            await _tenant.EnsureTenantResolvedAsync();
            model.TenantId = _tenant.TenantId;
            model.CreatedAt = DateTime.UtcNow;
            model.IsRead = false;
            _db.ContactMessages.Add(model);
            await _db.SaveChangesAsync();
        }

        public async Task MarkAsReadAsync(int id)
        {
            await _tenant.EnsureTenantResolvedAsync();
            var item = await _db.ContactMessages.FirstOrDefaultAsync(x => x.Id == id);
            if (item is null) return;

            item.IsRead = true;
            item.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            await _tenant.EnsureTenantResolvedAsync();
            var item = await _db.ContactMessages.FirstOrDefaultAsync(x => x.Id == id);
            if (item is null) return;

            _db.ContactMessages.Remove(item);
            await _db.SaveChangesAsync();
        }
    }

    public class SeoService : ISeoService
    {
        private readonly AppDbContext _db;
        private readonly ITenantContext _tenant;

        public SeoService(AppDbContext db, ITenantContext tenant)
        {
            _db = db;
            _tenant = tenant;
        }

        public async Task<PageSeo?> GetByPageKeyAsync(string pageKey)
        {
            await _tenant.EnsureTenantResolvedAsync();
            return await _db.PageSeos.FirstOrDefaultAsync(x => x.PageKey == pageKey);
        }

        public async Task SaveAsync(PageSeo model)
        {
            await _tenant.EnsureTenantResolvedAsync();
            var existing = await _db.PageSeos.FirstOrDefaultAsync(x => x.PageKey == model.PageKey);
            if (existing is null)
            {
                model.TenantId = _tenant.TenantId;
                model.CreatedAt = DateTime.UtcNow;
                _db.PageSeos.Add(model);
            }
            else
            {
                existing.Title = model.Title;
                existing.Description = model.Description;
                existing.Keywords = model.Keywords;
                existing.ImageUrl = model.ImageUrl;
                existing.CanonicalUrl = model.CanonicalUrl;
                existing.UpdatedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
        }
    }

    public class DashboardMetricsService : IDashboardMetricsService
    {
        private readonly AppDbContext _db;
        private readonly ITenantContext _tenant;

        public DashboardMetricsService(AppDbContext db, ITenantContext tenant)
        {
            _db = db;
            _tenant = tenant;
        }

        public async Task<DashboardMetricsDto> GetAsync()
        {
            await _tenant.EnsureTenantResolvedAsync();
            return new DashboardMetricsDto
            {
                ServicesCount = await _db.ServiceItems.CountAsync(),
                PublishedPostsCount = await _db.EditorialPosts.CountAsync(x => x.IsPublished),
                UnreadMessagesCount = await _db.ContactMessages.CountAsync(x => !x.IsRead),
                TeamMembersCount = await _db.TeamMembers.CountAsync(),
                TestimonialsCount = await _db.Testimonials.CountAsync()
            };
        }
    }

    public class SuperAdminTenantService : ISuperAdminTenantService
    {
        private const string AnnualManualPlanCode = "annual_manual";
        private const decimal AnnualManualPrice = 100m;
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public SuperAdminTenantService(
            AppDbContext db,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<List<Tenant>> GetTenantsAsync() =>
            await _db.Tenants
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

        public async Task<List<TenantSubscription>> GetSubscriptionsAsync() =>
            await _db.TenantSubscriptions
                .IgnoreQueryFilters()
                .Include(x => x.Tenant)
                .Include(x => x.SubscriptionPlan)
                .OrderByDescending(x => x.StartAt)
                .ToListAsync();

        public async Task<List<SubscriptionPlan>> GetPlansAsync() =>
            await _db.SubscriptionPlans
                .Include(x => x.Features)
                .Where(x => x.IsActive)
                .OrderBy(x => x.SortOrder)
                .ToListAsync();

        public async Task<TenantEditRequest?> GetTenantEditAsync(int tenantId)
        {
            var tenant = await _db.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == tenantId);

            if (tenant is null)
                return null;

            var owner = await GetTenantOwnerAsync(tenant);

            return new TenantEditRequest
            {
                Id = tenant.Id,
                Name = tenant.Name,
                Subdomain = tenant.Subdomain,
                IsActive = tenant.IsActive,
                CreatedAt = tenant.CreatedAt,
                TrialEndsAt = tenant.TrialEndsAt,
                OwnerUserId = owner?.Id ?? tenant.OwnerUserId,
                AdminFullName = owner?.FullName,
                AdminEmail = owner?.Email
            };
        }

        public Task<TenantOperationResult> SaveTenantAsync(Tenant tenant) =>
            SaveTenantAsync(new TenantEditRequest
            {
                Id = tenant.Id,
                Name = tenant.Name,
                Subdomain = tenant.Subdomain,
                IsActive = tenant.IsActive,
                TrialEndsAt = tenant.TrialEndsAt
            });

        public async Task<TenantOperationResult> SaveTenantAsync(TenantEditRequest tenant)
        {
            var existing = await _db.Tenants.FirstOrDefaultAsync(x => x.Id == tenant.Id);
            if (existing is null)
                return OperationFail("Tenant no encontrado.");

            var name = tenant.Name.Trim();
            var subdomain = NormalizeSubdomain(tenant.Subdomain ?? string.Empty);

            if (string.IsNullOrWhiteSpace(name))
                return OperationFail("El nombre del estudio es obligatorio.");

            if (string.IsNullOrWhiteSpace(subdomain))
                return OperationFail("El subdominio debe contener letras o numeros.");

            if (await _db.Tenants.AnyAsync(x => x.Id != tenant.Id && x.Subdomain == subdomain))
                return OperationFail("Ese subdominio ya esta asignado a otro tenant.");

            var owner = await GetTenantOwnerAsync(existing);
            var adminEmail = tenant.AdminEmail?.Trim().ToLowerInvariant();
            var adminPassword = tenant.NewAdminPassword?.Trim();

            if (owner is not null && string.IsNullOrWhiteSpace(adminEmail))
                return OperationFail("El email del administrador es obligatorio.");

            if (owner is not null && !string.Equals(owner.Email, adminEmail, StringComparison.OrdinalIgnoreCase))
            {
                var emailOwner = await _userManager.FindByEmailAsync(adminEmail!);
                if (emailOwner is not null && emailOwner.Id != owner.Id)
                    return OperationFail("Ya existe otro usuario con ese email.");
            }

            var adminTouched = !string.IsNullOrWhiteSpace(adminEmail) ||
                               !string.IsNullOrWhiteSpace(tenant.AdminFullName) ||
                               !string.IsNullOrWhiteSpace(adminPassword);

            if (owner is null && adminTouched)
                return OperationFail("No se encontro un usuario administrador asociado a este tenant.");

            await using var transaction = await _db.Database.BeginTransactionAsync();

            existing.Name = name;
            existing.Subdomain = subdomain;
            existing.IsActive = tenant.IsActive;
            existing.TrialEndsAt = tenant.TrialEndsAt;

            if (owner is not null)
            {
                existing.OwnerUserId = owner.Id;
                owner.FullName = tenant.AdminFullName?.Trim();
                owner.Email = adminEmail;
                owner.UserName = adminEmail;
                owner.EmailConfirmed = true;
                owner.TenantId = existing.Id;

                var updateUserResult = await _userManager.UpdateAsync(owner);
                if (!updateUserResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return OperationFail(string.Join(" | ", updateUserResult.Errors.Select(x => x.Description)));
                }

                if (!string.IsNullOrWhiteSpace(adminPassword))
                {
                    var resetToken = await _userManager.GeneratePasswordResetTokenAsync(owner);
                    var resetResult = await _userManager.ResetPasswordAsync(owner, resetToken, adminPassword);
                    if (!resetResult.Succeeded)
                    {
                        await transaction.RollbackAsync();
                        return OperationFail(string.Join(" | ", resetResult.Errors.Select(x => x.Description)));
                    }
                }
            }

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return new TenantOperationResult
            {
                Success = true,
                Message = "Tenant actualizado correctamente.",
                SubscriptionEndsAt = existing.TrialEndsAt
            };
        }

        private async Task<ApplicationUser?> GetTenantOwnerAsync(Tenant tenant)
        {
            if (!string.IsNullOrWhiteSpace(tenant.OwnerUserId))
            {
                var owner = await _userManager.FindByIdAsync(tenant.OwnerUserId);
                if (owner is not null)
                    return owner;
            }

            return await _userManager.Users
                .FirstOrDefaultAsync(x => x.TenantId == tenant.Id);
        }

        public async Task<TenantOperationResult> RenewAnnualSubscriptionAsync(int tenantId)
        {
            var tenant = await _db.Tenants.FirstOrDefaultAsync(x => x.Id == tenantId);
            if (tenant is null)
                return OperationFail("Tenant no encontrado.");

            var annualPlan = await EnsureAnnualManualPlanAsync();
            var now = DateTime.UtcNow;
            var baseDate = tenant.TrialEndsAt.HasValue && tenant.TrialEndsAt.Value > now
                ? tenant.TrialEndsAt.Value
                : now;
            var newExpiration = baseDate.AddDays(365);

            tenant.IsActive = true;
            tenant.TrialEndsAt = newExpiration;

            var subscription = await _db.TenantSubscriptions
                .IgnoreQueryFilters()
                .Where(x => x.TenantId == tenantId)
                .OrderByDescending(x => x.StartAt)
                .FirstOrDefaultAsync();

            if (subscription is null)
            {
                _db.TenantSubscriptions.Add(new TenantSubscription
                {
                    TenantId = tenantId,
                    SubscriptionPlanId = annualPlan.Id,
                    Status = SubscriptionStatus.Active,
                    StartAt = now,
                    EndAt = newExpiration,
                    NextBillingAt = newExpiration,
                    PriceAtSubscription = AnnualManualPrice,
                    PaymentProvider = "Offline / Manual"
                });
            }
            else
            {
                subscription.SubscriptionPlanId = annualPlan.Id;
                subscription.Status = SubscriptionStatus.Active;
                subscription.EndAt = newExpiration;
                subscription.TrialEndsAt = null;
                subscription.NextBillingAt = newExpiration;
                subscription.PriceAtSubscription = AnnualManualPrice;
                subscription.PaymentProvider = "Offline / Manual";
                subscription.UpdatedAt = now;
            }

            await _db.SaveChangesAsync();

            return new TenantOperationResult
            {
                Success = true,
                Message = $"Suscripcion anual renovada hasta {newExpiration:yyyy-MM-dd}.",
                SubscriptionEndsAt = newExpiration
            };
        }

        public async Task<CreateTenantResult> CreateTenantAsync(CreateTenantRequest request)
        {
            var name = request.Name.Trim();
            var subdomain = NormalizeSubdomain(request.Subdomain);
            var email = request.AdminEmail.Trim().ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(name))
                return Fail("El nombre del estudio es obligatorio.");

            if (string.IsNullOrWhiteSpace(subdomain))
                return Fail("El subdominio debe contener letras o numeros.");

            if (string.IsNullOrWhiteSpace(email))
                return Fail("El email del administrador es obligatorio.");

            if (await _db.Tenants.AnyAsync(x => x.Subdomain == subdomain))
                return Fail("Ese subdominio ya existe.");

            if (await _userManager.FindByEmailAsync(email) is not null)
                return Fail("Ya existe un usuario con ese email.");

            var plan = request.SubscriptionPlanId > 0
                ? await _db.SubscriptionPlans.FirstOrDefaultAsync(x => x.Id == request.SubscriptionPlanId && x.IsActive)
                : await EnsureAnnualManualPlanAsync();

            if (plan is null)
                return Fail("Plan no valido.");

            var generatedPassword = string.IsNullOrWhiteSpace(request.AdminPassword)
                ? GenerateSecurePassword()
                : request.AdminPassword;

            await using var transaction = await _db.Database.BeginTransactionAsync();

            var tenant = new Tenant
            {
                Name = name,
                Subdomain = subdomain,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                TrialEndsAt = DateTime.UtcNow.AddDays(request.TrialDays)
            };

            _db.Tenants.Add(tenant);
            await _db.SaveChangesAsync();

            _db.LawFirmSettings.Add(new LawFirmSettings
            {
                TenantId = tenant.Id,
                Name = name,
                BannerTitle = name,
                BannerSubtitle = "Asesoria juridica premium, clara y confiable",
                HeroTitle = name,
                HeroSubtitle = "Asesoria juridica premium, clara y confiable",
                Description = "Portal juridico profesional administrable desde LegalSaaS.",
                Email = email
            });

            _db.SeoSettings.Add(new SeoSettings
            {
                TenantId = tenant.Id,
                SiteTitle = name,
                MetaDescription = $"Sitio oficial de {name}",
                MetaKeywords = "abogado, estudio juridico, consulta legal, asesoria legal"
            });

            _db.TenantSubscriptions.Add(new TenantSubscription
            {
                TenantId = tenant.Id,
                SubscriptionPlanId = plan.Id,
                Status = SubscriptionStatus.Active,
                StartAt = DateTime.UtcNow,
                EndAt = tenant.TrialEndsAt,
                NextBillingAt = tenant.TrialEndsAt,
                PriceAtSubscription = AnnualManualPrice,
                PaymentProvider = "Offline / Manual"
            });

            await SeedTenantPermissionsAsync(tenant.Id);
            await _db.SaveChangesAsync();

            if (!await _roleManager.RoleExistsAsync("Admin"))
                await _roleManager.CreateAsync(new IdentityRole("Admin"));

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = request.AdminFullName.Trim(),
                TenantId = tenant.Id
            };

            var createUserResult = await _userManager.CreateAsync(user, generatedPassword);
            if (!createUserResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return Fail(string.Join(" | ", createUserResult.Errors.Select(x => x.Description)));
            }

            await _userManager.AddToRoleAsync(user, "Admin");

            tenant.OwnerUserId = user.Id;
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return new CreateTenantResult
            {
                Success = true,
                Message = "Tenant creado correctamente.",
                TenantId = tenant.Id,
                GeneratedPassword = generatedPassword
            };
        }

        private async Task SeedTenantPermissionsAsync(int tenantId)
        {
            var roles = new[] { "Admin", "Editor", "Lawyer", "Assistant" };
            var modules = new[]
            {
                ModuleKeys.Dashboard, ModuleKeys.Settings, ModuleKeys.LawyerProfile, ModuleKeys.Services,
                ModuleKeys.Editorial, ModuleKeys.Team, ModuleKeys.Testimonials, ModuleKeys.Contacts,
                ModuleKeys.Appointments, ModuleKeys.Subscriptions, ModuleKeys.Permissions
            };

            foreach (var role in roles)
            {
                foreach (var module in modules)
                {
                    var isAdmin = role == "Admin";
                    var canEditContent = role is "Editor" or "Lawyer";
                    var canAssist = role == "Assistant" &&
                                    (module == ModuleKeys.Contacts || module == ModuleKeys.Appointments);

                    _db.RoleModulePermissions.Add(new RoleModulePermission
                    {
                        TenantId = tenantId,
                        RoleName = role,
                        ModuleKey = module,
                        CanView = isAdmin || canEditContent || canAssist,
                        CanCreate = isAdmin || canEditContent || canAssist,
                        CanEdit = isAdmin || canEditContent || canAssist,
                        CanDelete = isAdmin,
                        CanPublish = isAdmin || (role == "Editor" && module == ModuleKeys.Editorial)
                    });
                }
            }

            await Task.CompletedTask;
        }

        private static CreateTenantResult Fail(string message) => new()
        {
            Success = false,
            Message = message
        };

        private static TenantOperationResult OperationFail(string message) => new()
        {
            Success = false,
            Message = message
        };

        private async Task<SubscriptionPlan> EnsureAnnualManualPlanAsync()
        {
            var plan = await _db.SubscriptionPlans
                .Include(x => x.Features)
                .FirstOrDefaultAsync(x => x.Code == AnnualManualPlanCode);

            if (plan is null)
            {
                plan = new SubscriptionPlan
                {
                    TenantId = 0,
                    Code = AnnualManualPlanCode,
                    Name = "Plan Anual Manual",
                    MonthlyPrice = AnnualManualPrice,
                    MaxUsers = 1,
                    MaxLawyers = 1,
                    IsActive = true,
                    IsHighlighted = true,
                    SortOrder = 1
                };

                _db.SubscriptionPlans.Add(plan);
                await _db.SaveChangesAsync();
            }
            else
            {
                plan.Name = "Plan Anual Manual";
                plan.MonthlyPrice = AnnualManualPrice;
                plan.MaxUsers = 1;
                plan.MaxLawyers = 1;
                plan.IsActive = true;
                plan.IsHighlighted = true;
                plan.SortOrder = 1;
            }

            var requiredFeatures = new[]
            {
                ("public_site", "Sitio publico"),
                ("contact_form", "Formulario de contacto"),
                ("appointments", "Agenda de citas"),
                ("editorial", "Editorial legal"),
                ("seo", "SEO basico")
            };

            foreach (var (key, name) in requiredFeatures)
            {
                if (plan.Features.Any(x => x.FeatureKey == key))
                    continue;

                _db.SubscriptionPlanFeatures.Add(new SubscriptionPlanFeature
                {
                    TenantId = 0,
                    SubscriptionPlanId = plan.Id,
                    FeatureKey = key,
                    FeatureName = name,
                    Enabled = true
                });
            }

            await _db.SaveChangesAsync();
            return plan;
        }

        private static string GenerateSecurePassword() =>
            "Ab" + Guid.NewGuid().ToString("N")[..10] + "!9";

        private static string NormalizeSubdomain(string value)
        {
            var normalized = value.Trim().ToLowerInvariant();
            normalized = Regex.Replace(normalized, @"[^a-z0-9-]", "-");
            normalized = Regex.Replace(normalized, "-+", "-").Trim('-');
            return normalized;
        }
    }

    public class AppointmentService : IAppointmentService
    {
        private readonly AppDbContext _db;
        private readonly ITenantContext _tenant;

        public AppointmentService(AppDbContext db, ITenantContext tenant)
        {
            _db = db;
            _tenant = tenant;
        }

        public async Task<List<Appointment>> GetAllAsync()
        {
            await _tenant.EnsureTenantResolvedAsync();
            return await _db.Appointments
                .Include(x => x.LawyerProfile)
                .OrderByDescending(x => x.StartAt)
                .ToListAsync();
        }

        public async Task<List<Appointment>> GetByRangeAsync(DateTime from, DateTime to)
        {
            await _tenant.EnsureTenantResolvedAsync();
            return await _db.Appointments
                .Include(x => x.LawyerProfile)
                .Where(x => x.StartAt >= from && x.StartAt <= to)
                .OrderBy(x => x.StartAt)
                .ToListAsync();
        }

        public async Task<Appointment?> GetByIdAsync(int id)
        {
            await _tenant.EnsureTenantResolvedAsync();
            return await _db.Appointments.Include(x => x.LawyerProfile).FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<CreateAppointmentResult> CreateAsync(Appointment model)
        {
            await _tenant.EnsureTenantResolvedAsync();
            if (string.IsNullOrWhiteSpace(model.ClientName))
                return Fail("El nombre del cliente es obligatorio.");

            if (string.IsNullOrWhiteSpace(model.Subject))
                return Fail("El asunto de la cita es obligatorio.");

            if (model.EndAt <= model.StartAt)
                return Fail("La hora final debe ser mayor que la hora inicial.");

            var tenantId = model.TenantId > 0
                ? model.TenantId
                : (_tenant.TenantId > 0 ? _tenant.TenantId : 1);

            var conflict = await _db.Appointments.AnyAsync(x =>
                x.TenantId == tenantId &&
                x.LawyerProfileId == model.LawyerProfileId &&
                x.Status != AppointmentStatus.Cancelled &&
                x.StartAt < model.EndAt &&
                x.EndAt > model.StartAt);

            if (conflict)
                return Fail("Ya existe una cita que se cruza con ese horario.");

            model.TenantId = tenantId;
            model.CreatedAt = DateTime.UtcNow;
            _db.Appointments.Add(model);
            await _db.SaveChangesAsync();

            return new CreateAppointmentResult
            {
                Success = true,
                Message = "Cita creada correctamente."
            };
        }

        public async Task UpdateStatusAsync(int id, AppointmentStatus status)
        {
            await _tenant.EnsureTenantResolvedAsync();
            var appt = await _db.Appointments.FirstOrDefaultAsync(x => x.Id == id);
            if (appt is null) return;

            appt.Status = status;
            appt.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            await _tenant.EnsureTenantResolvedAsync();
            var appt = await _db.Appointments.FirstOrDefaultAsync(x => x.Id == id);
            if (appt is null) return;

            _db.Appointments.Remove(appt);
            await _db.SaveChangesAsync();
        }

        private static CreateAppointmentResult Fail(string message) => new()
        {
            Success = false,
            Message = message
        };
    }

    public class RolePermissionService : IRolePermissionService
    {
        private readonly AppDbContext _db;
        private readonly ITenantContext _tenant;

        public RolePermissionService(AppDbContext db, ITenantContext tenant)
        {
            _db = db;
            _tenant = tenant;
        }

        public async Task<bool> CanViewAsync(string roleName, string moduleKey)
        {
            await _tenant.EnsureTenantResolvedAsync();
            if (_tenant.IsSuperAdmin) return true;

            var perm = await GetPermissionAsync(roleName, moduleKey);
            return perm?.CanView ?? false;
        }

        public async Task<bool> CanEditAsync(string roleName, string moduleKey)
        {
            await _tenant.EnsureTenantResolvedAsync();
            if (_tenant.IsSuperAdmin) return true;

            var perm = await GetPermissionAsync(roleName, moduleKey);
            return perm?.CanEdit ?? false;
        }

        public async Task SaveMatrixAsync(int tenantId, string roleName, IEnumerable<RoleModulePermission> permissions)
        {
            await _tenant.EnsureTenantResolvedAsync();
            var existing = await _db.RoleModulePermissions
                .IgnoreQueryFilters()
                .Where(x => x.TenantId == tenantId && x.RoleName == roleName)
                .ToListAsync();

            _db.RoleModulePermissions.RemoveRange(existing);

            foreach (var p in permissions)
            {
                p.TenantId = tenantId;
                p.RoleName = roleName;
                _db.RoleModulePermissions.Add(p);
            }

            await _db.SaveChangesAsync();
        }

        private async Task<RoleModulePermission?> GetPermissionAsync(string roleName, string moduleKey) =>
            await _db.RoleModulePermissions
                .IgnoreQueryFilters()
                .Where(x => (x.TenantId == _tenant.TenantId || x.TenantId == 0) &&
                            x.RoleName == roleName &&
                            x.ModuleKey == moduleKey)
                .OrderByDescending(x => x.TenantId)
                .FirstOrDefaultAsync();
    }

    internal static class SlugHelpers
    {
        public static string Slugify(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            var normalized = text.ToLowerInvariant().Trim();
            normalized = Regex.Replace(normalized, @"[^a-z0-9\s-]", "");
            normalized = Regex.Replace(normalized, @"\s+", "-");
            normalized = Regex.Replace(normalized, "-+", "-");
            return normalized.Trim('-');
        }
    }
}
