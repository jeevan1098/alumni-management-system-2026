//using System.Diagnostics;

using System.Diagnostics;
using System.Linq;
using Alumni_Management_System.Data;
using Alumni_Management_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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

            if (roles.Contains("Admin"))
                return RedirectToAction("Dashboard");

            if (roles.Contains("Staff"))
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
                .ToListAsync();

            var alumniYears = alumniChart
                .Select(x => (int?)x.Year)
                .Where(x => x.HasValue)
                .Select(x => x.Value)
                .ToList();

            var years = alumniYears
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            ViewBag.ChartLabels = years;

            ViewBag.AlumniData = years
                .Select(y => alumniChart.FirstOrDefault(a => a.Year == y)?.Count ?? 0)
                .ToList();

            ViewBag.RegistryData = years.Select(y => 0).ToList();

            var activity = new List<string>();

            var recentAlumni = await _context.Alumni
                .OrderByDescending(a => a.LastUpdated)
                .Take(3)
                .ToListAsync();

            activity.AddRange(recentAlumni
                .Select(a => $"Alumni updated: {a.FirstName} {a.LastName}"));

            var recentRegistry = await _context.AlumniRegistries
                .OrderByDescending(r => r.RegistryId)
                .Take(3)
                .ToListAsync();

            activity.AddRange(recentRegistry
                .Select(r => $"Registry updated: {r.FirstName} {r.LastName}"));

            var recentMessages = await _context.Messages
                .OrderByDescending(m => m.MessageId)
                .Take(3)
                .ToListAsync();

            activity.AddRange(recentMessages
                .Select(m => $"New message sent"));

            var recentJobs = await _context.AlumniEmployments
                .OrderByDescending(e => e.AlumniEmploymentId)
                .Take(3)
                .ToListAsync();

            activity.AddRange(recentJobs
                .Select(e => $"Employment updated"));

            ViewBag.RecentActivity = activity.Take(10).ToList();

            return View();
        }

        [Authorize(Roles = "Alumni")]
        public async Task<IActionResult> AlumniPortal()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Index");

            var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.UserId == user.Id || a.JagId == user.JagId);

            if (alumni != null)
            {

                if (string.IsNullOrEmpty(alumni.UserId) || alumni.UserId != user.Id)
                {
                    alumni.UserId = user.Id;
                    _context.Alumni.Update(alumni);
                    await _context.SaveChangesAsync();
                }

                int total = 18, done = 0;

                if (!string.IsNullOrEmpty(alumni.FirstName)) done++;
                if (!string.IsNullOrEmpty(alumni.LastName)) done++;
                if (!string.IsNullOrEmpty(alumni.PermanentEmail)) done++;
                if (!string.IsNullOrEmpty(alumni.Phone)) done++;
                if (!string.IsNullOrEmpty(alumni.Gender)) done++;
                if (alumni.AgeAtGraduation > 0) done++;
                if (!string.IsNullOrEmpty(alumni.Address)) done++;
                if (!string.IsNullOrEmpty(alumni.City)) done++;
                if (!string.IsNullOrEmpty(alumni.State)) done++;
                if (!string.IsNullOrEmpty(alumni.Country)) done++;
                if (alumni.GraduationYear > 0) done++;
                if (!string.IsNullOrEmpty(alumni.StudentEmail)) done++;
                if (!string.IsNullOrEmpty(alumni.PreferredFirstName)) done++;
                if (!string.IsNullOrEmpty(alumni.SocialMediaAccount)) done++;
                if (!string.IsNullOrEmpty(alumni.Prefix)) done++;
                if (alumni.Privacy) done++;
                if (alumni.IsActive) done++;

                ViewData["ProfileCompletion"] = (int)((double)done / total * 100);
            }

            return View();   // Views/Home/AlumniPortal.cshtml
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