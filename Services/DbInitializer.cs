using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using YgoProgressionDuels.Data;
using YgoProgressionDuels.Shared;

namespace YgoProgressionDuels.Services
{
    public class DbInitializer : IDbInitializer
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public DbInitializer(IDbContextFactory<ApplicationDbContext> dbContextFactory,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager)
        {
            _dbContextFactory = dbContextFactory;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task InitializeAsync()
        {
            //// Delete existing db and apply migrations to create a new one
            //using (ApplicationDbContext dbContext = _dbContextFactory.CreateDbContext())
            //{
            //    await dbContext.Database.EnsureDeletedAsync();
            //    await dbContext.Database.MigrateAsync();
            //}

            // Migrate any pending migrations
            using (ApplicationDbContext dbContext = _dbContextFactory.CreateDbContext())
            {
                if ((await dbContext.Database.GetPendingMigrationsAsync()).Any())
                {
                    await dbContext.Database.MigrateAsync();
                }
            }

            // Ensure that the ADMIN Role exists
            if (!await _roleManager.RoleExistsAsync(Constants.ADMIN))
            {
                await _roleManager.CreateAsync(new ApplicationRole(Constants.ADMIN));
            }

            // Ensure that default admin user exists
            string adminEmail = Encoding.UTF8.GetString(Convert.FromBase64String("RGlmZmlkZW50RGVja2FyZEBZZ29Qcm9ncmVzc2lvbkR1ZWxzLmNvbQ=="));
            ApplicationUser adminUser = await _userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                await _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "DiffidentDeckard",
                    AvatarUrl = Path.Combine(Path.DirectorySeparatorChar.ToString(), "images", "avatars", "admin", "diffidentdeckard.png"),
                    Email = adminEmail,
                    EmailConfirmed = true,
                    LockoutEnabled = false
                }, Encoding.UTF8.GetString(Convert.FromBase64String("TWVsb0dydW50eV80Mg==")));

                adminUser = await _userManager.FindByEmailAsync(adminEmail);
            }

            // Ensure that the default admin user has the admin role
            if (!await _userManager.IsInRoleAsync(adminUser, Constants.ADMIN))
            {
                await _userManager.AddToRoleAsync(adminUser, Constants.ADMIN);
            }

            // Ensure that default users exists
            string defaultUser1Email = Encoding.UTF8.GetString(Convert.FromBase64String("ZGlmZmlkZW50ZGVja2FyZEBnbWFpbC5jb20="));
            ApplicationUser defaultUser1 = await _userManager.FindByEmailAsync(defaultUser1Email);
            if (defaultUser1 == null)
            {
                await _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "Diffident_Deckard",
                    AvatarUrl = Path.Combine(Path.DirectorySeparatorChar.ToString(), "images", "avatars", "admin", "diffidentdeckard.png"),
                    Email = defaultUser1Email,
                    EmailConfirmed = true,
                    LockoutEnabled = false
                }, Encoding.UTF8.GetString(Convert.FromBase64String("TWVsb0dydW50eV80Mg==")));
            }

            string defaultUser2Email = Encoding.UTF8.GetString(Convert.FromBase64String("YXJyaWFnYWdhcnlAZ21haWwuY29t"));
            ApplicationUser defaultUser2 = await _userManager.FindByEmailAsync(defaultUser2Email);
            if (defaultUser2 == null)
            {
                await _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "Deckard",
                    AvatarUrl = Path.Combine(Path.DirectorySeparatorChar.ToString(), "images", "avatars", "admin", "feirizani_deckard.png"),
                    Email = defaultUser2Email,
                    EmailConfirmed = true,
                    LockoutEnabled = false
                }, Encoding.UTF8.GetString(Convert.FromBase64String("TWVsb0dydW50eV80Mg==")));
            }
        }
    }
}
