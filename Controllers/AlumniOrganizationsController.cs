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
    // "!roles.Contains(AdminRole)" below = acting as an alumnus: only Alumni
    // and Admin get in, so anyone without Admin manages just their own
    // records. An Admin who is also an alumnus gets the (scoped) admin view.
    [Authorize(Roles = "Alumni,Admin")]
    public class AlumniOrganizationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public AlumniOrganizationsController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: AlumniOrganizations
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);

            IQueryable<AlumniOrganization> query = _context.AlumniOrganizations.Include(a => a.Alumni).Include(a => a.Organization);

            // Alumni can only see their own organization records
            if (!roles.Contains(Constants.AdminRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                if (alumni != null)
                {
                    query = query.Where(ao => ao.AlumniId == alumni.AlumniId);
                }
                else
                {
                    return View(new List<AlumniOrganization>());
                }
            }
            else
            {
                // Admin sees organizations of alumni inside their access scope
                var visibleIds = (await Services.AccessScopeService.GetVisibleAlumniAsync(_context, currentUser, roles)).Select(a => a.AlumniId);
                query = query.Where(ao => visibleIds.Contains(ao.AlumniId));
            }

            return View(await query.ToListAsync());
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
                .Include(a => a.Organization)
                .FirstOrDefaultAsync(m => m.AlumniOrganizationId == id);
            if (alumniOrganization == null)
            {
                return NotFound();
            }

            // Check if Alumni user is trying to view another alumni's organization
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);
            if (!roles.Contains(Constants.AdminRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                if (alumni == null || alumniOrganization.AlumniId != alumni.AlumniId)
                {
                    TempData["ErrorMessage"] = "You can only view your own organization records.";
                    return RedirectToAction(nameof(Index));
                }
            }
            else if (!await Services.AccessScopeService.AreAlumniVisibleAsync(_context, currentUser, roles, alumniOrganization.AlumniId))
            {
                return OutsideScope();
            }

            return View(alumniOrganization);
        }

        // GET: AlumniOrganizations/Create
        public async Task<IActionResult> Create()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);

            // For Alumni users, auto-select their own AlumniId
            if (!roles.Contains(Constants.AdminRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                if (alumni == null)
                {
                    TempData["ErrorMessage"] = "Alumni profile not found.";
                    return RedirectToAction("Index", "Home");
                }
                ViewData["CurrentAlumniId"] = alumni.AlumniId;
                ViewData["AlumniId"] = Services.AlumniSelectList.Build(new[] { alumni }, alumni.AlumniId);
                ViewData["UserRole"] = Constants.AlumniRole;
            }
            else
            {
                // Admin can select any alumni
                ViewData["AlumniId"] = Services.AlumniSelectList.Build(await Services.AccessScopeService.GetVisibleAlumniAsync(_context, currentUser, roles));
                ViewData["UserRole"] = "Admin";
            }

            ViewData["OrganizationId"] = new SelectList(_context.StudentOrganizations, "OrganizationId", "OrganizationName");
            return View();
        }

        // POST: AlumniOrganizations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AlumniOrganizationId,AlumniId,OrganizationId,OfficerRoles")] AlumniOrganization alumniOrganization)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);

            // Validate: Alumni can only create organizations for themselves
            if (!roles.Contains(Constants.AdminRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                if (alumni == null || alumniOrganization.AlumniId != alumni.AlumniId)
                {
                    TempData["ErrorMessage"] = "You can only create organization records for yourself.";
                    return RedirectToAction(nameof(Index));
                }
            }
            else if (!await Services.AccessScopeService.AreAlumniVisibleAsync(_context, currentUser, roles, alumniOrganization.AlumniId))
            {
                return OutsideScope();
            }

            if (ModelState.IsValid)
            {
                _context.Add(alumniOrganization);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Organization record added successfully!";
                return RedirectToAction(nameof(Index));
            }

            // Repopulate dropdowns
            if (!roles.Contains(Constants.AdminRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                ViewData["CurrentAlumniId"] = alumni?.AlumniId;
                ViewData["AlumniId"] = Services.AlumniSelectList.Build(new[] { alumni }, alumniOrganization.AlumniId);
            }
            else
            {
                ViewData["AlumniId"] = Services.AlumniSelectList.Build(await Services.AccessScopeService.GetVisibleAlumniAsync(_context, currentUser, roles), alumniOrganization.AlumniId);
            }
            ViewData["OrganizationId"] = new SelectList(_context.StudentOrganizations, "OrganizationId", "OrganizationName", alumniOrganization.OrganizationId);
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

            // Check if Alumni user is trying to edit another alumni's organization
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);
            if (!roles.Contains(Constants.AdminRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                if (alumni == null || alumniOrganization.AlumniId != alumni.AlumniId)
                {
                    TempData["ErrorMessage"] = "You can only edit your own organization records.";
                    return RedirectToAction(nameof(Index));
                }
                ViewData["CurrentAlumniId"] = alumni.AlumniId;
                ViewData["AlumniId"] = Services.AlumniSelectList.Build(new[] { alumni }, alumniOrganization.AlumniId);
            }
            else if (!await Services.AccessScopeService.AreAlumniVisibleAsync(_context, currentUser, roles, alumniOrganization.AlumniId))
            {
                return OutsideScope();
            }
            else
            {
                ViewData["AlumniId"] = Services.AlumniSelectList.Build(await Services.AccessScopeService.GetVisibleAlumniAsync(_context, currentUser, roles), alumniOrganization.AlumniId);
            }

            ViewData["OrganizationId"] = new SelectList(_context.StudentOrganizations, "OrganizationId", "OrganizationName", alumniOrganization.OrganizationId);
            return View(alumniOrganization);
        }

        // POST: AlumniOrganizations/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AlumniOrganizationId,AlumniId,OrganizationId,OfficerRoles")] AlumniOrganization alumniOrganization)
        {
            if (id != alumniOrganization.AlumniOrganizationId)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);

            // The owner as stored - not just the AlumniId posted in the form,
            // which could be changed to claim someone else's record.
            var storedAlumniId = await _context.AlumniOrganizations
                .Where(x => x.AlumniOrganizationId == id)
                .Select(x => (int?)x.AlumniId)
                .FirstOrDefaultAsync();
            if (storedAlumniId == null)
            {
                return NotFound();
            }

            // Validate: Alumni can only edit their own organizations
            if (!roles.Contains(Constants.AdminRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                if (alumni == null || alumniOrganization.AlumniId != alumni.AlumniId || storedAlumniId != alumni.AlumniId)
                {
                    TempData["ErrorMessage"] = "You can only edit your own organization records.";
                    return RedirectToAction(nameof(Index));
                }
            }
            else if (!await Services.AccessScopeService.AreAlumniVisibleAsync(_context, currentUser, roles, storedAlumniId.Value, alumniOrganization.AlumniId))
            {
                return OutsideScope();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(alumniOrganization);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Organization record updated successfully!";
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

            // Repopulate dropdowns
            if (!roles.Contains(Constants.AdminRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                ViewData["CurrentAlumniId"] = alumni?.AlumniId;
                ViewData["AlumniId"] = Services.AlumniSelectList.Build(new[] { alumni }, alumniOrganization.AlumniId);
            }
            else
            {
                ViewData["AlumniId"] = Services.AlumniSelectList.Build(await Services.AccessScopeService.GetVisibleAlumniAsync(_context, currentUser, roles), alumniOrganization.AlumniId);
            }
            ViewData["OrganizationId"] = new SelectList(_context.StudentOrganizations, "OrganizationId", "OrganizationName", alumniOrganization.OrganizationId);
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
                .Include(a => a.Organization)
                .FirstOrDefaultAsync(m => m.AlumniOrganizationId == id);
            if (alumniOrganization == null)
            {
                return NotFound();
            }

            // Check if Alumni user is trying to delete another alumni's organization
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);
            if (!roles.Contains(Constants.AdminRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                if (alumni == null || alumniOrganization.AlumniId != alumni.AlumniId)
                {
                    TempData["ErrorMessage"] = "You can only delete your own organization records.";
                    return RedirectToAction(nameof(Index));
                }
            }
            else if (!await Services.AccessScopeService.AreAlumniVisibleAsync(_context, currentUser, roles, alumniOrganization.AlumniId))
            {
                return OutsideScope();
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
                // Check if Alumni user is trying to delete another alumni's organization
                var currentUser = await _userManager.GetUserAsync(User);
                var roles = await _userManager.GetRolesAsync(currentUser);
                if (!roles.Contains(Constants.AdminRole))
                {
                    var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                    if (alumni == null || alumniOrganization.AlumniId != alumni.AlumniId)
                    {
                        TempData["ErrorMessage"] = "You can only delete your own organization records.";
                        return RedirectToAction(nameof(Index));
                    }
                }
                else if (!await Services.AccessScopeService.AreAlumniVisibleAsync(_context, currentUser, roles, alumniOrganization.AlumniId))
                {
                    return OutsideScope();
                }

                _context.AlumniOrganizations.Remove(alumniOrganization);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Organization record deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private IActionResult OutsideScope()
        {
            TempData["ErrorMessage"] = "That alumnus is outside your access scope.";
            return RedirectToAction(nameof(Index));
        }

        private bool AlumniOrganizationExists(int id)
        {
            return _context.AlumniOrganizations.Any(e => e.AlumniOrganizationId == id);
        }
    }
}
