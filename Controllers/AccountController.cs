using Alumni_Management_System.Data;
using Alumni_Management_System.Models;
using Alumni_Management_System.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
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
        private readonly IEmailSender _emailSender;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            ApplicationDbContext context,
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IEmailSender emailSender,
            ILogger<AccountController> logger)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
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

            // Match on JAG ID AND last name together (case-insensitive) so a
            // leaked/guessed JAG ID alone isn't enough to start claiming
            // someone else's registry entry.
            if (registryEntry == null || !string.Equals(registryEntry.LastName?.Trim(), model.LastName?.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", "We couldn't find a matching Alumni Registry entry for that JAG ID and last name. Please contact the administrator for assistance.");
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
                ModelState.AddModelError("", "Your record was not found in the imported Alumni list. Please contact admin support.");
                return View(model);
            }

            // 3. Check if username is already taken
            var existingUser = await _userManager.FindByNameAsync(model.Username);
            if (existingUser != null)
            {
                ModelState.AddModelError("Username", "Username already taken. Please choose a different username.");
                return View(model);
            }

            // Two accounts sharing an email crashes any code path that does
            // an email lookup expecting a single match (e.g. Forgot
            // Password) - block it here rather than let it happen silently.
            var normalizedEmail = _userManager.NormalizeEmail(model.Email);
            if (await _userManager.Users.AnyAsync(u => u.NormalizedEmail == normalizedEmail))
            {
                ModelState.AddModelError("Email", "This email is already used by another account.");
                return View(model);
            }

            // A JAG ID maps to exactly one account. If this JAG ID already
            // has one (e.g. they were set up as Staff/Admin first), don't
            // create a second - they should sign in and have an admin add
            // the Alumni role to their existing account instead.
            if (await _userManager.Users.AnyAsync(u => u.JagId == model.JagId))
            {
                ModelState.AddModelError("", "An account already exists for this JAG ID. Please sign in instead, or contact an administrator if you can't access it.");
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

        // GET: Account/CompleteSetup
        // Forced first-login step for Admin/Staff accounts an admin created
        // with a placeholder username + temp password (see UsersController).
        // The controller carries [AllowAnonymous], so authentication is
        // checked manually here rather than relying on [Authorize].
        [HttpGet]
        public async Task<IActionResult> CompleteSetup()
        {
            var user = await GetSetupUserOrNullAsync();
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteSetup(CompleteSetupViewModel model)
        {
            var user = await GetSetupUserOrNullAsync();
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existingUser = await _userManager.FindByNameAsync(model.NewUsername);
            if (existingUser != null && existingUser.Id != user.Id)
            {
                ModelState.AddModelError(nameof(model.NewUsername), "Username already taken. Please choose a different username.");
                return View(model);
            }

            var passwordResult = await _userManager.ChangePasswordAsync(user, model.TempPassword, model.NewPassword);
            if (!passwordResult.Succeeded)
            {
                foreach (var error in passwordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            var usernameResult = await _userManager.SetUserNameAsync(user, model.NewUsername);
            if (!usernameResult.Succeeded)
            {
                foreach (var error in usernameResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            user.IsFirstLogin = false;
            // 2FA stays off/opt-in here - the user can turn it on themselves
            // in Two-Factor Auth settings whenever they want (see
            // TwoFactorSettingsController).
            await _userManager.UpdateAsync(user);

            // Security stamp changed (password + username) - refresh the
            // auth cookie so the current session isn't kicked out.
            await _signInManager.RefreshSignInAsync(user);

            TempData["SuccessMessage"] = "Account setup complete. Welcome!";
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/ForgotPassword
        // Self-service: a valid email/username on file gets a new temp
        // password emailed immediately - no admin has to act for the
        // request to be fulfilled. An admin can still reset someone's
        // password manually from Manage Users any time they want to
        // (UsersController.ResetPassword) - that stays as a separate,
        // optional path, not a required step in this one.
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        // POST: Account/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var byUsername = await _userManager.FindByNameAsync(model.UsernameOrEmail);

            // Email isn't guaranteed unique (Identity's RequireUniqueEmail
            // only blocks it going forward - accounts created before that was
            // enabled can still collide), so look up ALL matches rather than
            // UserManager.FindByEmailAsync, which throws
            // "Sequence contains more than one element" the moment two
            // accounts share an email.
            var normalizedEmail = _userManager.NormalizeEmail(model.UsernameOrEmail);
            var byEmail = await _userManager.Users
                .Where(u => u.NormalizedEmail == normalizedEmail)
                .ToListAsync();

            var matches = byUsername != null
                ? new List<AppUser> { byUsername }
                : byEmail;

            // Reset and email immediately - never reveal to the requester
            // whether the account exists, so the confirmation page is the
            // same either way.
            foreach (var match in matches)
            {
                if (string.IsNullOrWhiteSpace(match.Email))
                {
                    continue;
                }

                var tempPassword = Services.PasswordGenerator.GenerateTempPassword();
                var token = await _userManager.GeneratePasswordResetTokenAsync(match);
                var result = await _userManager.ResetPasswordAsync(match, token, tempPassword);
                if (!result.Succeeded)
                {
                    _logger.LogWarning("Self-service password reset failed for '{UserName}': {Errors}", match.UserName, string.Join(" ", result.Errors.Select(e => e.Description)));
                    continue;
                }

                match.MustChangePassword = true;
                await _userManager.UpdateAsync(match);

                await _emailSender.SendEmailAsync(
                    match.Email,
                    "Your temporary password - Alumni Management System",
                    $"<p>You requested a password reset.</p>" +
                    $"<p><strong>Username:</strong> {match.UserName}<br/>" +
                    $"<strong>Temporary password:</strong> {tempPassword}</p>" +
                    "<p>Log in with these - you'll be asked to set your own password right after.</p>" +
                    "<p>Didn't request this? Contact your administrator.</p>");
            }

            _logger.LogInformation("Self-service password reset requested for '{Input}'; matched {MatchCount} account(s).", model.UsernameOrEmail, matches.Count);

            ViewData["AdminContactEmail"] = await GetPrimaryAdminContactEmailAsync();
            return View("ForgotPasswordConfirmation");
        }

        private async Task<string> GetPrimaryAdminContactEmailAsync()
        {
            var admins = await _userManager.GetUsersInRoleAsync(Constants.AdminRole);
            var primary = admins
                .Where(a => !string.IsNullOrWhiteSpace(a.Email))
                .OrderBy(a => a.CreatedAt)
                .FirstOrDefault();
            return primary?.Email ?? "your system administrator";
        }

        // GET: Account/ChangeTempPassword
        // Forced next step after signing in with a temp password an admin
        // issued via UsersController.ResetPassword.
        [HttpGet]
        public async Task<IActionResult> ChangeTempPassword()
        {
            var user = await GetMustChangePasswordUserOrNullAsync();
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeTempPassword(ChangeTempPasswordViewModel model)
        {
            var user = await GetMustChangePasswordUserOrNullAsync();
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var passwordResult = await _userManager.ChangePasswordAsync(user, model.TempPassword, model.NewPassword);
            if (!passwordResult.Succeeded)
            {
                foreach (var error in passwordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            user.MustChangePassword = false;
            await _userManager.UpdateAsync(user);

            // Security stamp changed - refresh the auth cookie so the
            // current session isn't kicked out.
            await _signInManager.RefreshSignInAsync(user);

            TempData["SuccessMessage"] = "Password updated.";
            return RedirectToAction("Index", "Home");
        }

        private async Task<AppUser> GetMustChangePasswordUserOrNullAsync()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null || !user.MustChangePassword)
            {
                return null;
            }

            return user;
        }

        private async Task<AppUser> GetSetupUserOrNullAsync()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null || !user.IsFirstLogin)
            {
                return null;
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains(Constants.AdminRole) && !roles.Contains(Constants.StaffRole))
            {
                return null;
            }

            return user;
        }
    }
}

