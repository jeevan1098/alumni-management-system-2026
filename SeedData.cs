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
                    CreatedAt = DateTime.Now
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
                    CreatedAt = DateTime.Now
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
                new { Username = "john_doe", Email = "john.doe@email.com", JagId = "J0012345", FirstName = "John", LastName = "Doe" },
                new { Username = "jane_smith", Email = "jane.smith@email.com", JagId = "J0012346", FirstName = "Jane", LastName = "Smith" },
                new { Username = "michael.j", Email = "michael.johnson@email.com", JagId = "J0012347", FirstName = "Michael", LastName = "Johnson" },
                new { Username = "sarah.w", Email = "sarah.williams@email.com", JagId = "J0012348", FirstName = "Sarah", LastName = "Williams" },
                new { Username = "david_brown", Email = "david.brown@email.com", JagId = "J0012349", FirstName = "David", LastName = "Brown" }
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
                        CreatedAt = DateTime.Now
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

            if (!await context.DegreePrograms.AnyAsync())
            {
                var degreePrograms = new[]
                {
                    new DegreeProgram { Institution = "University", DegreeType = "BS", MajorFieldOfStudy = "Computer Science", Department = "Computer Science" },
                    new DegreeProgram { Institution = "University", DegreeType = "MS", MajorFieldOfStudy = "Computer Science", Department = "Computer Science" },
                    new DegreeProgram { Institution = "University", DegreeType = "BBA", MajorFieldOfStudy = "Business Administration", Department = "Business" },
                    new DegreeProgram { Institution = "University", DegreeType = "MBA", MajorFieldOfStudy = "Business Administration", Department = "Business" },
                    new DegreeProgram { Institution = "University", DegreeType = "BE", MajorFieldOfStudy = "Engineering", Department = "Engineering" },
                    new DegreeProgram { Institution = "University", DegreeType = "MS", MajorFieldOfStudy = "Data Science", Department = "Computer Science" },
                    new DegreeProgram { Institution = "University", DegreeType = "BIT", MajorFieldOfStudy = "Information Technology", Department = "Information Technology" }
                };
                await context.DegreePrograms.AddRangeAsync(degreePrograms);
                await context.SaveChangesAsync();
            }

            if (!await context.Employers.AnyAsync())
            {
                var employers = new[]
                {
                    new Employer { EmployerName = "Microsoft Corporation", Industry = "Technology", Location = "Redmond, WA" },
                    new Employer { EmployerName = "Google LLC", Industry = "Technology", Location = "Mountain View, CA" },
                    new Employer { EmployerName = "Amazon.com Inc", Industry = "E-commerce/Technology", Location = "Seattle, WA" },
                    new Employer { EmployerName = "JPMorgan Chase & Co", Industry = "Finance", Location = "New York, NY" },
                    new Employer { EmployerName = "Deloitte", Industry = "Consulting", Location = "New York, NY" },
                    new Employer { EmployerName = "IBM", Industry = "Technology", Location = "Armonk, NY" },
                    new Employer { EmployerName = "Accenture", Industry = "Consulting", Location = "Dublin, Ireland" }
                };
                await context.Employers.AddRangeAsync(employers);
                await context.SaveChangesAsync();
            }

            if (!await context.OrganizationTypes.AnyAsync())
            {
                var orgTypes = new[]
                {
                    new OrganizationType { OrganizationName = "Student Government Association" },
                    new OrganizationType { OrganizationName = "Computer Science Club" },
                    new OrganizationType { OrganizationName = "Business Leaders Society" },
                    new OrganizationType { OrganizationName = "Volunteer Corps" },
                    new OrganizationType { OrganizationName = "Athletics Association" }
                };
                await context.OrganizationTypes.AddRangeAsync(orgTypes);
                await context.SaveChangesAsync();
            }

            if (!await context.AlumniRegistries.AnyAsync())
            {
                var registries = new[]
                {
                    new AlumniRegistry { JagId = "J0012345", FirstName = "John", LastName = "Doe", AccountCreated = true },
                    new AlumniRegistry { JagId = "J0012346", FirstName = "Jane", LastName = "Smith", AccountCreated = true },
                    new AlumniRegistry { JagId = "J0012347", FirstName = "Michael", LastName = "Johnson", AccountCreated = true },
                    new AlumniRegistry { JagId = "J0012348", FirstName = "Sarah", LastName = "Williams", AccountCreated = true },
                    new AlumniRegistry { JagId = "J0012349", FirstName = "David", LastName = "Brown", AccountCreated = true },
                    new AlumniRegistry { JagId = "J0012350", FirstName = "Emily", LastName = "Davis", AccountCreated = false },
                    new AlumniRegistry { JagId = "J0012351", FirstName = "Robert", LastName = "Miller", AccountCreated = false }
                };
                await context.AlumniRegistries.AddRangeAsync(registries);
                await context.SaveChangesAsync();
            }

            if (!await context.Alumni.AnyAsync())
            {
                var alumniSeed = new[]
                {
                    new Alumni { JagId = "J0012345", FirstName = "John", LastName = "Doe", PermanentEmail = "john.doe@university.edu", GraduationYear = 2018, IsActive = true, Privacy = true, LastUpdated = DateTime.Now },
                    new Alumni { JagId = "J0012346", FirstName = "Jane", LastName = "Smith", PermanentEmail = "jane.smith@university.edu", GraduationYear = 2019, IsActive = true, Privacy = true, LastUpdated = DateTime.Now },
                    new Alumni { JagId = "J0012347", FirstName = "Michael", LastName = "Johnson", PermanentEmail = "michael.johnson@university.edu", GraduationYear = 2017, IsActive = true, Privacy = true, LastUpdated = DateTime.Now }
                };
                await context.Alumni.AddRangeAsync(alumniSeed);
                await context.SaveChangesAsync();
            }

            var johnUser = await userManager.FindByNameAsync("john_doe");
            var johnAlumni = await context.Alumni.FirstOrDefaultAsync(a => a.JagId == "J0012345");
            if (johnUser != null && johnAlumni != null)
            {
                johnAlumni.UserId = johnUser.Id;
                context.Alumni.Update(johnAlumni);
                await context.SaveChangesAsync();
            }

            return;
        }
    }
}
