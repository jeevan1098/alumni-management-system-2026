using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Alumni_Management_System.Data;
using Alumni_Management_System.Models;

namespace Alumni_Management_System.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class EmployersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
            => View(await _context.Employers.ToListAsync());

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var employer = await _context.Employers.FirstOrDefaultAsync(m => m.EmployerId == id);
            if (employer == null) return NotFound();
            return View(employer);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("EmployerId,EmployerName,Location,Industry")] Employer employer)
        {
            if (ModelState.IsValid)
            {
                _context.Add(employer);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Employer created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(employer);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var employer = await _context.Employers.FindAsync(id);
            if (employer == null) return NotFound();
            return View(employer);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EmployerId,EmployerName,Location,Industry")] Employer employer)
        {
            if (id != employer.EmployerId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employer);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Employer updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployerExists(employer.EmployerId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(employer);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var employer = await _context.Employers.FirstOrDefaultAsync(m => m.EmployerId == id);
            if (employer == null) return NotFound();

            // Check if any alumni employments use this employer
            var employmentCount = await _context.AlumniEmployments.CountAsync(ae => ae.EmployerId == id);
            var internshipCount = await _context.AlumniInternships.CountAsync(ai => ai.EmployerId == id);
            var total = employmentCount + internshipCount;

            if (total > 0)
            {
                TempData["ErrorMessage"] = $"Cannot delete this employer — {employmentCount} employment record(s) and {internshipCount} internship record(s) are linked to it. Remove those records first.";
                return RedirectToAction(nameof(Index));
            }

            return View(employer);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employmentCount = await _context.AlumniEmployments.CountAsync(ae => ae.EmployerId == id);
            var internshipCount = await _context.AlumniInternships.CountAsync(ai => ai.EmployerId == id);
            var total = employmentCount + internshipCount;

            if (total > 0)
            {
                TempData["ErrorMessage"] = $"Cannot delete this employer — {total} record(s) are linked to it.";
                return RedirectToAction(nameof(Index));
            }

            var employer = await _context.Employers.FindAsync(id);
            if (employer != null)
            {
                _context.Employers.Remove(employer);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Employer deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool EmployerExists(int id)
            => _context.Employers.Any(e => e.EmployerId == id);
    }
}
