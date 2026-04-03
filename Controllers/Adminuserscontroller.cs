using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Alumni_Management_System.Models;

namespace Alumni_Management_System.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        private static readonly string[] ExcludedRoles = { "Alumni" };

        public AdminUsersController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public async Task<IActionResult> Index()
        {
            // Get all users
            var allUsers = await _userManager.Users.ToListAsync();

            // Prepare a list for users to display
            var userRoles = new List<(AppUser User, string Role)>();

            foreach (var user in allUsers)
            {
                // Get all roles assigned to this user
                var roles = await _userManager.GetRolesAsync(user);

                // Skip Alumni users entirely
                if (roles.Any(r => r == Constants.AlumniRole))
                    continue;

                // Pick the first allowed role (Admin, Staff, etc.)
                var primaryRole = roles.FirstOrDefault(r => r != Constants.AlumniRole) ?? "No Role";

                userRoles.Add((user, primaryRole));
            }

            // Get all roles except Alumni for dropdowns
            var allowedRoles = await _roleManager.Roles
                .Where(r => r.Name != Constants.AlumniRole)
                .Select(r => r.Name)
                .ToListAsync();

            ViewBag.AllRoles = allowedRoles;
            ViewBag.UserRoles = userRoles;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRole(string userId, string newRole)
        {
            if (ExcludedRoles.Contains(newRole))
            {
                TempData["Error"] = "Cannot assign the Alumni role from User Management.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            var currentRoles = await _userManager.GetRolesAsync(user);

            if (currentRoles.Any(r => ExcludedRoles.Contains(r)))
            {
                TempData["Error"] = "Alumni user roles cannot be changed from User Management.";
                return RedirectToAction(nameof(Index));
            }

            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            if (!string.IsNullOrEmpty(newRole))
            {
                if (!await _roleManager.RoleExistsAsync(newRole))
                    await _roleManager.CreateAsync(new IdentityRole(newRole));

                await _userManager.AddToRoleAsync(user, newRole);
            }

            TempData["Success"] = $"Role updated to '{newRole}' for {user.Email}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var defaultPassword = "Reset@1234";
            var result = await _userManager.ResetPasswordAsync(user, token, defaultPassword);

            if (result.Succeeded)
                TempData["Success"] = $"Password reset for {user.Email}. Temporary password: {defaultPassword}";
            else
                TempData["Error"] = "Password reset failed: " + string.Join(", ", result.Errors.Select(e => e.Description));

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Id == userId)
            {
                TempData["Error"] = "You cannot delete your own account.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
                TempData["Success"] = $"User {user.Email} deleted successfully.";
            else
                TempData["Error"] = "Delete failed: " + string.Join(", ", result.Errors.Select(e => e.Description));

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Register()
        {
            ViewBag.AllRoles = _roleManager.Roles
                .Select(r => r.Name)
                .Where(r => !ExcludedRoles.Contains(r))
                .ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string username, string email, string password, string confirmPassword, string role)
        {
            ViewBag.AllRoles = _roleManager.Roles
                .Select(r => r.Name)
                .Where(r => !ExcludedRoles.Contains(r))
                .ToList();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                TempData["Error"] = "Username, email and password are all required.";
                return View();
            }

            if (password != confirmPassword)
            {
                TempData["Error"] = "Passwords do not match.";
                return View();
            }

            if (ExcludedRoles.Contains(role))
            {
                TempData["Error"] = "Cannot create an Alumni user from this page.";
                return View();
            }

            var existingByUsername = await _userManager.FindByNameAsync(username);
            if (existingByUsername != null)
            {
                TempData["Error"] = "Username is already taken. Please choose a different one.";
                return View();
            }

            var existingByEmail = await _userManager.FindByEmailAsync(email);
            if (existingByEmail != null)
            {
                TempData["Error"] = "A user with this email already exists.";
                return View();
            }

            var allJagIds = await _userManager.Users
                .Where(u => u.JagId != null && u.JagId.StartsWith("J00"))
                .Select(u => u.JagId)
                .ToListAsync();

            int maxNumber = allJagIds
                .Select(id => int.TryParse(id.Substring(3), out int n) ? n : 0)
                .DefaultIfEmpty(0)
                .Max();

            string nextJagId = $"J00{(maxNumber + 1):D4}";

            var newUser = new AppUser
            {
                UserName = username,
                Email = email,
                EmailConfirmed = true,
                JagId = nextJagId,
                CreatedAt = DateTime.Now,
                IsFirstLogin = true
            };

            var result = await _userManager.CreateAsync(newUser, password);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(role))
                {
                    if (!await _roleManager.RoleExistsAsync(role))
                        await _roleManager.CreateAsync(new IdentityRole(role));
                    await _userManager.AddToRoleAsync(newUser, role);
                }
                TempData["Success"] = $"User '{username}' registered with role '{role}'. JAG ID: {nextJagId}";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Registration failed: " + string.Join(", ", result.Errors.Select(e => e.Description));
            return View();
        }
    }
}