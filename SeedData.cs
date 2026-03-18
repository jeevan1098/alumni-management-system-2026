using Alumni_Management_System.Models;
using Alumni_Management_System.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Alumni_Management_System
{
    public class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            await EnsureRolesAsync(roleManager);

            var userManager = services.GetRequiredService<UserManager<AppUser>>();
            await EnsureTestUsersAsync(userManager);

            var context = services.GetRequiredService<ApplicationDbContext>();
            await EnsureSampleDataAsync(context, userManager);
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
            // Admin User
            if (!await userManager.Users.AnyAsync(x => x.UserName == "admin"))
            {
                var admin = new AppUser
                {
                    UserName = "admin",
                    Email = "admin@university.edu",
                    EmailConfirmed = true,
                    JagId = "J0000001",
                    CreatedAt = DateTime.Now
                };
                await userManager.CreateAsync(admin, "Admin@123");
                await userManager.AddToRoleAsync(admin, Constants.AdminRole);
            }

            // Staff User
            if (!await userManager.Users.AnyAsync(x => x.UserName == "staff"))
            {
                var staff = new AppUser
                {
                    UserName = "staff",
                    Email = "staff@university.edu",
                    EmailConfirmed = true,
                    JagId = "J0000002",
                    CreatedAt = DateTime.Now
                };
                await userManager.CreateAsync(staff, "Staff@123");
                await userManager.AddToRoleAsync(staff, Constants.StaffRole);
            }

            // Alumni Users
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
                    await userManager.CreateAsync(alumniUser, "Alumni@123");
                    await userManager.AddToRoleAsync(alumniUser, Constants.AlumniRole);
                }
            }
        }

        public static async Task EnsureSampleDataAsync(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            // Seed Degree Programs
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

            // Seed Employers
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

            // Seed Organization Types
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

            // Seed Alumni Registry
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

            // Note: Alumni seeding commented out due to foreign key constraints
            // Alumni records can be created directly through the application UI after user registration
            // This ensures proper relationships with AppUser accounts
        }
    }
}
