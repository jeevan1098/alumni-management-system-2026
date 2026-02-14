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
            if (!await userManager.Users.AnyAsync(x => x.UserName == "admin@admin.com"))
            {
                var admin = new AppUser
                {
                    UserName = "admin@admin.com",
                    Email = "admin@admin.com",
                    EmailConfirmed = true,
                    JagId = "J0000001",
                    CreatedAt = DateTime.Now
                };
                await userManager.CreateAsync(admin, "Password1!");
                await userManager.AddToRoleAsync(admin, Constants.AdminRole);
            }

            // Staff User
            if (!await userManager.Users.AnyAsync(x => x.UserName == "staff@university.edu"))
            {
                var staff = new AppUser
                {
                    UserName = "staff@university.edu",
                    Email = "staff@university.edu",
                    EmailConfirmed = true,
                    JagId = "J0000002",
                    CreatedAt = DateTime.Now
                };
                await userManager.CreateAsync(staff, "Password1!");
                await userManager.AddToRoleAsync(staff, Constants.StaffRole);
            }

            // Alumni Users
            var alumniUsers = new[]
            {
                new { Email = "john.doe@email.com", JagId = "J0012345", FirstName = "John", LastName = "Doe" },
                new { Email = "jane.smith@email.com", JagId = "J0012346", FirstName = "Jane", LastName = "Smith" },
                new { Email = "michael.johnson@email.com", JagId = "J0012347", FirstName = "Michael", LastName = "Johnson" },
                new { Email = "sarah.williams@email.com", JagId = "J0012348", FirstName = "Sarah", LastName = "Williams" },
                new { Email = "david.brown@email.com", JagId = "J0012349", FirstName = "David", LastName = "Brown" }
            };

            foreach (var alumniData in alumniUsers)
            {
                if (!await userManager.Users.AnyAsync(x => x.UserName == alumniData.Email))
                {
                    var alumniUser = new AppUser
                    {
                        UserName = alumniData.Email,
                        Email = alumniData.Email,
                        EmailConfirmed = true,
                        JagId = alumniData.JagId,
                        CreatedAt = DateTime.Now
                    };
                    await userManager.CreateAsync(alumniUser, "Password1!");
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
                    new AlumniRegistry { JagId = "J0012345", FirstName = "John", LastName = "Doe", GraduationYear = 2020, DegreeProgram = "Computer Science", EmailOnRecord = "john.doe@email.com", AccountCreated = true },
                    new AlumniRegistry { JagId = "J0012346", FirstName = "Jane", LastName = "Smith", GraduationYear = 2021, DegreeProgram = "Business Administration", EmailOnRecord = "jane.smith@email.com", AccountCreated = true },
                    new AlumniRegistry { JagId = "J0012347", FirstName = "Michael", LastName = "Johnson", GraduationYear = 2019, DegreeProgram = "Engineering", EmailOnRecord = "michael.johnson@email.com", AccountCreated = true },
                    new AlumniRegistry { JagId = "J0012348", FirstName = "Sarah", LastName = "Williams", GraduationYear = 2022, DegreeProgram = "Data Science", EmailOnRecord = "sarah.williams@email.com", AccountCreated = true },
                    new AlumniRegistry { JagId = "J0012349", FirstName = "David", LastName = "Brown", GraduationYear = 2020, DegreeProgram = "Information Technology", EmailOnRecord = "david.brown@email.com", AccountCreated = true },
                    new AlumniRegistry { JagId = "J0012350", FirstName = "Emily", LastName = "Davis", GraduationYear = 2018, DegreeProgram = "Computer Science", EmailOnRecord = "emily.davis@email.com", AccountCreated = false },
                    new AlumniRegistry { JagId = "J0012351", FirstName = "Robert", LastName = "Miller", GraduationYear = 2017, DegreeProgram = "Business Administration", EmailOnRecord = "robert.miller@email.com", AccountCreated = false }
                };
                await context.AlumniRegistries.AddRangeAsync(registries);
                await context.SaveChangesAsync();
            }

            // Seed Alumni (linked to users)
            if (!await context.Alumni.AnyAsync())
            {
                var johnUser = await userManager.FindByEmailAsync("john.doe@email.com");
                var janeUser = await userManager.FindByEmailAsync("jane.smith@email.com");
                var michaelUser = await userManager.FindByEmailAsync("michael.johnson@email.com");
                var sarahUser = await userManager.FindByEmailAsync("sarah.williams@email.com");
                var davidUser = await userManager.FindByEmailAsync("david.brown@email.com");

                var alumniRecords = new List<Alumni>();

                if (johnUser != null)
                {
                    alumniRecords.Add(new Alumni
                    {
                        UserId = johnUser.Id,
                        JagId = "J0012345",
                        FirstName = "John",
                        LastName = "Doe",
                        PermanentEmail = "john.doe@email.com",
                        StudentEmail = "john.doe@students.university.edu",
                        Phone = "555-0101",
                        Address = "123 Main St",
                        City = "Springfield",
                        State = "IL",
                        Postcode = "62701",
                        Country = "USA",
                        GraduationYear = 2020,
                        DateOfBirth = new DateOnly(1998, 5, 15),
                        Gender = "Male",
                        Privacy = true,
                        IsActive = true,
                        LastUpdated = DateTime.Now
                    });
                }

                // Add more alumni records as needed...
                await context.Alumni.AddRangeAsync(alumniRecords);
                await context.SaveChangesAsync();
            }
        }
    }
}
