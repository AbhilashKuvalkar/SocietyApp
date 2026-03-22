using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SocietyApp.Models;

namespace SocietyApp.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Society> Societies => Set<Society>();
        public DbSet<Member> Members => Set<Member>();
        public DbSet<Particular> Particulars => Set<Particular>();
        public DbSet<Receipt> Receipts => Set<Receipt>();
        public DbSet<ReceiptLineItem> ReceiptLineItems => Set<ReceiptLineItem>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Society>()
                .HasIndex(s => s.SocietyCode)
                .IsUnique();

            builder.Entity<Receipt>()
                .HasIndex(r => r.ReceiptNumber)
                .IsUnique();

            builder.Entity<Particular>()
                .Property(p => p.DefaultAmount)
                .HasPrecision(18, 2);

            builder.Entity<ReceiptLineItem>()
                .Property(r => r.Amount)
                .HasPrecision(18, 2);

            builder.Entity<Receipt>()
                .Property(r => r.TotalAmount)
                .HasPrecision(18, 2);

            // SQL Server raises an error when multiple cascade paths lead to the same
            // table. Receipt references both Society and Member (which also references
            // Society), and ReceiptLineItem references both Receipt and Particular
            // (which also references Society). Disable cascades on the FK sides that
            // form the cycle; deletes must be handled explicitly in the application.
            builder.Entity<Receipt>()
                .HasOne(r => r.Society)
                .WithMany(s => s.Receipts)
                .HasForeignKey(r => r.SocietyId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Receipt>()
                .HasOne(r => r.Member)
                .WithMany(m => m.Receipts)
                .HasForeignKey(r => r.MemberId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<ReceiptLineItem>()
                .HasOne(l => l.Particular)
                .WithMany(p => p.LineItems)
                .HasForeignKey(l => l.ParticularId)
                .OnDelete(DeleteBehavior.NoAction);
        }

        // ── Default particular names used as a template for every new society ────
        // Kept here so the definition lives in one place alongside the schema.
        public static readonly (string Name, int Order)[] DefaultParticulars =
        {
            ("Municipal Taxes",                 1),
            ("Water Charges",                   2),
            ("Maintenance Charges",             3),
            ("Sinking Fund",                    4),
            ("Maintenance to Association.",     5),
            ("Late Fees",                       6),
            ("Non Occupation Charges (Sublet)", 7),
            ("Other Charges",                   8),
        };

        /// <summary>
        /// Seeds a standard set of Particulars for a given society.
        /// Inserts only items that do not already exist for that society,
        /// so it is safe to call on every app start or after a new society is created.
        /// </summary>
        public async Task SeedDefaultParticularsAsync(int societyId)
        {
            var existing = await Particulars
                .Where(p => p.SocietyId == societyId)
                .Select(p => p.Name)
                .ToListAsync();

            var toAdd = DefaultParticulars
                .Where(d => !existing.Contains(d.Name))
                .Select(d => new Particular
                {
                    SocietyId = societyId,
                    Name = d.Name,
                    DisplayOrder = d.Order,
                    IsActive = true,
                })
                .ToList();

            if (toAdd.Count > 0)
            {
                Particulars.AddRange(toAdd);
                await SaveChangesAsync();
            }
        }

        /// <summary>
        /// Application-level seed: runs once at startup.
        /// Creates a default admin user and demo society only when the tables are empty.
        /// </summary>
        public static async Task SeedAsync(IServiceProvider services, IConfiguration config)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // ── Roles ──────────────────────────────────────────────────────────────
            string[] roles = { "Chairman", "Secretary", "Treasurer", "Member" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // ── Super Admin — no society mapped, created only if Users table is empty ──
            if (!await db.Users.AnyAsync())
            {
                var superAdmin = new ApplicationUser
                {
                    UserName = "superadmin@society.com",
                    Email = "superadmin@society.com",
                    Role = "Chairman",
                    EmailConfirmed = true,
                    // SocietyId intentionally left null
                };
                var superAdminPwd = config["SeedPasswords:SuperAdmin"] ?? 
                    throw new InvalidOperationException("SeedPasswords:SuperAdmin is not configured.");
                await userManager.CreateAsync(superAdmin, superAdminPwd);
            }

            //// ── Demo Society (only if Societies table is empty) ────────────────────
            //if (!await db.Societies.AnyAsync())
            //{
            //    var society = new Society
            //    {
            //        Name = "Green Park Co-Op Hsg. Soc. Ltd.",
            //        RegistrationNumber = "MH/MUM/2001/1234",
            //        Address = "Plot No. 5, Sector 7, Green Park, Mumbai - 400001",
            //        SocietyCode = "GREENPARK",
            //        IsActive = true,
            //    };
            //    db.Societies.Add(society);
            //    await db.SaveChangesAsync();
            //    await db.SeedDefaultParticularsAsync(society.Id);
            //}
        }
    }
}
