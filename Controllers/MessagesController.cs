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
using System.Threading.Tasks;


namespace Alumni_Management_System.Controllers
{
    [Authorize(Roles = "Admin")]
    public class MessagesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public MessagesController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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
                _context.Messages.Remove(message);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Message deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool MessageExists(int id)
        {
            return _context.Messages.Any(e => e.MessageId == id);
        }
        public async Task<IActionResult> Alumnimessagefilter(int id)
        {
            var messageId = id;

            var data = await _context.Alumni
                .Where(a => a.SolicitationCode == true
                            && a.IsActive == false
                            && a.User != null
                            )
                .Include(a => a.AlumniDegrees)
                    .ThenInclude(d => d.Degree)
                .Select(a => new AlumniMailingViewmodel
                {
                    AlumniId = a.AlumniId,

                    FirstName = a.FirstName,
                    LastName = a.LastName,
                    PermanentEmail = a.PermanentEmail,
                    GraduationYear = a.GraduationYear,

                    // ✅ Degree Name
                    Degree = string.Join(", ",
                        a.AlumniDegrees
                            .Select(d => d.Degree.MajorFieldOfStudy)
                            .Distinct()
                    ),

                    // ✅ NEW: Degree Type (BS, MS, etc.)
                    DegreeType = string.Join(", ",
                        a.AlumniDegrees
                            .Select(d => d.Degree.DegreeType)
                            .Distinct()
                    )
                })
                .ToListAsync();

            return View(data);
        }
        [HttpPost]
        public async Task<IActionResult> SaveSelectedAlumniMessage(int MessageId, List<AlumniMailingViewmodel> model)
        {
            var selectedAlumni = model.Where(x => x.IsSelected).ToList();

            foreach (var item in selectedAlumni)
            {
                // prevent duplicate entries
                var exists = await _context.AlumniMessages
                    .AnyAsync(x => x.MessageId == MessageId && x.AlumniId == item.AlumniId);

                if (!exists)
                {
                    _context.AlumniMessages.Add(new AlumniMessage
                    {
                        MessageId = MessageId,
                        AlumniId = item.AlumniId,
                        SentAt = DateTime.Now   // or null if not sent yet
                    });
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

    }
}
