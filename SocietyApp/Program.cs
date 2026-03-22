using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SocietyApp.Data;
using SocietyApp.Models;
using SocietyApp.Services;

namespace SocietyApp
{
    public partial class Program
    {
        private static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ── Database ──────────────────────────────────────────────────────────────────
            builder.Services.AddDbContextPool<AppDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    sqlServerOptionsAction => sqlServerOptionsAction.EnableRetryOnFailure()));

            // ── Identity ──────────────────────────────────────────────────────────────────
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/Login";
            });

            // ── App Services ──────────────────────────────────────────────────────────────
            builder.Services.AddScoped<SocietyService>();
            builder.Services.AddScoped<MemberService>();
            builder.Services.AddScoped<ParticularService>();
            builder.Services.AddScoped<ReceiptService>();
            builder.Services.AddScoped<ReportService>();
            builder.Services.AddScoped<PdfService>();

            // ── Custom Claims (add SocietyId to user claims) ──────────────────────────────
            builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>,
                SocietyClaimsPrincipalFactory>();

            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

            // ── Database Migration + Seed on startup ──────────────────────────────────────
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.Migrate();
            }
            await AppDbContext.SeedAsync(app.Services, builder.Configuration);

            app.Run();
        }
    }

    // ── Custom Claims Factory ─────────────────────────────────────────────────────
    public class SocietyClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser>
    {
        private readonly AppDbContext _db;

        public SocietyClaimsPrincipalFactory(UserManager<ApplicationUser> userManager,
            IOptions<IdentityOptions> optionsAccessor, AppDbContext db) : base(userManager, optionsAccessor)
        {
            _db = db;
        }

        protected override async Task<System.Security.Claims.ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);

            if (user.SocietyId.HasValue)
            {
                identity.AddClaim(new System.Security.Claims.Claim("SocietyId", user.SocietyId.Value.ToString()));
                var society = await _db.Societies.FindAsync(user.SocietyId.Value);
                if (society != null)
                    identity.AddClaim(new System.Security.Claims.Claim("SocietyName", society.Name));
            }

            identity.AddClaim(new System.Security.Claims.Claim("UserRole", user.Role));
            return identity;
        }
    }
}