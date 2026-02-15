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
    public class AlumniOrganizationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AlumniOrganizationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: AlumniOrganizations
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.AlumniOrganizations.Include(a => a.Alumni).Include(a => a.OrganizationType);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: AlumniOrganizations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alumniOrganization = await _context.AlumniOrganizations
                .Include(a => a.Alumni)
                .Include(a => a.OrganizationType)
                .FirstOrDefaultAsync(m => m.AlumniOrganizationId == id);
            if (alumniOrganization == null)
            {
                return NotFound();
            }

            return View(alumniOrganization);
        }

        // GET: AlumniOrganizations/Create
        public IActionResult Create()
        {
            ViewData["AlumniId"] = new SelectList(_context.Alumni, "AlumniId", "FirstName");
            ViewData["OrganizationTypeId"] = new SelectList(_context.OrganizationTypes, "OrganizationTypeId", "OrganizationName");
            return View();
        }

        // POST: AlumniOrganizations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AlumniOrganizationId,AlumniId,OrganizationTypeId,OfficerRoles")] AlumniOrganization alumniOrganization)
        {
            if (ModelState.IsValid)
            {
                _context.Add(alumniOrganization);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AlumniId"] = new SelectList(_context.Alumni, "AlumniId", "FirstName", alumniOrganization.AlumniId);
            ViewData["OrganizationTypeId"] = new SelectList(_context.OrganizationTypes, "OrganizationTypeId", "OrganizationName", alumniOrganization.OrganizationTypeId);
            return View(alumniOrganization);
        }

        // GET: AlumniOrganizations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alumniOrganization = await _context.AlumniOrganizations.FindAsync(id);
            if (alumniOrganization == null)
            {
                return NotFound();
            }
            ViewData["AlumniId"] = new SelectList(_context.Alumni, "AlumniId", "FirstName", alumniOrganization.AlumniId);
            ViewData["OrganizationTypeId"] = new SelectList(_context.OrganizationTypes, "OrganizationTypeId", "OrganizationName", alumniOrganization.OrganizationTypeId);
            return View(alumniOrganization);
        }

        // POST: AlumniOrganizations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AlumniOrganizationId,AlumniId,OrganizationTypeId,OfficerRoles")] AlumniOrganization alumniOrganization)
        {
            if (id != alumniOrganization.AlumniOrganizationId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(alumniOrganization);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AlumniOrganizationExists(alumniOrganization.AlumniOrganizationId))
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
            ViewData["AlumniId"] = new SelectList(_context.Alumni, "AlumniId", "FirstName", alumniOrganization.AlumniId);
            ViewData["OrganizationTypeId"] = new SelectList(_context.OrganizationTypes, "OrganizationTypeId", "OrganizationName", alumniOrganization.OrganizationTypeId);
            return View(alumniOrganization);
        }

        // GET: AlumniOrganizations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alumniOrganization = await _context.AlumniOrganizations
                .Include(a => a.Alumni)
                .Include(a => a.OrganizationType)
                .FirstOrDefaultAsync(m => m.AlumniOrganizationId == id);
            if (alumniOrganization == null)
            {
                return NotFound();
            }

            return View(alumniOrganization);
        }

        // POST: AlumniOrganizations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var alumniOrganization = await _context.AlumniOrganizations.FindAsync(id);
            if (alumniOrganization != null)
            {
                _context.AlumniOrganizations.Remove(alumniOrganization);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AlumniOrganizationExists(int id)
        {
            return _context.AlumniOrganizations.Any(e => e.AlumniOrganizationId == id);
        }
    }
}
