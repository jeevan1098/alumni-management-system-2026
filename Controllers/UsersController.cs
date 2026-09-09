using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Alumni_Management_System.Data;
using Alumni_Management_System.Models;
using Alumni_Management_System.Models.ViewModels;

namespace Alumni_Management_System.Controllers
{
    // Admin-only screen to create Staff/Admin accounts directly. Alumni
    // accounts are intentionally NOT created here - they go through the
    // existing Alumni Registry -> VerifyJagId -> RegisterAlumni self-service
    // flow, since an Alumni account must link to a real Alumni/JagId record.
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailSender _emailSender;

        public UsersController(ApplicationDbContext context, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.OrderBy(u => u.UserName).ToListAsync();
            var rolesByUserId = new Dictionary<string, IList<string>>();
            foreach (var user in users)
            {
                rolesByUserId[user.Id] = await _userManager.GetRolesAsync(user);
            }
            ViewData["RolesByUserId"] = rolesByUserId;

            // Scoping is granted per (user, role) - Admin/Staff accounts here
            // have exactly one relevant role, so a single lookup by UserId is
            // enough for the inline "Manage Scope" control on this page.
            var scopeByUserId = await _context.UserAccessScopes
                .Include(s => s.College)
                .Where(s => s.IsActive)
                .ToDictionaryAsync(s => s.UserId);
            ViewData["ScopeByUserId"] = scopeByUserId;

            ViewData["Colleges"] = new SelectList(await _context.Colleges.Where(c => c.IsActive).ToListAsync(), "CollegeId", "CollegeName");

            return View(users);
        }

        // POST: Users/SetScope - inline scope management from Manage Users,
        // so an admin doesn't have to leave this page to grant/change one.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetScope(string userId, int? collegeId, string accessLevel)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var primaryRole = roles.FirstOrDefault(r => r == Constants.AdminRole || r == Constants.StaffRole);
            if (primaryRole == null)
            {
                TempData["ErrorMessage"] = "Access scopes only apply to Admin/Staff accounts.";
                return RedirectToAction(nameof(Index));
            }

            var role = await _roleManager.FindByNameAsync(primaryRole);

            var scope = await _context.UserAccessScopes
                .FirstOrDefaultAsync(s => s.UserId == userId && s.RoleId == role.Id);

            if (scope == null)
            {
                scope = new UserAccessScope { UserId = userId, RoleId = role.Id };
                _context.UserAccessScopes.Add(scope);
            }

            scope.CollegeId = collegeId;
            scope.AccessLevel = string.IsNullOrWhiteSpace(accessLevel) ? "Full" : accessLevel;
            scope.IsActive = true;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Access scope updated for {user.UserName}.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Users/RemoveScope/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveScope(int id)
        {
            var scope = await _context.UserAccessScopes.FindAsync(id);
            if (scope != null)
            {
                _context.UserAccessScopes.Remove(scope);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Access scope removed.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Users/AddRole - grants an ADDITIONAL role to an existing
        // account (e.g. an Alumni who is also becoming Staff), so a person
        // never needs a second account for the same JAG ID.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRole(string userId, string role)
        {
            if (role != Constants.AdminRole && role != Constants.StaffRole)
            {
                TempData["ErrorMessage"] = "Role must be Admin or Staff.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            if (await _userManager.IsInRoleAsync(user, role))
            {
                TempData["ErrorMessage"] = $"{user.UserName} already has the {role} role.";
                return RedirectToAction(nameof(Index));
            }

            await _userManager.AddToRoleAsync(user, role);
            TempData["SuccessMessage"] = $"{role} role added for {user.UserName}.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Users/RemoveRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveRole(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            if (user.UserName == "admin" && role == Constants.AdminRole)
            {
                TempData["ErrorMessage"] = "The default admin account must keep the Admin role.";
                return RedirectToAction(nameof(Index));
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Count <= 1)
            {
                TempData["ErrorMessage"] = $"Can't remove {user.UserName}'s last role - they'd be left without any access.";
                return RedirectToAction(nameof(Index));
            }

            await _userManager.RemoveFromRoleAsync(user, role);
            TempData["SuccessMessage"] = $"{role} role removed from {user.UserName}.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Users/Create
        public async Task<IActionResult> Create()
        {
            return View(new CreateUserViewModel { JagId = await GenerateStaffJagIdAsync() });
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (model.Role != Constants.AdminRole && model.Role != Constants.StaffRole)
            {
                ModelState.AddModelError(nameof(model.Role), "Role must be Admin or Staff.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // One JagId -> one account: if this JagId already has an account
            // (of any role), grant it the newly requested role instead of
            // creating a second account for the same person - same outcome
            // as clicking "Add Role" for them on the Manage Users page.
            var existingJagId = await _userManager.Users.FirstOrDefaultAsync(u => u.JagId == model.JagId);
            if (existingJagId != null)
            {
                if (await _userManager.IsInRoleAsync(existingJagId, model.Role))
                {
                    ModelState.AddModelError(nameof(model.JagId), $"{existingJagId.UserName} already has the {model.Role} role for this JAG ID.");
                    return View(model);
                }

                await _userManager.AddToRoleAsync(existingJagId, model.Role);
                TempData["SuccessMessage"] = $"An account already exists for this JAG ID ({existingJagId.UserName}) - added the {model.Role} role to it instead of creating a new account.";
                return RedirectToAction(nameof(Index));
            }

            // Past this point we're actually creating a brand-new account,
            // so Email/Password (optional on the model - see
            // CreateUserViewModel) become required.
            if (string.IsNullOrWhiteSpace(model.Email))
            {
                ModelState.AddModelError(nameof(model.Email), "Email is required to create a new account.");
            }
            if (string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError(nameof(model.Password), "A temporary password is required to create a new account.");
            }
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Two accounts sharing an email crashes any code path that does
            // an email lookup expecting a single match (e.g. Forgot
            // Password) - block it here rather than let it happen silently.
            var normalizedEmail = _userManager.NormalizeEmail(model.Email);
            if (await _userManager.Users.AnyAsync(u => u.NormalizedEmail == normalizedEmail))
            {
                ModelState.AddModelError(nameof(model.Email), "This email is already used by another account.");
                return View(model);
            }

            // Username is a placeholder, not chosen by the admin - the new
            // hire picks their real username during the forced first-login
            // setup flow (see AccountController.CompleteSetup).
            var placeholderUsername = $"pending_{model.JagId}";

            var user = new AppUser
            {
                UserName = placeholderUsername,
                Email = model.Email,
                EmailConfirmed = true,
                JagId = model.JagId,
                CreatedAt = DateTime.Now,
                IsFirstLogin = true,
                TwoFactorEnabled = false // opt-in - user can enable it later in Two-Factor Auth settings
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            await _userManager.AddToRoleAsync(user, model.Role);

            await _emailSender.SendEmailAsync(
                user.Email,
                "Your Alumni Management System account",
                $"<p>A {model.Role} account has been created for you.</p>" +
                $"<p><strong>Temporary username:</strong> {placeholderUsername}<br/>" +
                $"<strong>Temporary password:</strong> {model.Password}</p>" +
                "<p>Log in with these at the site, then you'll be asked to choose your own username and password.</p>");

            TempData["SuccessMessage"] = $"{model.Role} account created. Temporary login credentials were emailed to {user.Email}.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            ViewData["Roles"] = string.Join(", ", roles);
            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            if (user.UserName == "admin")
            {
                TempData["ErrorMessage"] = "The default admin account cannot be deleted.";
                return RedirectToAction(nameof(Index));
            }

            await _userManager.DeleteAsync(user);
            TempData["SuccessMessage"] = "User account deleted.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Users/ResetPassword - triggered by an admin after a
        // forgot-password request (see AccountController.ForgotPassword).
        // Generates a fresh temp password, forces the user to change it on
        // next sign-in, and emails it to them directly (the admin never sees
        // it, same as the temp password on account creation).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var tempPassword = Services.PasswordGenerator.GenerateTempPassword();
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, tempPassword);

            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = "Could not reset password: " + string.Join(" ", result.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(Index));
            }

            user.MustChangePassword = true;
            await _userManager.UpdateAsync(user);

            await _emailSender.SendEmailAsync(
                user.Email,
                "Your password has been reset - Alumni Management System",
                $"<p>Your administrator reset your password.</p>" +
                $"<p><strong>Temporary password:</strong> {tempPassword}</p>" +
                $"<p>Log in with your existing username (<strong>{user.UserName}</strong>) and this temporary password - you'll be asked to set your own right after.</p>");

            TempData["SuccessMessage"] = $"Password reset for {user.UserName}. A temporary password was emailed to {user.Email}.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<string> GenerateStaffJagIdAsync()
        {
            var existingIds = await _userManager.Users
                .Select(u => u.JagId)
                .Where(j => j != null && j.StartsWith("J00"))
                .ToListAsync();

            var maxNumber = existingIds
                .Select(j => int.TryParse(j.Substring(3), out var n) ? n : 0)
                .DefaultIfEmpty(0)
                .Max();

            return $"J00{(maxNumber + 1):D6}";
        }
    }
}
