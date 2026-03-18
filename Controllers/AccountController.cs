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

        // GET: Account/VerifyJagId
        [HttpGet]
        public IActionResult VerifyJagId()
        {
            return View();
        }

        // POST: Account/VerifyJagId
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyJagId(VerifyJagIdViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if JAG ID exists in Alumni Registry
            var registryEntry = await _context.AlumniRegistries
                .FirstOrDefaultAsync(r => r.JagId == model.JagId);

            if (registryEntry == null)
            {
                ModelState.AddModelError("JagId", "JAG ID not found in the Alumni Registry. Please contact the administrator for assistance.");
                return View(model);
            }

            // Check if account already exists for this JAG ID
            if (registryEntry.AccountCreated)
            {
                ModelState.AddModelError("JagId", "Account already exists associated with this JAG ID. Please contact administrator for help.");
                return View(model);
            }

            // JAG ID is valid and no account exists - redirect to registration
            TempData["JagId"] = model.JagId;
            TempData["FirstName"] = registryEntry.FirstName;
            TempData["LastName"] = registryEntry.LastName;

            return RedirectToAction(nameof(RegisterAlumni));
        }

        // GET: Account/RegisterAlumni
        [HttpGet]
        public IActionResult RegisterAlumni()
        {
            // Check if we have JAG ID from verification step
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

            // Keep data in TempData for POST
            TempData.Keep();

            return View(model);
        }

        // POST: Account/RegisterAlumni
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> RegisterAlumni(AlumniRegisterViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(model);
        //    }

        //    // Double-check JAG ID exists and account not created
        //    var registryEntry = await _context.AlumniRegistries
        //        .FirstOrDefaultAsync(r => r.JagId == model.JagId);

        //    if (registryEntry == null || registryEntry.AccountCreated)
        //    {
        //        ModelState.AddModelError("", "Invalid registration attempt. Please start the registration process again.");
        //        return RedirectToAction(nameof(VerifyJagId));
        //    }

        //    // Create the user account
        //    var user = new AppUser
        //    {
        //        UserName = model.Email,
        //        Email = model.Email,
        //        JagId = model.JagId,
        //        PhoneNumber = model.PhoneNumber,
        //        EmailConfirmed = true,
        //        CreatedAt = DateTime.Now,
        //        IsFirstLogin = true
        //    };

        //    var result = await _userManager.CreateAsync(user, model.Password);

        //    if (result.Succeeded)
        //    {
        //        _logger.LogInformation("User created a new account with password.");

        //        // Assign Alumni role
        //        await _userManager.AddToRoleAsync(user, Constants.AlumniRole);

        //        // Create Alumni profile
        //        var alumni = new Alumni
        //        {
        //            UserId = user.Id,
        //            JagId = model.JagId,
        //            FirstName = model.FirstName,
        //            LastName = model.LastName,
        //            PermanentEmail = model.Email,
        //            GraduationYear = model.GraduationYear ?? DateTime.Now.Year,
        //            IsActive = true,
        //            Privacy = true,
        //            LastUpdated = DateTime.Now
        //        };

        //        _context.Alumni.Add(alumni);

        //        // Update Alumni Registry - mark account as created
        //        registryEntry.AccountCreated = true;
        //        _context.AlumniRegistries.Update(registryEntry);

        //        await _context.SaveChangesAsync();

        //        // Sign in the user
        //        await _signInManager.SignInAsync(user, isPersistent: false);

        //        _logger.LogInformation("User registered successfully and signed in.");

        //        TempData["InfoMessage"] = "Registration successful! Please complete your profile to get started.";
        //        // Redirect to profile edit page for first-time users
        //        return RedirectToAction("Edit", "Alumni", new { id = alumni.AlumniId });
        //    }

        //    // If we got this far, something failed, redisplay form
        //    foreach (var error in result.Errors)
        //    {
        //        ModelState.AddModelError(string.Empty, error.Description);
        //    }

        //    return View(model);
        //}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterAlumni(AlumniRegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 1. Verify JAG ID exists in Registry and hasn't been claimed yet
            var registryEntry = await _context.AlumniRegistries
                .FirstOrDefaultAsync(r => r.JagId == model.JagId);

            if (registryEntry == null || registryEntry.AccountCreated)
            {
                ModelState.AddModelError("", "Invalid registration attempt or account already exists.");
                return RedirectToAction(nameof(VerifyJagId));
            }

            // 2. Find the bulk-imported Alumni record (UserId is currently null)
            var existingAlumni = await _context.Alumni
                .FirstOrDefaultAsync(a => a.JagId == model.JagId);

            if (existingAlumni == null)
            {
                ModelState.AddModelError("", "Your record was not found in the imported Alumni list. Please contact support.");
                return View(model);
            }

            // 3. Check if username is already taken
            var existingUser = await _userManager.FindByNameAsync(model.Username);
            if (existingUser != null)
            {
                ModelState.AddModelError("Username", "Username already taken. Please choose a different username.");
                return View(model);
            }

            // 4. Create the Identity User account
            var user = new AppUser
            {
                UserName = model.Username,  // Use custom username instead of email
                Email = model.Email,
                JagId = model.JagId,
                PhoneNumber = model.PhoneNumber,
                EmailConfirmed = true,
                CreatedAt = DateTime.Now,
                IsFirstLogin = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("Identity User created for existing Alumni record.");

                // 4. Assign Alumni Role
                await _userManager.AddToRoleAsync(user, Constants.AlumniRole);

                // 5. Verify Alumni record exists (already checked above, but double-check)
                // No need to map UserId anymore - relationship is via JagId
                // Just update the last updated timestamp
                existingAlumni.LastUpdated = DateTime.Now;
                _context.Alumni.Update(existingAlumni);

                // 6. Update Registry status
                registryEntry.AccountCreated = true;
                _context.AlumniRegistries.Update(registryEntry);

                await _context.SaveChangesAsync();

                // 7. Sign in and Redirect
                await _signInManager.SignInAsync(user, isPersistent: false);

                _logger.LogInformation($"User registered successfully and mapped to Alumni ID: {existingAlumni.AlumniId}");

                TempData["SuccessMessage"] = "Registration successful! Welcome to the Alumni Management System.";
                // Redirect to Alumni Portal
                return RedirectToAction("AlumniPortal", "Home");
            }

            // Handle Identity errors
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }
    }
}

