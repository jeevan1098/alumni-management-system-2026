using Alumni_Management_System.Data;
using Alumni_Management_System.Models;
using Alumni_Management_System.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;



namespace Alumni_Management_System.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class MessagesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public MessagesController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Messages.Include(m => m.CreatedByNavigation);
            return View(await applicationDbContext.ToListAsync());
        }

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

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("MessageId,Title,MessageBody,MessageType")] Message message)
        {

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

        [Authorize(Roles = "Admin")]
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

            return View(message);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MessageId,Title,MessageBody,MessageType")] Message message)
        {
            if (id != message.MessageId)
            {
                return NotFound();
            }

            var originalMessage = await _context.Messages.AsNoTracking().FirstOrDefaultAsync(m => m.MessageId == id);
            if (originalMessage == null)
            {
                return NotFound();
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

            return View(message);
        }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var message = await _context.Messages.FindAsync(id);

        if (message == null)
        {
            return NotFound();
        }

        bool isSentToAlumni = await _context.AlumniMessages
            .AnyAsync(am => am.MessageId == id);

        if (isSentToAlumni)
        {
            TempData["ErrorMessage"] = "This message cannot be deleted because it is already sent to alumni.";
            return RedirectToAction(nameof(Index));
        }

        _context.Messages.Remove(message);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Message deleted successfully!";
        return RedirectToAction(nameof(Index));
    }

        private bool MessageExists(int id)
        {
            return _context.Messages.Any(e => e.MessageId == id);
        }
        public async Task<IActionResult> Alumnimessagefilter(int id)
        {
            ViewBag.MessageId = id;

            //  Get already mapped alumni for this message
            var mappedAlumniIds = await _context.AlumniMessages
                .Where(x => x.MessageId == id)
                .Select(x => x.AlumniId)
                .ToListAsync();

            var data = await _context.Alumni
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

                    //  All degrees
                    Degree = string.Join(", ",
                        a.AlumniDegrees
                            .Select(d => d.Degree.MajorFieldOfStudy)
                            .Distinct()
                    ),

                    //  Degree Type
                    DegreeType = string.Join(", ",
                        a.AlumniDegrees
                            .Select(d => d.Degree.DegreeType)
                            .Distinct()
                    ),

                    //  Already mapped logic
                    IsAlreadyMapped = mappedAlumniIds.Contains(a.AlumniId),

                    //  Pre-select already mapped
                    IsSelected = mappedAlumniIds.Contains(a.AlumniId)
                })
                .ToListAsync();

            return View(data);
        }


        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> SaveSelectedAlumniMessage(int MessageId, List<AlumniMailingViewmodel> model)
        {
            //  Include both selected + already mapped
            var selectedAlumni = model
                .Where(x => x.IsSelected || x.IsAlreadyMapped)
                .ToList();

            List<string> emailsToSend = new List<string>();

            foreach (var item in selectedAlumni)
            {
                //  Prevent duplicate mapping
                var exists = await _context.AlumniMessages
                    .AnyAsync(x => x.MessageId == MessageId && x.AlumniId == item.AlumniId);

                if (!exists)
                {
                    _context.AlumniMessages.Add(new AlumniMessage
                    {
                        MessageId = MessageId,
                        AlumniId = item.AlumniId,
                        SentAt = DateTime.Now
                    });
                }

                //  Always send email (even if already mapped)
                if (!string.IsNullOrEmpty(item.PermanentEmail))
                {
                    emailsToSend.Add(item.PermanentEmail);
                }
            }

            //  Save DB first
            await _context.SaveChangesAsync();

            //  Get message content
            var message = await _context.Messages.FindAsync(MessageId);

            //  Send emails
            if (message != null && emailsToSend.Any())
            {
                await SendEmailAsync(
                    emailsToSend.Distinct().ToList(), // avoid duplicates
                    message.Title,
                    message.MessageBody
                );
            }

            return RedirectToAction("Index");
        }
        private async Task SendEmailAsync(List<string> emails, string subject, string body)
        {
            var fromAddress = new MailAddress("socalumnimanagement@southalabama.edu", "Alumni Management System");
            const string fromPassword = "lmspheqscoortfol";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };

            int successCount = 0;
            int failCount = 0;

            foreach (var email in emails)
            {
                try
                {
                    var toAddress = new MailAddress(email);

                    using (var message = new MailMessage(fromAddress, toAddress)
                    {
                        Subject = subject,
                        Body = $@"
                    <p>Hello,</p>
                    <p>{body}</p>
                    <br/>
                    <p>Thanks,<br/>Alumni Management System,<br/>University of South Alabama</p>",
                        IsBodyHtml = true
                    })
                    {
                        await smtp.SendMailAsync(message);
                        successCount++;
                    }
                }
                catch
                {
                    failCount++;
                }
            }

            TempData["SuccessMessage"] = $"{successCount} emails sent successfully.";

            if (failCount > 0)
            {
                TempData["ErrorMessage"] = $"{failCount} emails failed to send.";
            }
        }

    }
}
