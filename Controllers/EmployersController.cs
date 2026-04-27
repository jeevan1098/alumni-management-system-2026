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
            NormalizeEmployer(employer);

            bool isDuplicate = await _context.Employers.AnyAsync(e =>
                e.EmployerName.ToLower() == employer.EmployerName.ToLower() &&
                (e.Location ?? "").ToLower() == (employer.Location ?? "").ToLower() &&
                (e.Industry ?? "").ToLower() == (employer.Industry ?? "").ToLower());

            if (isDuplicate)
                ModelState.AddModelError(string.Empty,
                    "An employer with the same Name, Location, and Industry already exists.");

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

            NormalizeEmployer(employer);

            bool isDuplicate = await _context.Employers.AnyAsync(e =>
                e.EmployerId != employer.EmployerId &&
                e.EmployerName.ToLower() == employer.EmployerName.ToLower() &&
                (e.Location ?? "").ToLower() == (employer.Location ?? "").ToLower() &&
                (e.Industry ?? "").ToLower() == (employer.Industry ?? "").ToLower());

            if (isDuplicate)
                ModelState.AddModelError(string.Empty,
                    "An employer with the same Name, Location, and Industry already exists.");

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

        /// <summary>
        /// Normalizes Employer fields to Title Case to prevent case-variant duplicates.
        /// </summary>
        private static void NormalizeEmployer(Employer employer)
        {
            employer.EmployerName = ToTitleCase(employer.EmployerName);
            employer.Location = ToTitleCase(employer.Location);
            employer.Industry = ToTitleCase(employer.Industry);
        }

        private static string ToTitleCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return value;
            return System.Globalization.CultureInfo.CurrentCulture
                         .TextInfo.ToTitleCase(value.Trim().ToLower());
        }
    }
}