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
    public class AlumniInternshipsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AlumniInternshipsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: AlumniInternships
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.AlumniInternships.Include(a => a.Alumni).Include(a => a.Employer);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: AlumniInternships/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alumniInternship = await _context.AlumniInternships
                .Include(a => a.Alumni)
                .Include(a => a.Employer)
                .FirstOrDefaultAsync(m => m.AlumniInternshipId == id);
            if (alumniInternship == null)
            {
                return NotFound();
            }

            return View(alumniInternship);
        }

        // GET: AlumniInternships/Create
        public IActionResult Create()
        {
            ViewData["AlumniId"] = new SelectList(_context.Alumni, "AlumniId", "FirstName");
            ViewData["EmployerId"] = new SelectList(_context.Employers, "EmployerId", "EmployerName");
            return View();
        }

        // POST: AlumniInternships/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AlumniInternshipId,AlumniId,EmployerId,InternshipType,Title,StartDate,EndDate")] AlumniInternship alumniInternship)
        {
            if (ModelState.IsValid)
            {
                _context.Add(alumniInternship);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AlumniId"] = new SelectList(_context.Alumni, "AlumniId", "FirstName", alumniInternship.AlumniId);
            ViewData["EmployerId"] = new SelectList(_context.Employers, "EmployerId", "EmployerName", alumniInternship.EmployerId);
            return View(alumniInternship);
        }

        // GET: AlumniInternships/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alumniInternship = await _context.AlumniInternships.FindAsync(id);
            if (alumniInternship == null)
            {
                return NotFound();
            }
            ViewData["AlumniId"] = new SelectList(_context.Alumni, "AlumniId", "FirstName", alumniInternship.AlumniId);
            ViewData["EmployerId"] = new SelectList(_context.Employers, "EmployerId", "EmployerName", alumniInternship.EmployerId);
            return View(alumniInternship);
        }

        // POST: AlumniInternships/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AlumniInternshipId,AlumniId,EmployerId,InternshipType,Title,StartDate,EndDate")] AlumniInternship alumniInternship)
        {
            if (id != alumniInternship.AlumniInternshipId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(alumniInternship);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AlumniInternshipExists(alumniInternship.AlumniInternshipId))
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
            ViewData["AlumniId"] = new SelectList(_context.Alumni, "AlumniId", "FirstName", alumniInternship.AlumniId);
            ViewData["EmployerId"] = new SelectList(_context.Employers, "EmployerId", "EmployerName", alumniInternship.EmployerId);
            return View(alumniInternship);
        }

        // GET: AlumniInternships/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alumniInternship = await _context.AlumniInternships
                .Include(a => a.Alumni)
                .Include(a => a.Employer)
                .FirstOrDefaultAsync(m => m.AlumniInternshipId == id);
            if (alumniInternship == null)
            {
                return NotFound();
            }

            return View(alumniInternship);
        }

        // POST: AlumniInternships/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var alumniInternship = await _context.AlumniInternships.FindAsync(id);
            if (alumniInternship != null)
            {
                _context.AlumniInternships.Remove(alumniInternship);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AlumniInternshipExists(int id)
        {
            return _context.AlumniInternships.Any(e => e.AlumniInternshipId == id);
        }
    }
}
