using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Alumni_Management_System.Models;

namespace Alumni_Management_System.Controllers
{
    // Lets a signed-in user turn email-based 2FA on/off. Optional/opt-in for
    // every role for now (Admin/Staff included) - IsMandatory stays wired up
    // so a future per-user "force 2FA" flag can flip it on for a specific
    // account without touching this controller's structure again.
    [Authorize]
    public class TwoFactorSettingsController : Controller
    {
        private readonly UserManager<AppUser> _userManager;

        public TwoFactorSettingsController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewData["IsMandatory"] = false;
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enable()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            await _userManager.SetTwoFactorEnabledAsync(user, true);
            TempData["SuccessMessage"] = "Two-factor authentication enabled. You'll be emailed a code on your next login.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Disable()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            await _userManager.SetTwoFactorEnabledAsync(user, false);
            TempData["SuccessMessage"] = "Two-factor authentication disabled.";
            return RedirectToAction(nameof(Index));
        }
    }
}
