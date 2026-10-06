using Alumni_Management_System.Data;
using Alumni_Management_System.Models;
using Alumni_Management_System.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;



namespace Alumni_Management_System.Controllers
{
    // Staff can compose/send messages (Create, Edit, and the send-mail flow
    // below); only Delete stays Admin-only (see its own [Authorize] override).
    [Authorize(Roles = "Admin,Staff")]
    public class MessagesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailSender _emailSender;

        public MessagesController(ApplicationDbContext context, UserManager<AppUser> userManager, IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        // GET: Messages
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Messages.Include(m => m.CreatedByNavigation);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Messages/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var message = await _context.Messages
                .Include(m => m.CreatedByNavigation)
                .FirstOrDefaultAsync(m => m.MessageId == id);
            if (message == null)
            {
                return NotFound();
            }

            return View(message);
        }

        // GET: Messages/Create
        public IActionResult Create()
        {
            // No dropdown needed - CreatedBy will be auto-assigned to current admin
            return View();
        }

        // POST: Messages/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MessageId,Title,MessageBody,MessageType")] Message message)
        {
            // Auto-assign CreatedBy to current admin user
            var currentUser = await _userManager.GetUserAsync(User);
            message.CreatedBy = currentUser.Id;
            message.CreatedAt = DateTime.Now;

            if (ModelState.IsValid)
            {
                _context.Add(message);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Message created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(message);
        }

        // GET: Messages/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var message = await _context.Messages.FindAsync(id);
            if (message == null)
            {
                return NotFound();
            }
            if (!CanEdit(message))
            {
                return NotYourMessage();
            }
            // No dropdown needed - CreatedBy cannot be edited
            return View(message);
        }

        // POST: Messages/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MessageId,Title,MessageBody,MessageType")] Message message)
        {
            if (id != message.MessageId)
            {
                return NotFound();
            }

            // Preserve original CreatedBy and CreatedAt
            var originalMessage = await _context.Messages.AsNoTracking().FirstOrDefaultAsync(m => m.MessageId == id);
            if (originalMessage == null)
            {
                return NotFound();
            }
            if (!CanEdit(originalMessage))
            {
                return NotYourMessage();
            }

            message.CreatedBy = originalMessage.CreatedBy;
            message.CreatedAt = originalMessage.CreatedAt;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(message);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Message updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MessageExists(message.MessageId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(message);
        }

        // GET: Messages/Delete/5
        [Authorize(Roles = "Admin")] // Only Admin can delete messages
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var message = await _context.Messages
                .Include(m => m.CreatedByNavigation)
                .FirstOrDefaultAsync(m => m.MessageId == id);
            if (message == null)
            {
                return NotFound();
            }

            var inUse = await InUseReasonAsync(message);
            if (inUse != null)
            {
                TempData["ErrorMessage"] = inUse;
                return RedirectToAction(nameof(Index));
            }

            return View(message);
        }

        // POST: Messages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // Only Admin can delete messages
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message != null)
            {
                var inUse = await InUseReasonAsync(message);
                if (inUse != null)
                {
                    TempData["ErrorMessage"] = inUse;
                    return RedirectToAction(nameof(Index));
                }

                _context.Messages.Remove(message);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Message deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        // Admins can edit any message; Staff only the ones they wrote.
        private bool CanEdit(Message message) =>
            User.IsInRole(Constants.AdminRole) || message.CreatedBy == _userManager.GetUserId(User);

        private IActionResult NotYourMessage()
        {
            TempData["ErrorMessage"] = "You can only edit messages you created.";
            return RedirectToAction(nameof(Index));
        }

        // A sent message is the record of what alumni received, so it stays.
        private async Task<string> InUseReasonAsync(Message message)
        {
            var sentTo = await _context.AlumniMessages.CountAsync(am => am.MessageId == message.MessageId);
            return sentTo == 0 ? null
                : $"\"{message.Title}\" can't be deleted - it was sent to {sentTo} {(sentTo == 1 ? "alumnus" : "alumni")} and is kept as a record of what they received.";
        }

        private bool MessageExists(int id)
        {
            return _context.Messages.Any(e => e.MessageId == id);
        }
        public async Task<IActionResult> Alumnimessagefilter(int id)
        {
            ViewBag.MessageId = id;

            // ✅ Get already mapped alumni for this message
            var mappedAlumniIds = await _context.AlumniMessages
                .Where(x => x.MessageId == id)
                .Select(x => x.AlumniId)
                .ToListAsync();

            // Only alumni inside the sender's scope (see AccessScopeService.ApplyTo).
            var data = await (await GetScopedAlumniAsync())
                .Where(a => a.SolicitationCode == true)
                .Include(a => a.AlumniDegrees)
                    .ThenInclude(d => d.Degree)
                .Select(a => new AlumniMailingViewmodel
                {
                    AlumniId = a.AlumniId,

                    FirstName = a.FirstName,
                    LastName = a.LastName,
                    PermanentEmail = a.PermanentEmail,
                    GraduationYear = a.GraduationYear,

                    // ✅ All degrees
                    Degree = string.Join(", ",
                        a.AlumniDegrees
                            .Select(d => d.Degree.MajorFieldOfStudy)
                            .Distinct()
                    ),

                    // ✅ Degree Type
                    DegreeType = string.Join(", ",
                        a.AlumniDegrees
                            .Select(d => d.Degree.DegreeType)
                            .Distinct()
                    ),

                    // ✅ Already mapped logic
                    IsAlreadyMapped = mappedAlumniIds.Contains(a.AlumniId),

                    // ✅ Pre-select already mapped
                    IsSelected = mappedAlumniIds.Contains(a.AlumniId)
                })
                .ToListAsync();

            return View(data);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveSelectedAlumniMessage(int MessageId, List<AlumniMailingViewmodel> model)
        {
            var message = await _context.Messages.FindAsync(MessageId);
            if (message == null)
            {
                return NotFound();
            }

            // Only newly ticked alumni - the greyed-out rows already received it.
            var selectedIds = model
                .Where(x => x.IsSelected && !x.IsAlreadyMapped)
                .Select(x => x.AlumniId)
                .Distinct()
                .ToList();

            var alreadySentIds = await _context.AlumniMessages
                .Where(x => x.MessageId == MessageId)
                .Select(x => x.AlumniId)
                .ToListAsync();

            // Recipients and their emails come from the database, not the
            // posted form - only alumni inside the sender's scope who allow
            // contact, so neither the scope nor the opt-out can be bypassed.
            var recipients = await (await GetScopedAlumniAsync())
                .Where(a => selectedIds.Contains(a.AlumniId) && a.SolicitationCode
                            && !alreadySentIds.Contains(a.AlumniId)
                            && a.PermanentEmail != null && a.PermanentEmail != "")
                .Select(a => new { a.AlumniId, a.PermanentEmail })
                .ToListAsync();

            if (!recipients.Any())
            {
                TempData["ErrorMessage"] = "No alumni were selected (or none have an email on file) - nothing was sent.";
                return RedirectToAction("Index");
            }

            var htmlBody = BuildEmailBody(message.MessageBody);
            int sent = 0;
            var failures = new List<string>();

            foreach (var item in recipients)
            {
                try
                {
                    await _emailSender.SendEmailAsync(item.PermanentEmail, message.Title, htmlBody);
                }
                catch (Exception ex)
                {
                    failures.Add(ex.Message);
                    continue;
                }

                // Recorded only once the email actually went out, so a failed
                // send leaves the alumnus selectable for another try.
                _context.AlumniMessages.Add(new AlumniMessage
                {
                    MessageId = MessageId,
                    AlumniId = item.AlumniId,
                    SentAt = DateTime.Now
                });
                sent++;
            }

            await _context.SaveChangesAsync();

            if (sent > 0)
            {
                TempData["SuccessMessage"] = $"{sent} {(sent == 1 ? "email" : "emails")} sent successfully.";
            }
            if (failures.Any())
            {
                // Usually one cause for all of them (e.g. email not configured), so list each reason once.
                var reasons = string.Join(" ", failures.Distinct());
                TempData["ErrorMessage"] = $"{failures.Count} {(failures.Count == 1 ? "email" : "emails")} failed to send: {reasons}";
            }

            return RedirectToAction("Index");
        }
        private async Task<IQueryable<Alumni>> GetScopedAlumniAsync()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);
            var scope = await Services.AccessScopeService.GetScopeAsync(_context, currentUser, roles);
            return Services.AccessScopeService.ApplyTo(_context.Alumni, scope);
        }

        private static string BuildEmailBody(string body) => $@"
                    <p>Hello,</p>
                    <p>{body}</p>
                    <br/>
                    <p>Thanks,<br/>Alumni Management System,<br/>University of South Alabama</p>";

    }
}
