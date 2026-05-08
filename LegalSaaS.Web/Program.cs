using LegalSaaS.Application.Interfaces;
using LegalSaaS.Infrastructure.Data;
using LegalSaaS.Infrastructure.Identity;
using LegalSaaS.Infrastructure.Services;
using LegalSaaS.Web.Components;
using LegalSaaS.Web.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Configuración de Tenant y DbContext
var dataProtectionKeysPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "DataProtectionKeys");
Directory.CreateDirectory(dataProtectionKeysPath);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath))
    .SetApplicationName("LegalSaaS");

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantContext, HttpTenantContext>();
builder.Services.AddTransient<ILawFirmSettingsService, LawFirmSettingsService>();
builder.Services.AddTransient<ILawyerProfileService, LawyerProfileService>();
builder.Services.AddTransient<IServiceItemService, ServiceItemService>();
builder.Services.AddTransient<IEditorialPostService, EditorialPostService>();
builder.Services.AddTransient<ITeamMemberService, TeamMemberService>();
builder.Services.AddTransient<ITestimonialService, TestimonialService>();
builder.Services.AddTransient<IContactMessageService, ContactMessageService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddTransient<ISeoService, SeoService>();
builder.Services.AddTransient<IDashboardMetricsService, DashboardMetricsService>();
builder.Services.AddTransient<ISuperAdminTenantService, SuperAdminTenantService>();
builder.Services.AddTransient<IAppointmentService, AppointmentService>();
builder.Services.AddTransient<IRolePermissionService, RolePermissionService>();
builder.Services.AddDbContext<AppDbContext>(
    options => options
        .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
        .ConfigureWarnings(warnings =>
            warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)),
    contextLifetime: ServiceLifetime.Transient,
    optionsLifetime: ServiceLifetime.Singleton);

// Configuración de Identity
builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<AppDbContext>()
.AddSignInManager()
.AddDefaultTokenProviders();

builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, TenantClaimsPrincipalFactory>();

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddIdentityCookies();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.AccessDeniedPath = "/login?denied=1";
});

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Ejecutar el Seeder al iniciar
using (var scope = app.Services.CreateScope())
{
    await DbSeeder.SeedAsync(scope.ServiceProvider);
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseMiddleware<LegalSaaS.Web.Middlewares.TenantResolutionMiddleware>();
app.UseAuthorization();
app.UseAntiforgery();

app.MapPost("/account/login", async (
    [FromForm] LoginEndpointRequest request,
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    AppDbContext db) =>
{
    var email = request.Email.Trim().ToLowerInvariant();
    var user = await userManager.FindByEmailAsync(email);

    if (user is null)
    {
        return Results.Redirect($"/login?error=1&email={Uri.EscapeDataString(email)}");
    }

    var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, false);
    if (!result.Succeeded)
    {
        return Results.Redirect($"/login?error=1&email={Uri.EscapeDataString(email)}");
    }

    var isSuperAdmin = await userManager.IsInRoleAsync(user, "SuperAdmin");
    if (!isSuperAdmin)
    {
        var tenant = await db.Tenants.FirstOrDefaultAsync(x => x.Id == user.TenantId);
        var isBlocked = tenant is null ||
                        !tenant.IsActive ||
                        (tenant.TrialEndsAt.HasValue && tenant.TrialEndsAt.Value < DateTime.UtcNow);

        if (isBlocked)
        {
            return Results.Redirect($"/login?expired=1&email={Uri.EscapeDataString(email)}");
        }
    }

    await signInManager.SignInAsync(user, request.RememberMe);

    var target = isSuperAdmin
        ? "/superadmin/dashboard"
        : "/admin/dashboard";

    return Results.Redirect(string.IsNullOrWhiteSpace(request.ReturnUrl) ? target : request.ReturnUrl);
}).DisableAntiforgery();

app.MapPost("/account/logout", async (SignInManager<ApplicationUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.Redirect("/login");
}).DisableAntiforgery();

app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

app.Run();

public sealed class LoginEndpointRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
    public string? ReturnUrl { get; set; }
}
