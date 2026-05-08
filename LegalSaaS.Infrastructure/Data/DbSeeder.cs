using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using LegalSaaS.Domain.Entities;
using LegalSaaS.Infrastructure.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LegalSaaS.Infrastructure.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var sp = scope.ServiceProvider;

            var db = sp.GetRequiredService<AppDbContext>();
            var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();

            await db.Database.MigrateAsync();

            string[] roles = ["SuperAdmin", "Admin", "Editor", "Lawyer", "Assistant"];
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            await SeedPlansAsync(db);
            await SeedDefaultPermissionsAsync(db);

            var superAdminEmail = "superadmin@legalsaas.local";
            var superAdmin = await userManager.FindByEmailAsync(superAdminEmail);

            if (superAdmin is null)
            {
                superAdmin = new ApplicationUser
                {
                    UserName = superAdminEmail,
                    Email = superAdminEmail,
                    EmailConfirmed = true,
                    TenantId = 0,
                    FullName = "Super Admin LegalSaaS"
                };

                var result = await userManager.CreateAsync(superAdmin, "SuperAdmin1234!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(superAdmin, "SuperAdmin");
                }
            }

            if (!await db.Tenants.AnyAsync())
            {
                var tenant = new Tenant
                {
                    Name = "Estudio Jurídico Ab Zoila Segura Egas",
                    Subdomain = "abogadaseguraegas",
                    IsActive = true,
                    TrialEndsAt = DateTime.UtcNow.AddDays(365)
                };

                db.Tenants.Add(tenant);
                await db.SaveChangesAsync();

                db.LawFirmSettings.Add(new LawFirmSettings
                {
                    TenantId = tenant.Id,
                    Name = "Estudio Jurídico Segura",
                    LegalName = "Estudio Jurídico Segura",
                    BannerTitle = "Defensa legal seria, humana y confiable",
                    BannerSubtitle = "Civil, familia, niñez y adolescencia",
                    HeroTitle = "Defensa legal seria, humana y confiable",
                    HeroSubtitle = "Asesoría y patrocinio jurídico con enfoque profesional.",
                    EditorialBioTitle = "Una practica legal cercana, estrategica y profundamente humana",
                    EditorialBioText = "El estudio combina criterio juridico, comunicacion clara y tecnologia para acompanar decisiones sensibles con rigor y calma.",
                    Description = "Asesoría y patrocinio jurídico con enfoque profesional, tecnología y acompañamiento claro.",
                    City = "Babahoyo",
                    Phone = "+593 99 000 0000",
                    WhatsApp = "+593990000000",
                    Email = "admin@segura.com"
                });

                db.LawyerProfiles.Add(new LawyerProfile
                {
                    TenantId = tenant.Id,
                    FullName = "Abogada Segura",
                    Title = "Abogada en derecho civil, familia, niñez y adolescencia",
                    Biography = "Acompañamiento legal estrategico para familias, empresas y personas que necesitan decisiones claras.",
                    AcademicBackground = "Derecho civil, familia, niñez y adolescencia.",
                    ExperienceYears = "10+",
                    RegistrationNumber = "FORO-EC-0000",
                    Email = "admin@segura.com",
                    WhatsApp = "+593990000000",
                    PublicSlug = "abogadazoila-segura-egas"
                });

                db.ServiceItems.AddRange(
                    new ServiceItem
                    {
                        TenantId = tenant.Id,
                        Title = "Derecho de familia",
                        Slug = "derecho-de-familia",
                        Summary = "Divorcios, alimentos, tenencias y acuerdos con enfoque humano.",
                        SortOrder = 1,
                        ShowOnHome = true
                    },
                    new ServiceItem
                    {
                        TenantId = tenant.Id,
                        Title = "Derecho civil",
                        Slug = "derecho-civil",
                        Summary = "Contratos, obligaciones, procesos y defensa patrimonial.",
                        SortOrder = 2,
                        ShowOnHome = true
                    },
                    new ServiceItem
                    {
                        TenantId = tenant.Id,
                        Title = "Consulta estrategica",
                        Slug = "consulta-estrategica",
                        Summary = "Diagnostico legal claro antes de tomar decisiones importantes.",
                        SortOrder = 3,
                        ShowOnHome = true
                    });

                db.Testimonials.AddRange(
                    new Testimonial
                    {
                        TenantId = tenant.Id,
                        ClientName = "Cliente verificado",
                        ReviewText = "Recibi orientacion clara desde la primera consulta y pude tomar decisiones con tranquilidad.",
                        Content = "Recibi orientacion clara desde la primera consulta y pude tomar decisiones con tranquilidad.",
                        Source = "Consulta privada",
                        Rating = 5,
                        IsApproved = true
                    },
                    new Testimonial
                    {
                        TenantId = tenant.Id,
                        ClientName = "Familia asesorada",
                        ReviewText = "El acompanamiento fue ordenado, humano y muy profesional durante todo el proceso.",
                        Content = "El acompanamiento fue ordenado, humano y muy profesional durante todo el proceso.",
                        Source = "Caso de familia",
                        Rating = 5,
                        IsApproved = true
                    });

                var starterPlan = await db.SubscriptionPlans.FirstAsync(x => x.Code == "annual_manual");
                db.TenantSubscriptions.Add(new TenantSubscription
                {
                    TenantId = tenant.Id,
                    SubscriptionPlanId = starterPlan.Id,
                    Status = SubscriptionStatus.Active,
                    StartAt = DateTime.UtcNow,
                    EndAt = tenant.TrialEndsAt,
                    NextBillingAt = tenant.TrialEndsAt,
                    PriceAtSubscription = 100,
                    PaymentProvider = "Offline / Manual"
                });

                await db.SaveChangesAsync();
            }

            var tenantFirst = await db.Tenants.OrderBy(x => x.Id).FirstAsync();

            var adminEmail = "admin@segura.com";
            var admin = await userManager.FindByEmailAsync(adminEmail);

            if (admin is null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    TenantId = tenantFirst.Id,
                    FullName = "Administrador Segura"
                };

                var result = await userManager.CreateAsync(admin, "Admin1234!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }

                tenantFirst.OwnerUserId = admin.Id;
                await db.SaveChangesAsync();
            }

            await BackfillHeroSettingsAsync(db);
        }

        private static async Task SeedPlansAsync(AppDbContext db)
        {
            var annual = await db.SubscriptionPlans
                .Include(x => x.Features)
                .FirstOrDefaultAsync(x => x.Code == "annual_manual");

            if (annual is null)
            {
                annual = new SubscriptionPlan
                {
                    TenantId = 0,
                    Code = "annual_manual",
                    Name = "Plan Anual Manual",
                    MonthlyPrice = 100,
                    MaxUsers = 1,
                    MaxLawyers = 1,
                    IsHighlighted = true,
                    SortOrder = 1,
                    IsActive = true
                };

                db.SubscriptionPlans.Add(annual);
                await db.SaveChangesAsync();
            }
            else
            {
                annual.Name = "Plan Anual Manual";
                annual.MonthlyPrice = 100;
                annual.MaxUsers = 1;
                annual.MaxLawyers = 1;
                annual.IsHighlighted = true;
                annual.SortOrder = 1;
                annual.IsActive = true;
            }

            var legacyPlans = await db.SubscriptionPlans
                .Where(x => x.Code != "annual_manual")
                .ToListAsync();

            foreach (var legacy in legacyPlans)
            {
                legacy.IsActive = false;
                legacy.IsHighlighted = false;
            }

            var features = new List<(string Key, string Name)>
            {
                ("public_site", "Sitio publico"),
                ("contact_form", "Formulario de contacto"),
                ("appointments", "Agenda de citas"),
                ("editorial", "Editorial legal"),
                ("seo", "SEO basico")
            };

            foreach (var feature in features)
            {
                var existing = annual.Features.FirstOrDefault(x => x.FeatureKey == feature.Key);
                if (existing is null)
                {
                    db.SubscriptionPlanFeatures.Add(new SubscriptionPlanFeature
                    {
                        SubscriptionPlanId = annual.Id,
                        TenantId = 0,
                        FeatureKey = feature.Key,
                        FeatureName = feature.Name,
                        Enabled = true
                    });
                }
                else
                {
                    existing.FeatureName = feature.Name;
                    existing.Enabled = true;
                }
            }

            await db.SaveChangesAsync();
        }

        private static async Task BackfillHeroSettingsAsync(AppDbContext db)
        {
            var settings = await db.LawFirmSettings.IgnoreQueryFilters().ToListAsync();
            foreach (var firm in settings)
            {
                if (string.IsNullOrWhiteSpace(firm.HeroTitle))
                    firm.HeroTitle = !string.IsNullOrWhiteSpace(firm.BannerTitle)
                        ? firm.BannerTitle
                        : "Defensa legal seria y confiable";

                if (string.IsNullOrWhiteSpace(firm.HeroSubtitle))
                    firm.HeroSubtitle = !string.IsNullOrWhiteSpace(firm.BannerSubtitle)
                        ? firm.BannerSubtitle
                        : "Asesoría y patrocinio jurídico con enfoque profesional.";

                if (string.IsNullOrWhiteSpace(firm.PrimaryColor))
                    firm.PrimaryColor = "#0B192C";

                if (string.IsNullOrWhiteSpace(firm.HeaderBackgroundColor))
                    firm.HeaderBackgroundColor = "#FFFFFF";
            }

            await db.SaveChangesAsync();
        }

        private static async Task SeedDefaultPermissionsAsync(AppDbContext db)
        {
            if (await db.RoleModulePermissions.IgnoreQueryFilters().AnyAsync(x => x.TenantId == 0))
                return;

            var modules = new[]
            {
                ModuleKeys.Dashboard, ModuleKeys.Settings, ModuleKeys.LawyerProfile, ModuleKeys.Services,
                ModuleKeys.Editorial, ModuleKeys.Team, ModuleKeys.Testimonials, ModuleKeys.Contacts,
                ModuleKeys.Appointments, ModuleKeys.Subscriptions, ModuleKeys.Tenants, ModuleKeys.Permissions
            };

            foreach (var module in modules)
            {
                db.RoleModulePermissions.Add(new RoleModulePermission
                {
                    TenantId = 0,
                    RoleName = "SuperAdmin",
                    ModuleKey = module,
                    CanView = true,
                    CanCreate = true,
                    CanEdit = true,
                    CanDelete = true,
                    CanPublish = true
                });

                db.RoleModulePermissions.Add(new RoleModulePermission
                {
                    TenantId = 0,
                    RoleName = "Admin",
                    ModuleKey = module,
                    CanView = true,
                    CanCreate = true,
                    CanEdit = true,
                    CanDelete = true,
                    CanPublish = true
                });
            }

            await db.SaveChangesAsync();
        }
    }
}
