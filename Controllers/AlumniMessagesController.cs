using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Alumni_Management_System.Data;
using Alumni_Management_System.Models;

namespace Alumni_Management_System.Controllers
{
    [Authorize(Roles = "Admin,Staff,Alumni")]
    public class AlumniMessagesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public AlumniMessagesController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);

            IQueryable<AlumniMessage> query = _context.AlumniMessages.Include(a => a.Alumni).Include(a => a.Message);

            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                if (alumni != null)
                {
                    query = query.Where(am => am.AlumniId == alumni.AlumniId);
                    ViewData["UserRole"] = Constants.AlumniRole;
                }
                else
                {
                    return View(new List<AlumniMessage>());
                }
            }
            else
            {
                ViewData["UserRole"] = "Admin";
            }

            return View(await query.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alumniMessage = await _context.AlumniMessages
                .Include(a => a.Alumni)
                .Include(a => a.Message)
                .FirstOrDefaultAsync(m => m.AlumniMessageId == id);
            if (alumniMessage == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);
            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                if (alumni == null || alumniMessage.AlumniId != alumni.AlumniId)
                {
                    TempData["ErrorMessage"] = "You can only view your own messages.";
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(alumniMessage);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {

            ViewData["AlumniId"] = new SelectList(_context.Alumni .Where(a => a.SolicitationCode).Select(a => new
                                    {
                                        a.AlumniId,
                                        FullName = a.FirstName + " " + a.LastName 
                                    }).ToList(), "AlumniId", "FullName");

            ViewData["MessageId"] = new SelectList(_context.Messages, "MessageId", "MessageBody");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("AlumniMessageId,AlumniId,MessageId,SentAt")] AlumniMessage alumniMessage)
        {
            if (ModelState.IsValid)
            {
                _context.Add(alumniMessage);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Message sent successfully!";
                return RedirectToAction(nameof(Index));
            }
            ViewData["AlumniId"] = new SelectList(_context.Alumni, "AlumniId", "FirstName", alumniMessage.AlumniId);
            ViewData["MessageId"] = new SelectList(_context.Messages, "MessageId", "MessageBody", alumniMessage.MessageId);
            return View(alumniMessage);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alumniMessage = await _context.AlumniMessages.FindAsync(id);
            if (alumniMessage == null)
            {
                return NotFound();
            }
            ViewData["AlumniId"] = new SelectList(_context.Alumni, "AlumniId", "FirstName", alumniMessage.AlumniId);
            ViewData["MessageId"] = new SelectList(_context.Messages, "MessageId", "MessageBody", alumniMessage.MessageId);
            return View(alumniMessage);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AlumniMessageId,AlumniId,MessageId,SentAt")] AlumniMessage alumniMessage)
        {
            if (id != alumniMessage.AlumniMessageId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(alumniMessage);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Message updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AlumniMessageExists(alumniMessage.AlumniMessageId))
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
            ViewData["AlumniId"] = new SelectList(_context.Alumni, "AlumniId", "FirstName", alumniMessage.AlumniId);
            ViewData["MessageId"] = new SelectList(_context.Messages, "MessageId", "MessageBody", alumniMessage.MessageId);
            return View(alumniMessage);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alumniMessage = await _context.AlumniMessages
                .Include(a => a.Alumni)
                .Include(a => a.Message)
                .FirstOrDefaultAsync(m => m.AlumniMessageId == id);
            if (alumniMessage == null)
            {
                return NotFound();
            }

            return View(alumniMessage);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var alumniMessage = await _context.AlumniMessages.FindAsync(id);
            if (alumniMessage != null)
            {
                _context.AlumniMessages.Remove(alumniMessage);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Message deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool AlumniMessageExists(int id)
        {
            return _context.AlumniMessages.Any(e => e.AlumniMessageId == id);
        }
    }
}
