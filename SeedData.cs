using Alumni_Management_System.Models;
using Alumni_Management_System.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Alumni_Management_System
{
    public class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<ApplicationDbContext>();

            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            await EnsureRolesAsync(roleManager);

            var userManager = services.GetRequiredService<UserManager<AppUser>>();
            await EnsureTestUsersAsync(userManager);

            await EnsureSampleDataAsync(context, userManager);
        }

        public static async Task ClearAllDataAsync(ApplicationDbContext context)
        {

            context.AlumniEmployments.RemoveRange(context.AlumniEmployments);
            context.AlumniInternships.RemoveRange(context.AlumniInternships);
            context.AlumniMessages.RemoveRange(context.AlumniMessages);
            context.AlumniOrganizations.RemoveRange(context.AlumniOrganizations);
            context.AlumniDegrees.RemoveRange(context.AlumniDegrees);
            context.Messages.RemoveRange(context.Messages);

            context.Alumni.RemoveRange(context.Alumni);
            context.AlumniRegistries.RemoveRange(context.AlumniRegistries);

            context.Employers.RemoveRange(context.Employers);
            context.DegreePrograms.RemoveRange(context.DegreePrograms);
            context.OrganizationTypes.RemoveRange(context.OrganizationTypes);

            await context.Database.ExecuteSqlRawAsync("DELETE FROM [AspNetUserRoles]");
            context.Users.RemoveRange(context.Users);
            context.Roles.RemoveRange(context.Roles);

            await context.SaveChangesAsync();
        }
        public static async Task EnsureRolesAsync(RoleManager<IdentityRole>
        roleManager)
        {
            var roles = new[] { Constants.AdminRole, Constants.AlumniRole, Constants.StaffRole };

            foreach (var role in roles)
            {
                var roleExists = await roleManager.RoleExistsAsync(role);
                if (!roleExists)
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
        public static async Task EnsureTestUsersAsync(UserManager<AppUser> userManager)
        {

            if (!await userManager.Users.AnyAsync(x => x.UserName == "admin"))
            {
                var admin = new AppUser
                {
                    UserName = "admin",
                    Email = "admin@university.edu",
                    EmailConfirmed = true,
                    JagId = null,
                    CreatedAt = DateTime.Now,
                    IsFirstLogin = false
                };
                var createAdminResult = await userManager.CreateAsync(admin, "Admin@123");
                if (!createAdminResult.Succeeded)
                {
                    throw new InvalidOperationException("Admin user creation failed: " + string.Join(", ", createAdminResult.Errors.Select(e => e.Description)));
                }
                var roleResult = await userManager.AddToRoleAsync(admin, Constants.AdminRole);
                if (!roleResult.Succeeded)
                {
                    throw new InvalidOperationException("Admin AddToRole failed: " + string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                }
            }

            if (!await userManager.Users.AnyAsync(x => x.UserName == "staff"))
            {
                var staff = new AppUser
                {
                    UserName = "staff",
                    Email = "staff@university.edu",
                    EmailConfirmed = true,
                    JagId = null,
                    CreatedAt = DateTime.Now,
                    IsFirstLogin = false
                };
                var createStaffResult = await userManager.CreateAsync(staff, "Staff@123");
                if (!createStaffResult.Succeeded)
                {
                    throw new InvalidOperationException("Staff user creation failed: " + string.Join(", ", createStaffResult.Errors.Select(e => e.Description)));
                }
                var staffRoleResult = await userManager.AddToRoleAsync(staff, Constants.StaffRole);
                if (!staffRoleResult.Succeeded)
                {
                    throw new InvalidOperationException("Staff AddToRole failed: " + string.Join(", ", staffRoleResult.Errors.Select(e => e.Description)));
                }
            }

            var alumniUsers = new[]
            {
                new { Username = "alumni", Email = "alumni@university.edu", JagId = "J0012345", FirstName = "Alumni", LastName = "Sample" }
            };

            foreach (var alumniData in alumniUsers)
            {
                if (!await userManager.Users.AnyAsync(x => x.UserName == alumniData.Username))
                {
                    var alumniUser = new AppUser
                    {
                        UserName = alumniData.Username,
                        Email = alumniData.Email,
                        EmailConfirmed = true,
                        JagId = alumniData.JagId,
                        CreatedAt = DateTime.Now,
                        IsFirstLogin = false
                    };
                    var createAlumniResult = await userManager.CreateAsync(alumniUser, "Alumni@123");
                    if (!createAlumniResult.Succeeded)
                    {
                        throw new InvalidOperationException("Alumni user creation failed for " + alumniData.Username + ": " + string.Join(", ", createAlumniResult.Errors.Select(e => e.Description)));
                    }
                    var alumniRoleResult = await userManager.AddToRoleAsync(alumniUser, Constants.AlumniRole);
                    if (!alumniRoleResult.Succeeded)
                    {
                        throw new InvalidOperationException("Alumni role assignment failed for " + alumniData.Username + ": " + string.Join(", ", alumniRoleResult.Errors.Select(e => e.Description)));
                    }
                }
            }
        }

        public static async Task EnsureSampleDataAsync(ApplicationDbContext context, UserManager<AppUser> userManager)
        {

            if (!await context.AlumniRegistries.AnyAsync())
            {
                var registries = new[]
                {
                    new AlumniRegistry { JagId = "J0012345", FirstName = "Alumni", LastName = "Sample", AccountCreated = true }
                };
                await context.AlumniRegistries.AddRangeAsync(registries);
                await context.SaveChangesAsync();
            }

            if (!await context.Alumni.AnyAsync())
            {
                var alumniSeed = new[]
                {
                    new Alumni { JagId = "J0012345", FirstName = "Sample", LastName = "Alumni", PermanentEmail = "alumni@university.edu", GraduationYear = 2018, IsActive = true, Privacy = false, LastUpdated = DateTime.Now }
                };
                await context.Alumni.AddRangeAsync(alumniSeed);
                await context.SaveChangesAsync();
            }

            var alumniUser = await userManager.FindByNameAsync("alumni");
            var sampleAlumni = await context.Alumni.FirstOrDefaultAsync(a => a.JagId == "J0012345");
            if (alumniUser != null && sampleAlumni != null)
            {
                sampleAlumni.UserId = alumniUser.Id;
                context.Alumni.Update(sampleAlumni);
                await context.SaveChangesAsync();
            }

            return;
        }
    }
}
