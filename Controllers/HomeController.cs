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
            
            // User is authenticated but not found in database
            if (user == null)
                return View("PublicHome");

            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains("Admin") || roles.Contains("Staff"))
                return RedirectToAction("Dashboard");

            if (roles.Contains("Alumni"))
                return RedirectToAction("AlumniPortal");

            return View("PublicHome");
        }

        // ================= ADMIN/STAFF DASHBOARD =================
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Dashboard()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);
            var allowedCollegeIds = await Services.AccessScopeService.GetAllowedCollegeIdsAsync(_context, currentUser, roles);

            IQueryable<Alumni> scopedAlumni = _context.Alumni;
            if (allowedCollegeIds != null)
            {
                scopedAlumni = scopedAlumni.Where(a => a.CollegeId != null && allowedCollegeIds.Contains(a.CollegeId.Value));

                var scopedCollegeNames = await _context.Colleges
                    .Where(c => allowedCollegeIds.Contains(c.CollegeId))
                    .Select(c => c.CollegeName)
                    .ToListAsync();
                ViewData["ScopeLabel"] = string.Join(", ", scopedCollegeNames);
            }

            // ===== COUNTS =====
            ViewData["TotalAlumni"] = await scopedAlumni.CountAsync();
            ViewData["TotalRegistry"] = await _context.AlumniRegistries.CountAsync();
            ViewData["TotalUsers"] = await _context.Users.CountAsync();
            ViewData["TotalMessages"] = await _context.Messages.CountAsync();
            ViewData["TotalEmployments"] = await _context.AlumniEmployments.CountAsync();
            ViewData["TotalColleges"] = await _context.Colleges.CountAsync();

            // =========================================================
            // 📊 CHART DATA - Alumni by graduation year, and by college
            // =========================================================

            var alumniChart = await scopedAlumni
                .Where(a => a.GraduationYear > 0)
                .GroupBy(a => a.GraduationYear)
                .Select(g => new { Year = g.Key, Count = g.Count() })
                .OrderBy(g => g.Year)
                .ToListAsync();

            ViewBag.ChartLabels = alumniChart.Select(g => g.Year).ToList();
            ViewBag.AlumniData = alumniChart.Select(g => g.Count).ToList();

            var alumniByCollege = await scopedAlumni
                .Where(a => a.CollegeId != null)
                .GroupBy(a => a.College.CollegeName)
                .Select(g => new { College = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .ToListAsync();

            ViewBag.CollegeLabels = alumniByCollege.Select(g => g.College).ToList();
            ViewBag.CollegeData = alumniByCollege.Select(g => g.Count).ToList();

            // =========================================================
            // 🔴 RECENT ACTIVITY - pulled from the audit trail (real
            // timestamps + who made the change), not hand-rolled per-entity
            // guesses.
            // =========================================================

            ViewBag.RecentActivity = await Services.RecentActivityService.GetRecentAsync(_context, take: 8);

            return View();
        }


        // ================= ALUMNI PORTAL =================
        [Authorize(Roles = "Alumni")]
        public async Task<IActionResult> AlumniPortal()
        {
            var user = await _userManager.GetUserAsync(User);
            
            // User is authenticated but not found in database
            if (user == null)
                return RedirectToAction("Index");

            var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == user.JagId);

            if (alumni != null)
            {
                int total = 18, done = 0;

                if (!string.IsNullOrEmpty(alumni.FirstName)) done++;
                if (!string.IsNullOrEmpty(alumni.LastName)) done++;
                if (!string.IsNullOrEmpty(alumni.PermanentEmail)) done++;
                if (!string.IsNullOrEmpty(alumni.Phone)) done++;
                if (!string.IsNullOrEmpty(alumni.Gender)) done++;
                if (alumni.DateOfBirth != null) done++;
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