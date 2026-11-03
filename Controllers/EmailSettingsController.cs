using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Alumni_Management_System.Models.ViewModels;
using Alumni_Management_System.Services;

namespace Alumni_Management_System.Controllers
{
    // Lets Admins change the SMTP server, mailbox and password used for all
    // outgoing email without a code change. Stored as a file - see
    // EmailSettingsStore.
    [Authorize(Roles = "Admin")]
    public class EmailSettingsController : Controller
    {
        private readonly EmailSettingsStore _store;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<EmailSettingsController> _logger;

        public EmailSettingsController(EmailSettingsStore store, IEmailSender emailSender, ILogger<EmailSettingsController> logger)
        {
            _store = store;
            _emailSender = emailSender;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var effective = _store.GetEffective();
            var model = new EmailSettingsViewModel
            {
                Host = effective.Host,
                Port = effective.Port,
                EnableSsl = effective.EnableSsl,
                FromAddress = effective.FromAddress,
                FromName = effective.FromName,
                Username = effective.Username
            };
            return View(Prepare(model));
        }

        // POST: EmailSettings - "save" writes the settings; "test" saves
        // them too and then sends a test email with them.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(EmailSettingsViewModel model, string submit)
        {
            if (string.IsNullOrEmpty(model.NewPassword) && !_store.HasPassword())
            {
                ModelState.AddModelError(nameof(model.NewPassword), "Please enter the SMTP password.");
            }
            if (submit != "test")
            {
                ModelState.Remove(nameof(model.TestEmailTo));
            }
            else if (string.IsNullOrWhiteSpace(model.TestEmailTo))
            {
                ModelState.AddModelError(nameof(model.TestEmailTo), "Enter an email address to send the test to.");
            }
            if (!ModelState.IsValid)
            {
                return View(Prepare(model));
            }

            await _store.SaveAsync(new SmtpOptions
            {
                Host = model.Host,
                Port = model.Port,
                EnableSsl = model.EnableSsl,
                FromAddress = model.FromAddress,
                FromName = string.IsNullOrWhiteSpace(model.FromName) ? "Alumni Management System" : model.FromName,
                Username = model.Username
            }, model.NewPassword, User.Identity?.Name);

            if (submit != "test")
            {
                TempData["SuccessMessage"] = "Email settings were saved. New emails will use them right away.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                await _emailSender.SendEmailAsync(model.TestEmailTo.Trim(), "Alumni Management System - test email",
                    "<p>This is a test email from the Alumni Management System.</p><p>If you received it, the email settings are working.</p>");
                TempData["SuccessMessage"] = $"Email settings were saved, and a test email was sent to {model.TestEmailTo.Trim()}.";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Test email to '{Email}' failed.", model.TestEmailTo);
                TempData["ErrorMessage"] = $"Email settings were saved, but the test email failed: {ex.GetBaseException().Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reset()
        {
            await _store.ResetAsync(User.Identity?.Name);
            TempData["SuccessMessage"] = "Email settings were reset to the values in appsettings.json.";
            return RedirectToAction(nameof(Index));
        }

        private EmailSettingsViewModel Prepare(EmailSettingsViewModel model)
        {
            var saved = _store.GetSaved();
            model.NewPassword = null;
            model.HasPassword = _store.HasPassword();
            model.IsCustomized = saved != null;
            model.UpdatedBy = saved?.UpdatedBy;
            model.UpdatedAt = saved?.UpdatedAt;
            return model;
        }
    }
}
