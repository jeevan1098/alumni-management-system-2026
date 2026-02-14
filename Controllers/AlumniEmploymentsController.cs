using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Alumni_Management_System.Data;
using Alumni_Management_System.Models;

namespace Alumni_Management_System.Controllers
{
    public class AlumniEmploymentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AlumniEmploymentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: AlumniEmployments
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.AlumniEmployments.Include(a => a.Alumni).Include(a => a.Employer);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: AlumniEmployments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alumniEmployment = await _context.AlumniEmployments
                .Include(a => a.Alumni)
                .Include(a => a.Employer)
                .FirstOrDefaultAsync(m => m.AlumniEmploymentId == id);
            if (alumniEmployment == null)
            {
                return NotFound();
            }

            return View(alumniEmployment);
        }

        // GET: AlumniEmployments/Create
        public IActionResult Create()
        {
            ViewData["AlumniId"] = new SelectList(_context.Alumni, "AlumniId", "FirstName");
            ViewData["EmployerId"] = new SelectList(_context.Employers, "EmployerId", "EmployerName");
            return View();
        }

        // POST: AlumniEmployments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AlumniEmploymentId,AlumniId,EmployerId,JobTitle,StartDate,EndDate,SalaryRange")] AlumniEmployment alumniEmployment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(alumniEmployment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AlumniId"] = new SelectList(_context.Alumni, "AlumniId", "FirstName", alumniEmployment.AlumniId);
            ViewData["EmployerId"] = new SelectList(_context.Employers, "EmployerId", "EmployerName", alumniEmployment.EmployerId);
            return View(alumniEmployment);
        }

        // GET: AlumniEmployments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alumniEmployment = await _context.AlumniEmployments.FindAsync(id);
            if (alumniEmployment == null)
            {
                return NotFound();
            }
            ViewData["AlumniId"] = new SelectList(_context.Alumni, "AlumniId", "FirstName", alumniEmployment.AlumniId);
            ViewData["EmployerId"] = new SelectList(_context.Employers, "EmployerId", "EmployerName", alumniEmployment.EmployerId);
            return View(alumniEmployment);
        }

        // POST: AlumniEmployments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AlumniEmploymentId,AlumniId,EmployerId,JobTitle,StartDate,EndDate,SalaryRange")] AlumniEmployment alumniEmployment)
        {
            if (id != alumniEmployment.AlumniEmploymentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(alumniEmployment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AlumniEmploymentExists(alumniEmployment.AlumniEmploymentId))
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
            ViewData["AlumniId"] = new SelectList(_context.Alumni, "AlumniId", "FirstName", alumniEmployment.AlumniId);
            ViewData["EmployerId"] = new SelectList(_context.Employers, "EmployerId", "EmployerName", alumniEmployment.EmployerId);
            return View(alumniEmployment);
        }

        // GET: AlumniEmployments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alumniEmployment = await _context.AlumniEmployments
                .Include(a => a.Alumni)
                .Include(a => a.Employer)
                .FirstOrDefaultAsync(m => m.AlumniEmploymentId == id);
            if (alumniEmployment == null)
            {
                return NotFound();
            }

            return View(alumniEmployment);
        }

        // POST: AlumniEmployments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var alumniEmployment = await _context.AlumniEmployments.FindAsync(id);
            if (alumniEmployment != null)
            {
                _context.AlumniEmployments.Remove(alumniEmployment);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AlumniEmploymentExists(int id)
        {
            return _context.AlumniEmployments.Any(e => e.AlumniEmploymentId == id);
        }
    }
}
