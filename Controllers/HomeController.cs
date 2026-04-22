using System.Diagnostics;
using System.Linq;
using Alumni_Management_System.Data;
using Alumni_Management_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Alumni_Management_System.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public HomeController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            if (!User.Identity.IsAuthenticated)
                return View("PublicHome");

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return View("PublicHome");

            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains("Admin") || roles.Contains("Staff"))
                return RedirectToAction("Dashboard");

            if (roles.Contains("Alumni"))
                return RedirectToAction("AlumniPortal");

            return View("PublicHome");
        }

        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            var roles = user != null ? await _userManager.GetRolesAsync(user) : new List<string>();
            ViewData["UserRole"] = roles.FirstOrDefault();

            ViewData["TotalAlumni"] = await _context.Alumni.CountAsync();
            ViewData["TotalRegistry"] = await _context.AlumniRegistries.CountAsync();
            ViewData["TotalUsers"] = await _context.Users.CountAsync();
            ViewData["TotalMessages"] = await _context.Messages.CountAsync();
            ViewData["TotalEmployments"] = await _context.AlumniEmployments.CountAsync();
            ViewData["PendingApprovals"] = await _context.Alumni.CountAsync(a => !a.IsActive);

            var alumniChart = await _context.Alumni
                .Where(a => a.GraduationYear > 0)
                .GroupBy(a => a.GraduationYear)
                .Select(g => new { Year = g.Key, Count = g.Count() })
                .OrderBy(g => g.Year)
                .ToListAsync();

            ViewBag.ChartLabels = alumniChart.Select(x => x.Year).ToArray();
            ViewBag.AlumniData = alumniChart.Select(x => x.Count).ToArray();

            var activity = new List<string>();

            var recentAlumni = await _context.Alumni
                .OrderByDescending(a => a.LastUpdated)
                .Take(3)
                .ToListAsync();
            activity.AddRange(recentAlumni.Select(a => $"Alumni updated: {a.FirstName} {a.LastName}"));

            var recentRegistry = await _context.AlumniRegistries
                .OrderByDescending(r => r.RegistryId)
                .Take(3)
                .ToListAsync();
            activity.AddRange(recentRegistry.Select(r => $"Registry updated: {r.FirstName} {r.LastName}"));

            var recentMessages = await _context.Messages
                .OrderByDescending(m => m.MessageId)
                .Take(3)
                .ToListAsync();
            activity.AddRange(recentMessages.Select(m => "New message sent"));

            var recentJobs = await _context.AlumniEmployments
                .OrderByDescending(e => e.AlumniEmploymentId)
                .Take(3)
                .ToListAsync();
            activity.AddRange(recentJobs.Select(e => "Employment updated"));

            ViewBag.RecentActivity = activity.Take(10).ToList();

            return View();
        }

        [Authorize(Roles = "Alumni")]
        public async Task<IActionResult> AlumniPortal()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Index");

            var alumni = await _context.Alumni
                .FirstOrDefaultAsync(a => a.UserId == user.Id || a.JagId == user.JagId);

            if (alumni == null)
            {
                ViewData["ProfileCompletion"] = 0;
                ViewBag.Employments = Enumerable.Empty<AlumniEmployment>();
                ViewBag.Internships = Enumerable.Empty<AlumniInternship>();
                ViewBag.Organizations = Enumerable.Empty<AlumniOrganization>();
                ViewBag.Degrees = Enumerable.Empty<AlumniDegree>();

                return View(null);
            }

            // Link alumni account on first login if needed
            if (string.IsNullOrEmpty(alumni.UserId) || alumni.UserId != user.Id)
            {
                alumni.UserId = user.Id;
                _context.Alumni.Update(alumni);
                await _context.SaveChangesAsync();
            }

            // Load related data for portal
            var employments = await _context.AlumniEmployments
                .Where(e => e.AlumniId == alumni.AlumniId)
                .Include(e => e.Employer)
                .OrderByDescending(e => e.StartDate)
                .ToListAsync();

            var internships = await _context.AlumniInternships
                .Where(i => i.AlumniId == alumni.AlumniId)
                .Include(i => i.Employer)
                .OrderByDescending(i => i.StartDate)
                .ToListAsync();

            var organizations = await _context.AlumniOrganizations
                .Where(o => o.AlumniId == alumni.AlumniId)
                .Include(o => o.OrganizationType)
                .ToListAsync();

            var degrees = await _context.AlumniDegrees
                .Where(d => d.AlumniId == alumni.AlumniId)
                .Include(d => d.Degree)
                .OrderByDescending(d => d.DateConferred)
                .ToListAsync();

            ViewBag.Employments = employments;
            ViewBag.Internships = internships;
            ViewBag.Organizations = organizations;
            ViewBag.Degrees = degrees;

            // Correct profile completion calculation
            int total = 14;
            int done = 0;

            if (!string.IsNullOrWhiteSpace(alumni.FirstName)) done++;
            if (!string.IsNullOrWhiteSpace(alumni.LastName)) done++;
            if (!string.IsNullOrWhiteSpace(alumni.PermanentEmail)) done++;
            if (!string.IsNullOrWhiteSpace(alumni.Phone)) done++;
            if (!string.IsNullOrWhiteSpace(alumni.Gender)) done++;
            if (alumni.AgeAtGraduation > 0) done++;
            if (!string.IsNullOrWhiteSpace(alumni.Address)) done++;
            if (!string.IsNullOrWhiteSpace(alumni.City)) done++;
            if (!string.IsNullOrWhiteSpace(alumni.State)) done++;
            if (!string.IsNullOrWhiteSpace(alumni.Country)) done++;
            if (alumni.GraduationYear > 0) done++;
            if (!string.IsNullOrWhiteSpace(alumni.StudentEmail)) done++;
            if (!string.IsNullOrWhiteSpace(alumni.SocialMediaAccount)) done++;
            if (!string.IsNullOrWhiteSpace(alumni.Prefix)) done++;

            ViewData["ProfileCompletion"] = (int)Math.Round((double)done / total * 100);

            return View(alumni);
        }
        public IActionResult Privacy() => View();

        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
