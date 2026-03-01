//using System.Diagnostics;
//using Alumni_Management_System.Data;
//using Alumni_Management_System.Models;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//namespace Alumni_Management_System.Controllers
//{
//    public class HomeController : Controller
//    {
//        private readonly ILogger<HomeController> _logger;
//        private readonly ApplicationDbContext _context;
//        private readonly UserManager<AppUser> _userManager;

//        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<AppUser> userManager)
//        {
//            _logger = logger;
//            _context = context;
//            _userManager = userManager;
//        }

//        [AllowAnonymous]
//        public async Task<IActionResult> Index()
//        {
//            if (User.Identity?.IsAuthenticated == true)
//            {
//                var currentUser = await _userManager.GetUserAsync(User);
//                var roles = await _userManager.GetRolesAsync(currentUser);

//                if (roles.Contains(Constants.AlumniRole))
//                {
//                    var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.UserId == currentUser.Id);
//                    if (alumni != null)
//                    {
//                        // Calculate profile completion
//                        int totalFields = 18;
//                        int completedFields = 0;

//                        if (!string.IsNullOrEmpty(alumni.FirstName)) completedFields++;
//                        if (!string.IsNullOrEmpty(alumni.LastName)) completedFields++;
//                        if (!string.IsNullOrEmpty(alumni.PermanentEmail)) completedFields++;
//                        if (!string.IsNullOrEmpty(alumni.Phone)) completedFields++;
//                        if (!string.IsNullOrEmpty(alumni.Gender)) completedFields++;
//                        if (alumni.DateOfBirth.HasValue) completedFields++;
//                        if (!string.IsNullOrEmpty(alumni.Address)) completedFields++;
//                        if (!string.IsNullOrEmpty(alumni.City)) completedFields++;
//                        if (!string.IsNullOrEmpty(alumni.State)) completedFields++;
//                        if (!string.IsNullOrEmpty(alumni.Postcode)) completedFields++;
//                        if (!string.IsNullOrEmpty(alumni.Country)) completedFields++;
//                        if (alumni.GraduationYear > 0) completedFields++;
//                        if (!string.IsNullOrEmpty(alumni.PreferredFirstName)) completedFields++;
//                        if (!string.IsNullOrEmpty(alumni.StudentEmail)) completedFields++;
//                        if (!string.IsNullOrEmpty(alumni.SocialMediaAccount)) completedFields++;
//                        if (!string.IsNullOrEmpty(alumni.Prefix)) completedFields++;
//                        if (alumni.Privacy) completedFields++;
//                        if (alumni.IsActive) completedFields++;

//                        int completionPercentage = (int)((double)completedFields / totalFields * 100);

//                        ViewData["ProfileCompletion"] = completionPercentage;
//                        ViewData["CompletedFields"] = completedFields;
//                        ViewData["TotalFields"] = totalFields;
//                    }
//                }
//            }

//            return View();
//        }

//        [AllowAnonymous]
//        public IActionResult Privacy()
//        {
//            return View();
//        }

//        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
//        public IActionResult Error()
//        {
//            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
//        }
//    }
//}
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

        // ================= LOGIN LANDING =================
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            // NOT LOGGED IN → SHOW LOGIN PAGE
            if (!User.Identity.IsAuthenticated)
                return View("PublicHome");

            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains("Admin"))
                return RedirectToAction("Dashboard");

            if (roles.Contains("Alumni"))
                return RedirectToAction("AlumniPortal");

            return View("PublicHome");
        }

        // ================= ADMIN DASHBOARD =================
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Dashboard()
        {
            // ===== COUNTS =====
            ViewData["TotalAlumni"] = await _context.Alumni.CountAsync();
            ViewData["TotalRegistry"] = await _context.AlumniRegistries.CountAsync();
            ViewData["TotalUsers"] = await _context.Users.CountAsync();
            ViewData["TotalMessages"] = await _context.Messages.CountAsync();
            ViewData["TotalEmployments"] = await _context.AlumniEmployments.CountAsync();
            ViewData["PendingApprovals"] = await _context.Alumni.CountAsync(a => !a.IsActive);

            // =========================================================
            // 📊 REAL CHART DATA (Alumni + Registry + Users by Year)
            // =========================================================

            var alumniChart = await _context.Alumni
                .Where(a => a.GraduationYear > 0)
                .GroupBy(a => a.GraduationYear)
                .Select(g => new { Year = g.Key, Count = g.Count() })
                .ToListAsync();

            var registryChart = await _context.AlumniRegistries
                .Where(r => r.GraduationYear > 0)
                .GroupBy(r => r.GraduationYear)
                .Select(g => new { Year = g.Key, Count = g.Count() })
                .ToListAsync();

            // Convert to List BEFORE Union
            // Convert to NON-NULLABLE int list
            var alumniYears = alumniChart
                .Select(x => (int?)x.Year)
                .Where(x => x.HasValue)
                .Select(x => x.Value)
                .ToList();

            var registryYears = registryChart
                .Select(x => (int?)x.Year)
                .Where(x => x.HasValue)
                .Select(x => x.Value)
                .ToList();

            // Merge safely
            var years = alumniYears
                .Concat(registryYears)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            ViewBag.ChartLabels = years;





            ViewBag.ChartLabels = years;

            ViewBag.AlumniData = years
                .Select(y => alumniChart.FirstOrDefault(a => a.Year == y)?.Count ?? 0)
                .ToList();

            ViewBag.RegistryData = years
                .Select(y => registryChart.FirstOrDefault(r => r.Year == y)?.Count ?? 0)
                .ToList();

            // =========================================================
            // 🔴 REAL LIVE ACTIVITY FEED
            // =========================================================

            var activity = new List<string>();

            // Recent Alumni
            var recentAlumni = await _context.Alumni
                .OrderByDescending(a => a.LastUpdated)
                .Take(3)
                .ToListAsync();

            activity.AddRange(recentAlumni
                .Select(a => $"Alumni updated: {a.FirstName} {a.LastName}"));

            // Recent Registry
            var recentRegistry = await _context.AlumniRegistries
                .OrderByDescending(r => r.RegistryId)
                .Take(3)
                .ToListAsync();

            activity.AddRange(recentRegistry
                .Select(r => $"Registry updated: {r.FirstName} {r.LastName}"));

            // Recent Messages
            var recentMessages = await _context.Messages
                .OrderByDescending(m => m.MessageId)
                .Take(3)
                .ToListAsync();

            activity.AddRange(recentMessages
                .Select(m => $"New message sent"));

            // Recent Employments
            var recentJobs = await _context.AlumniEmployments
                .OrderByDescending(e => e.AlumniEmploymentId)
                .Take(3)
                .ToListAsync();

            activity.AddRange(recentJobs
                .Select(e => $"Employment updated"));

            ViewBag.RecentActivity = activity.Take(10).ToList();

            return View();
        }


        // ================= ALUMNI PORTAL =================
        [Authorize(Roles = "Alumni")]
        public async Task<IActionResult> AlumniPortal()
        {
            var user = await _userManager.GetUserAsync(User);
            var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.UserId == user.Id);

            if (alumni != null)
            {
                int total = 18, done = 0;

                if (!string.IsNullOrEmpty(alumni.FirstName)) done++;
                if (!string.IsNullOrEmpty(alumni.LastName)) done++;
                if (!string.IsNullOrEmpty(alumni.PermanentEmail)) done++;
                if (!string.IsNullOrEmpty(alumni.Phone)) done++;
                if (!string.IsNullOrEmpty(alumni.Gender)) done++;
                if (alumni.DateOfBirth.HasValue) done++;
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