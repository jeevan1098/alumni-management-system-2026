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
    [Authorize]
    public class AlumniMessagesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public AlumniMessagesController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: AlumniMessages
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);

            IQueryable<AlumniMessage> query = _context.AlumniMessages.Include(a => a.Alumni).Include(a => a.Message);

            // Alumni can only see their own messages
            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.UserId == currentUser.Id);
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

        // GET: AlumniMessages/Details/5
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

            // Check if Alumni user is trying to view another alumni's message
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);
            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.UserId == currentUser.Id);
                if (alumni == null || alumniMessage.AlumniId != alumni.AlumniId)
                {
                    TempData["ErrorMessage"] = "You can only view your own messages.";
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(alumniMessage);
        }

        // GET: AlumniMessages/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["AlumniId"] = new SelectList(_context.Alumni, "AlumniId", "FirstName");
            ViewData["MessageId"] = new SelectList(_context.Messages, "MessageId", "MessageBody");
            return View();
        }

        // POST: AlumniMessages/Create
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

        // GET: AlumniMessages/Edit/5
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

        // POST: AlumniMessages/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
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

        // GET: AlumniMessages/Delete/5
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

        // POST: AlumniMessages/Delete/5
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
