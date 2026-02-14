using Alumni_Management_System.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Alumni_Management_System
{
    public class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            var roleManager =
            services.GetRequiredService<RoleManager<IdentityRole>>();
            await EnsureRolesAsync(roleManager);
            var userManager =
            services.GetRequiredService<UserManager<AppUser>>();
            await EnsureTestAdminAsync(userManager);
        }
        public static async Task EnsureRolesAsync(RoleManager<IdentityRole>
        roleManager)
        {
            var adminAlreadyExists = await roleManager.RoleExistsAsync
            (Constants.AdminRole);
            if (adminAlreadyExists)
            {
                return;
            }
            await roleManager.CreateAsync(new IdentityRole
            (Constants.AdminRole));
            var stdUserAlreadyExists = await roleManager.RoleExistsAsync
            (Constants.StandardUserRole);
            if (stdUserAlreadyExists)
            {
                return;
            }
            await roleManager.CreateAsync(new IdentityRole
            (Constants.StandardUserRole));
        }
        public static async Task EnsureTestAdminAsync
        (UserManager<AppUser> userManager)
        {
            var testAdmin = await userManager.Users.Where(x => x.UserName
            == "admin@admin.com").SingleOrDefaultAsync();
            if (testAdmin != null)
            {
                return;
            }
            testAdmin = new AppUser
            {
                UserName = "admin@admin.com",
                Email = "admin@admin.com",
                JagId = "ADMIN001",
                CreatedAt = DateTime.Now
            };
            await userManager.CreateAsync(testAdmin, "Password1!");
            await userManager.AddToRoleAsync(testAdmin,
            Constants.AdminRole);
        }
    }
}
