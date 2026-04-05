using Alumni_Management_System.Data;
using Alumni_Management_System.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Alumni_Management_System
{
    public class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<AppUser>>();
            var context = services.GetRequiredService<ApplicationDbContext>();

            // Ensure roles exist
            await EnsureRolesAsync(roleManager);

            // Ensure minimal users exist
            await EnsureTestUsersAsync(userManager, context);
        }

        private static async Task EnsureRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            var roles = new[] { Constants.AdminRole, Constants.StaffRole, Constants.AlumniRole };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var result = await roleManager.CreateAsync(new IdentityRole(role));
                    if (!result.Succeeded)
                        throw new InvalidOperationException($"Failed to create role {role}: " +
                            string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }

        private static async Task EnsureTestUsersAsync(UserManager<AppUser> userManager, ApplicationDbContext context)
        {
            // Admin
            if (!await userManager.Users.AnyAsync(x => x.UserName == "admin"))
            {
                var admin = new AppUser
                {
                    UserName = "admin",
                    Email = "admin@university.edu",
                    EmailConfirmed = true,
                    JagId = "J0010000", // required
                    CreatedAt = DateTime.Now
                };
                var createAdmin = await userManager.CreateAsync(admin, "Admin@123");
                if (!createAdmin.Succeeded)
                    throw new InvalidOperationException("Admin creation failed: " +
                        string.Join(", ", createAdmin.Errors.Select(e => e.Description)));

                await userManager.AddToRoleAsync(admin, Constants.AdminRole);
            }

            // Staff
            if (!await userManager.Users.AnyAsync(x => x.UserName == "staff"))
            {
                var staff = new AppUser
                {
                    UserName = "staff",
                    Email = "staff@university.edu",
                    EmailConfirmed = true,
                    JagId = "J0010001",
                    CreatedAt = DateTime.Now
                };
                var createStaff = await userManager.CreateAsync(staff, "Staff@123");
                if (!createStaff.Succeeded)
                    throw new InvalidOperationException("Staff creation failed: " +
                        string.Join(", ", createStaff.Errors.Select(e => e.Description)));

                await userManager.AddToRoleAsync(staff, Constants.StaffRole);
            }

            // Alumni
            if (!await userManager.Users.AnyAsync(x => x.UserName == "alumni"))
            {
                var alumniUser = new AppUser
                {
                    UserName = "alumni",
                    Email = "alumni@university.edu",
                    EmailConfirmed = true,
                    JagId = "J0010002",
                    CreatedAt = DateTime.Now
                };
                var createAlumni = await userManager.CreateAsync(alumniUser, "Alumni@123");
                if (!createAlumni.Succeeded)
                    throw new InvalidOperationException("Alumni creation failed: " +
                        string.Join(", ", createAlumni.Errors.Select(e => e.Description)));

                await userManager.AddToRoleAsync(alumniUser, Constants.AlumniRole);

                // Add to AlumniRegistry
                if (!await context.AlumniRegistries.AnyAsync(a => a.JagId == alumniUser.JagId))
                {
                    var registry = new AlumniRegistry
                    {
                        JagId = alumniUser.JagId,
                        FirstName = "Sample",
                        LastName = "Alumni",
                        AccountCreated = true
                    };
                    context.AlumniRegistries.Add(registry);
                    await context.SaveChangesAsync();
                }

                // Add to Alumni table
                if (!await context.Alumni.AnyAsync(a => a.JagId == alumniUser.JagId))
                {
                    var alumni = new Alumni
                    {
                        JagId = alumniUser.JagId,
                        UserId = alumniUser.Id,
                        FirstName = "Sample",
                        LastName = "Alumni",
                        PermanentEmail = alumniUser.Email,
                        GraduationYear = 2020,
                        IsActive = true,
                        Privacy = true,
                        LastUpdated = DateTime.Now
                    };
                    context.Alumni.Add(alumni);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}