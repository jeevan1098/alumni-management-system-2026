using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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

        public AlumniMessagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: AlumniMessages
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.AlumniMessages.Include(a => a.Alumni).Include(a => a.Message);
            return View(await applicationDbContext.ToListAsync());
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

            return View(alumniMessage);
        }

        // GET: AlumniMessages/Create
        public IActionResult Create()
        {
            ViewData["AlumniId"] = new SelectList(_context.Alumni, "AlumniId", "FirstName");
            ViewData["MessageId"] = new SelectList(_context.Messages, "MessageId", "MessageBody");
            return View();
        }

        // POST: AlumniMessages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AlumniMessageId,AlumniId,MessageId,SentAt")] AlumniMessage alumniMessage)
        {
            if (ModelState.IsValid)
            {
                _context.Add(alumniMessage);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AlumniId"] = new SelectList(_context.Alumni, "AlumniId", "FirstName", alumniMessage.AlumniId);
            ViewData["MessageId"] = new SelectList(_context.Messages, "MessageId", "MessageBody", alumniMessage.MessageId);
            return View(alumniMessage);
        }

        // GET: AlumniMessages/Edit/5
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
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var alumniMessage = await _context.AlumniMessages.FindAsync(id);
            if (alumniMessage != null)
            {
                _context.AlumniMessages.Remove(alumniMessage);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AlumniMessageExists(int id)
        {
            return _context.AlumniMessages.Any(e => e.AlumniMessageId == id);
        }
    }
}
