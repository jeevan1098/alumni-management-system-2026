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
    public class OrganizationTypesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrganizationTypesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
            => View(await _context.OrganizationTypes.ToListAsync());

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var organizationType = await _context.OrganizationTypes.FirstOrDefaultAsync(m => m.OrganizationTypeId == id);
            if (organizationType == null) return NotFound();
            return View(organizationType);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("OrganizationTypeId,OrganizationName")] OrganizationType organizationType)
        {
            organizationType.OrganizationName = ToTitleCase(organizationType.OrganizationName);

            bool isDuplicate = await _context.OrganizationTypes.AnyAsync(o =>
                o.OrganizationName.ToLower() == organizationType.OrganizationName.ToLower());

            if (isDuplicate)
                ModelState.AddModelError(string.Empty,
                    "An organization type with the same name already exists.");

            if (ModelState.IsValid)
            {
                _context.Add(organizationType);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Organization type created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(organizationType);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var organizationType = await _context.OrganizationTypes.FindAsync(id);
            if (organizationType == null) return NotFound();
            return View(organizationType);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrganizationTypeId,OrganizationName")] OrganizationType organizationType)
        {
            if (id != organizationType.OrganizationTypeId) return NotFound();

            organizationType.OrganizationName = ToTitleCase(organizationType.OrganizationName);

            bool isDuplicate = await _context.OrganizationTypes.AnyAsync(o =>
                o.OrganizationTypeId != organizationType.OrganizationTypeId &&
                o.OrganizationName.ToLower() == organizationType.OrganizationName.ToLower());

            if (isDuplicate)
                ModelState.AddModelError(string.Empty,
                    "An organization type with the same name already exists.");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(organizationType);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Organization type updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrganizationTypeExists(organizationType.OrganizationTypeId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(organizationType);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var organizationType = await _context.OrganizationTypes.FirstOrDefaultAsync(m => m.OrganizationTypeId == id);
            if (organizationType == null) return NotFound();

            // Check if any alumni are assigned to this organization type
            var alumniCount = await _context.AlumniOrganizations.CountAsync(ao => ao.OrganizationTypeId == id);
            if (alumniCount > 0)
            {
                TempData["ErrorMessage"] = $"Cannot delete this organization type — {alumniCount} alumni are assigned to it. Remove their organization records first.";
                return RedirectToAction(nameof(Index));
            }

            return View(organizationType);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Double-check before deleting
            var alumniCount = await _context.AlumniOrganizations.CountAsync(ao => ao.OrganizationTypeId == id);
            if (alumniCount > 0)
            {
                TempData["ErrorMessage"] = $"Cannot delete this organization type — {alumniCount} alumni are assigned to it.";
                return RedirectToAction(nameof(Index));
            }

            var organizationType = await _context.OrganizationTypes.FindAsync(id);
            if (organizationType != null)
            {
                _context.OrganizationTypes.Remove(organizationType);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Organization type deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool OrganizationTypeExists(int id)
            => _context.OrganizationTypes.Any(e => e.OrganizationTypeId == id);

        private static string ToTitleCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return value;
            return System.Globalization.CultureInfo.CurrentCulture
                         .TextInfo.ToTitleCase(value.Trim().ToLower());
        }
    }
}