using System.Diagnostics;
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
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var roles = await _userManager.GetRolesAsync(currentUser);

                if (roles.Contains(Constants.AlumniRole))
                {
                    var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.UserId == currentUser.Id);
                    if (alumni != null)
                    {
                        // Calculate profile completion
                        int totalFields = 18;
                        int completedFields = 0;

                        if (!string.IsNullOrEmpty(alumni.FirstName)) completedFields++;
                        if (!string.IsNullOrEmpty(alumni.LastName)) completedFields++;
                        if (!string.IsNullOrEmpty(alumni.PermanentEmail)) completedFields++;
                        if (!string.IsNullOrEmpty(alumni.Phone)) completedFields++;
                        if (!string.IsNullOrEmpty(alumni.Gender)) completedFields++;
                        if (alumni.DateOfBirth.HasValue) completedFields++;
                        if (!string.IsNullOrEmpty(alumni.Address)) completedFields++;
                        if (!string.IsNullOrEmpty(alumni.City)) completedFields++;
                        if (!string.IsNullOrEmpty(alumni.State)) completedFields++;
                        if (!string.IsNullOrEmpty(alumni.Postcode)) completedFields++;
                        if (!string.IsNullOrEmpty(alumni.Country)) completedFields++;
                        if (alumni.GraduationYear > 0) completedFields++;
                        if (!string.IsNullOrEmpty(alumni.PreferredFirstName)) completedFields++;
                        if (!string.IsNullOrEmpty(alumni.StudentEmail)) completedFields++;
                        if (!string.IsNullOrEmpty(alumni.SocialMediaAccount)) completedFields++;
                        if (!string.IsNullOrEmpty(alumni.Prefix)) completedFields++;
                        if (alumni.Privacy) completedFields++;
                        if (alumni.IsActive) completedFields++;

                        int completionPercentage = (int)((double)completedFields / totalFields * 100);

                        ViewData["ProfileCompletion"] = completionPercentage;
                        ViewData["CompletedFields"] = completedFields;
                        ViewData["TotalFields"] = totalFields;
                    }
                }
            }

            return View();
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
