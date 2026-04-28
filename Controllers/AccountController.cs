using Alumni_Management_System.Data;
using Alumni_Management_System.Models;
using Alumni_Management_System.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Alumni_Management_System.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            ApplicationDbContext context,
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            ILogger<AccountController> logger)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string newPassword, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword != confirmPassword)
            {
                TempData["ErrorMessage"] = "Passwords do not match or are empty.";
                return View();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login", new { area = "Identity" });

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (result.Succeeded)
            {
                user.IsFirstLogin = false;
                await _userManager.UpdateAsync(user);
                await _signInManager.RefreshSignInAsync(user);
                TempData["SuccessMessage"] = "Password changed successfully!";

                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains(Constants.AlumniRole))
                    return RedirectToAction("AlumniPortal", "Home");
                return RedirectToAction("Index", "Home");
            }

            TempData["ErrorMessage"] = "Failed: " + string.Join(", ", result.Errors.Select(e => e.Description));
            return View();
        }

        [HttpGet]
        public IActionResult VerifyJagId()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyJagId(VerifyJagIdViewModel model)
        {
            // Normalise: convert leading lowercase 'j' to uppercase 'J'
            if (!string.IsNullOrEmpty(model.JagId) && model.JagId[0] == 'j')
                model.JagId = "J" + model.JagId.Substring(1);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var registryEntry = await _context.AlumniRegistries
                  .FirstOrDefaultAsync(r => r.JagId == model.JagId && r.LastName.Trim().ToLower() == model.LastName.Trim().ToLower());

            if (registryEntry == null)
            {
                ModelState.AddModelError("JagId", "JAG ID not found. Please contact the administrator for assistance.");
                return View(model);
            }


            if (registryEntry.AccountCreated)
            {
                ModelState.AddModelError("JagId", "Account already exists associated with this JAG ID. Please contact the administrator for help.");
                return View(model);
            }

            TempData["JagId"] = model.JagId;
            TempData["FirstName"] = registryEntry.FirstName;
            TempData["LastName"] = registryEntry.LastName;

            return RedirectToAction(nameof(RegisterAlumni));
        }

        [HttpGet]
        public IActionResult RegisterAlumni()
        {

            if (TempData["JagId"] == null)
            {
                return RedirectToAction(nameof(VerifyJagId));
            }

            var model = new AlumniRegisterViewModel
            {
                JagId = TempData["JagId"]?.ToString(),
                FirstName = TempData["FirstName"]?.ToString(),
                LastName = TempData["LastName"]?.ToString()
            };

            TempData.Keep();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterAlumni(AlumniRegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var registryEntry = await _context.AlumniRegistries
                .FirstOrDefaultAsync(r => r.JagId == model.JagId);

            if (registryEntry == null || registryEntry.AccountCreated)
            {
                ModelState.AddModelError("", "Invalid registration attempt or account already exists.");
                return RedirectToAction(nameof(VerifyJagId));
            }

            var existingAlumni = await _context.Alumni
                .FirstOrDefaultAsync(a => a.JagId == model.JagId);

            if (existingAlumni == null)
            {
                ModelState.AddModelError("", "Record not found. Please contact admin support.");
                return View(model);
            }

            var existingUser = await _userManager.FindByNameAsync(model.Username);
            if (existingUser != null)
            {
                ModelState.AddModelError("Username", "Username already taken. Please choose a different username.");
                return View(model);
            }

            var optionalEmail = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();

            var user = new AppUser
            {
                UserName = model.Username,
                Email = optionalEmail,
                JagId = model.JagId,
                PhoneNumber = null,
                EmailConfirmed = true,
                CreatedAt = DateTime.Now,
                IsFirstLogin = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("Identity User created for existing Alumni record.");

                await _userManager.AddToRoleAsync(user, Constants.AlumniRole);

                existingAlumni.UserId = user.Id;
                existingAlumni.LastUpdated = DateTime.Now;
                _context.Alumni.Update(existingAlumni);

                registryEntry.AccountCreated = true;
                _context.AlumniRegistries.Update(registryEntry);

                if (!string.IsNullOrEmpty(optionalEmail))
                {
                    existingAlumni.PermanentEmail = optionalEmail;
                }

                await _context.SaveChangesAsync();

                await _signInManager.SignInAsync(user, isPersistent: false);

                _logger.LogInformation($"User registered successfully and mapped to Alumni ID: {existingAlumni.AlumniId}");

                TempData["SuccessMessage"] = "Registration successful! Welcome to the Alumni Management System.";

                return RedirectToAction("AlumniPortal", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }
    }
}
