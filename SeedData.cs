using Alumni_Management_System.Models;
using Alumni_Management_System.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Alumni_Management_System
{
    public class SeedData
    {
        // Test logins for every role combination, all sharing TestPassword.
        // The ones holding the Alumni role also get a Registry row and an
        // Alumni profile (see EnsureSampleDataAsync) so profile pages work.
        public const string TestPassword = "Password1";

        public static readonly (string UserName, string Email, string JagId, string FirstName, string LastName, string[] Roles)[] TestAccounts =
        {
            ("test.admin",        "test.admin@university.edu",        "J90000001", "Test", "Admin",        new[] { Constants.AdminRole }),
            ("test.staff",        "test.staff@university.edu",        "J90000002", "Test", "Staff",        new[] { Constants.StaffRole }),
            ("test.alumni",       "test.alumni@university.edu",       "J90000003", "Test", "Alumni",       new[] { Constants.AlumniRole }),
            ("test.staff.alumni", "test.staff.alumni@university.edu", "J90000004", "Test", "StaffAlumni",  new[] { Constants.StaffRole, Constants.AlumniRole }),
            ("test.admin.alumni", "test.admin.alumni@university.edu", "J90000005", "Test", "AdminAlumni",  new[] { Constants.AdminRole, Constants.AlumniRole }),
        };

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
                    CreatedAt = DateTime.Now,
                    IsFirstLogin = false, // not a placeholder account - real username chosen up front
                    TwoFactorEnabled = false // opt-in - user can enable it later in Two-Factor Auth settings
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
                    CreatedAt = DateTime.Now,
                    IsFirstLogin = false, // not a placeholder account - real username chosen up front
                    TwoFactorEnabled = false // opt-in - user can enable it later in Two-Factor Auth settings
                };
                await userManager.CreateAsync(staff, "Staff@123");
                await userManager.AddToRoleAsync(staff, Constants.StaffRole);
            }

            // Test accounts - one per role combination (see TestAccounts).
            foreach (var test in TestAccounts)
            {
                var user = await userManager.Users.FirstOrDefaultAsync(x => x.UserName == test.UserName);
                if (user == null)
                {
                    user = new AppUser
                    {
                        UserName = test.UserName,
                        Email = test.Email,
                        EmailConfirmed = true,
                        JagId = test.JagId,
                        CreatedAt = DateTime.Now,
                        IsFirstLogin = false,
                        MustChangePassword = false,
                        TwoFactorEnabled = false
                    };
                    // "Password1" is deliberately simpler than the password
                    // policy (no symbol) so it's easy to type while testing -
                    // hash it directly instead of CreateAsync(user, password),
                    // which would reject it. Real accounts still get the policy.
                    user.PasswordHash = userManager.PasswordHasher.HashPassword(user, TestPassword);
                    var created = await userManager.CreateAsync(user);
                    if (!created.Succeeded)
                    {
                        throw new InvalidOperationException($"Seeding test account {test.UserName} failed: " +
                            string.Join("; ", created.Errors.Select(e => e.Description)));
                    }
                }

                foreach (var role in test.Roles)
                {
                    if (!await userManager.IsInRoleAsync(user, role))
                    {
                        await userManager.AddToRoleAsync(user, role);
                    }
                }
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
            // Seed Colleges (idempotent per-college: the initial migration already
            // creates a default "School of Computing" college for backfill purposes)
            if (!await context.Colleges.AnyAsync(c => c.CollegeName == "School of Computing"))
            {
                await context.Colleges.AddAsync(new College { CollegeName = "School of Computing", IsInternal = true, IsActive = true });
                await context.SaveChangesAsync();
            }
            if (!await context.Colleges.AnyAsync(c => c.CollegeName == "Mitchell College of Business"))
            {
                await context.Colleges.AddAsync(new College { CollegeName = "Mitchell College of Business", IsInternal = true, IsActive = true });
                await context.SaveChangesAsync();
            }

            var soc = await context.Colleges.FirstAsync(c => c.CollegeName == "School of Computing");
            var mcob = await context.Colleges.FirstAsync(c => c.CollegeName == "Mitchell College of Business");

            // Seed Departments (idempotent per-department)
            var departmentsToSeed = new[]
            {
                new Department { CollegeId = soc.CollegeId, DepartmentName = "Computer Science", IsActive = true },
                new Department { CollegeId = soc.CollegeId, DepartmentName = "Information Technology", IsActive = true },
                new Department { CollegeId = soc.CollegeId, DepartmentName = "Engineering", IsActive = true },
                new Department { CollegeId = mcob.CollegeId, DepartmentName = "Business", IsActive = true }
            };
            foreach (var dept in departmentsToSeed)
            {
                if (!await context.Departments.AnyAsync(d => d.DepartmentName == dept.DepartmentName && d.CollegeId == dept.CollegeId))
                {
                    await context.Departments.AddAsync(dept);
                    await context.SaveChangesAsync();
                }
            }

            var csDept = await context.Departments.FirstAsync(d => d.DepartmentName == "Computer Science");
            var itDept = await context.Departments.FirstAsync(d => d.DepartmentName == "Information Technology");
            var engDept = await context.Departments.FirstAsync(d => d.DepartmentName == "Engineering");
            var bizDept = await context.Departments.FirstAsync(d => d.DepartmentName == "Business");

            // Seed Degree Programs
            if (!await context.DegreePrograms.AnyAsync())
            {
                var degreePrograms = new[]
                {
                    new DegreeProgram { Institution = "University", DegreeType = "BS", MajorFieldOfStudy = "Computer Science", DepartmentId = csDept.DepartmentId },
                    new DegreeProgram { Institution = "University", DegreeType = "MS", MajorFieldOfStudy = "Computer Science", DepartmentId = csDept.DepartmentId },
                    new DegreeProgram { Institution = "University", DegreeType = "BBA", MajorFieldOfStudy = "Business Administration", DepartmentId = bizDept.DepartmentId },
                    new DegreeProgram { Institution = "University", DegreeType = "MBA", MajorFieldOfStudy = "Business Administration", DepartmentId = bizDept.DepartmentId },
                    new DegreeProgram { Institution = "University", DegreeType = "BE", MajorFieldOfStudy = "Engineering", DepartmentId = engDept.DepartmentId },
                    new DegreeProgram { Institution = "University", DegreeType = "MS", MajorFieldOfStudy = "Data Science", DepartmentId = csDept.DepartmentId },
                    new DegreeProgram { Institution = "University", DegreeType = "BIT", MajorFieldOfStudy = "Information Technology", DepartmentId = itDept.DepartmentId }
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

            // Seed Student Organizations
            if (!await context.StudentOrganizations.AnyAsync())
            {
                var studentOrgs = new[]
                {
                    new StudentOrganization { OrganizationName = "Student Government Association", CollegeId = soc.CollegeId, IsActive = true },
                    new StudentOrganization { OrganizationName = "Computer Science Club", CollegeId = soc.CollegeId, DepartmentId = csDept.DepartmentId, IsActive = true },
                    new StudentOrganization { OrganizationName = "Business Leaders Society", CollegeId = mcob.CollegeId, DepartmentId = bizDept.DepartmentId, IsActive = true },
                    new StudentOrganization { OrganizationName = "Volunteer Corps", CollegeId = soc.CollegeId, IsActive = true },
                    new StudentOrganization { OrganizationName = "Athletics Association", CollegeId = soc.CollegeId, IsActive = true }
                };
                await context.StudentOrganizations.AddRangeAsync(studentOrgs);
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

            // Seed an Alumni profile for each demo Alumni login above. The login
            // and Registry row alone aren't enough - My Profile, Add Employment,
            // etc. all look up the Alumni row by JagId and fail without it.
            // Alumni <-> AppUser is a logical JagId link (no FK), so this is
            // safe; per-JagId check so it also back-fills existing databases.
            var alumniProfiles = new List<(string JagId, string FirstName, string LastName)>
            {
                ("J0012345", "John", "Doe"),
                ("J0012346", "Jane", "Smith"),
                ("J0012347", "Michael", "Johnson"),
                ("J0012348", "Sarah", "Williams"),
                ("J0012349", "David", "Brown")
            };
            alumniProfiles.AddRange(TestAccounts
                .Where(t => t.Roles.Contains(Constants.AlumniRole))
                .Select(t => (t.JagId, t.FirstName, t.LastName)));

            foreach (var (jagId, firstName, lastName) in alumniProfiles)
            {
                var user = await userManager.Users.FirstOrDefaultAsync(u => u.JagId == jagId);
                if (user == null) continue;

                var registry = await context.AlumniRegistries.FirstOrDefaultAsync(r => r.JagId == jagId);
                if (registry == null)
                {
                    registry = new AlumniRegistry { JagId = jagId, FirstName = firstName, LastName = lastName, AccountCreated = true };
                    context.AlumniRegistries.Add(registry);
                    await context.SaveChangesAsync();
                }

                if (await context.Alumni.AnyAsync(a => a.JagId == jagId)) continue;

                // Don't collide with an email another Alumni record already uses.
                var email = user.Email;
                if (string.IsNullOrWhiteSpace(email) || await context.Alumni.AnyAsync(a => a.PermanentEmail == email))
                {
                    email = $"{jagId.ToLower()}@pending.import";
                }

                context.Alumni.Add(new Alumni
                {
                    JagId = jagId,
                    FirstName = registry.FirstName,
                    LastName = registry.LastName,
                    PermanentEmail = email,
                    GraduationYear = 0,
                    IsActive = true,
                    Privacy = true,
                    SolicitationCode = false,
                    LastUpdated = DateTime.Now
                });
                await context.SaveChangesAsync();
            }
        }
    }
}
