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
    [Authorize(Roles = "Alumni,Admin,Staff")]
    public class AlumniOrganizationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public AlumniOrganizationsController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);
            IQueryable<AlumniOrganization> query = _context.AlumniOrganizations
                .Include(a => a.Alumni).Include(a => a.OrganizationType);
            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                if (alumni != null) query = query.Where(ao => ao.AlumniId == alumni.AlumniId);
                else return View(new List<AlumniOrganization>());
            }
            return View(await query.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var alumniOrganization = await _context.AlumniOrganizations
                .Include(a => a.Alumni).Include(a => a.OrganizationType)
                .FirstOrDefaultAsync(m => m.AlumniOrganizationId == id);
            if (alumniOrganization == null) return NotFound();
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);
            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                if (alumni == null || alumniOrganization.AlumniId != alumni.AlumniId)
                { TempData["ErrorMessage"] = "You can only view your own organization records."; return RedirectToAction(nameof(Index)); }
            }
            return View(alumniOrganization);
        }

        private async Task PopulateAlumniDropdown(object selectedId = null)
        {
            var alumniList = await _context.Alumni.OrderBy(a => a.LastName).ToListAsync();
            ViewData["AlumniId"] = new SelectList(
                alumniList.Select(a => new SelectListItem
                {
                    Value = a.AlumniId.ToString(),
                    Text = $"{a.JagId} — {a.FirstName} {a.LastName}"
                }), "Value", "Text", selectedId?.ToString());
        }

        [Authorize(Roles = "Admin, Alumni")]
        public async Task<IActionResult> Create()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);
            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                if (alumni == null) { TempData["ErrorMessage"] = "Alumni profile not found."; return RedirectToAction("Index", "Home"); }
                ViewData["CurrentAlumniId"] = alumni.AlumniId;
                ViewData["AlumniId"] = new SelectList(new[] { new SelectListItem { Value = alumni.AlumniId.ToString(), Text = $"{alumni.JagId} — {alumni.FirstName} {alumni.LastName}" } }, "Value", "Text", alumni.AlumniId);
                ViewData["UserRole"] = Constants.AlumniRole;
            }
            else { await PopulateAlumniDropdown(); ViewData["UserRole"] = "Admin"; }
            ViewData["OrganizationTypeId"] = new SelectList(_context.OrganizationTypes, "OrganizationTypeId", "OrganizationName");
            return View();
        }

        [Authorize(Roles = "Admin, Alumni")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AlumniOrganizationId,AlumniId,OrganizationTypeId,OfficerRoles")] AlumniOrganization alumniOrganization)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);
            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                if (alumni == null || alumniOrganization.AlumniId != alumni.AlumniId)
                { TempData["ErrorMessage"] = "You can only create organization records for yourself."; return RedirectToAction(nameof(Index)); }
            }
            if (ModelState.IsValid)
            {
                _context.Add(alumniOrganization);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Organization record added successfully!";
                return RedirectToAction(nameof(Index));
            }
            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                ViewData["CurrentAlumniId"] = alumni?.AlumniId;
                ViewData["AlumniId"] = new SelectList(new[] { new SelectListItem { Value = alumni?.AlumniId.ToString(), Text = $"{alumni?.JagId} — {alumni?.FirstName} {alumni?.LastName}" } }, "Value", "Text", alumniOrganization.AlumniId);
                ViewData["UserRole"] = Constants.AlumniRole;
            }
            else { await PopulateAlumniDropdown(alumniOrganization.AlumniId); ViewData["UserRole"] = "Admin"; }
            ViewData["OrganizationTypeId"] = new SelectList(_context.OrganizationTypes, "OrganizationTypeId", "OrganizationName", alumniOrganization.OrganizationTypeId);
            return View(alumniOrganization);
        }

        [Authorize(Roles = "Admin, Alumni")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var alumniOrganization = await _context.AlumniOrganizations.FindAsync(id);
            if (alumniOrganization == null) return NotFound();
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);
            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                if (alumni == null || alumniOrganization.AlumniId != alumni.AlumniId)
                { TempData["ErrorMessage"] = "You can only edit your own organization records."; return RedirectToAction(nameof(Index)); }
                ViewData["CurrentAlumniId"] = alumni.AlumniId;
                ViewData["AlumniId"] = new SelectList(new[] { new SelectListItem { Value = alumni.AlumniId.ToString(), Text = $"{alumni.JagId} — {alumni.FirstName} {alumni.LastName}" } }, "Value", "Text", alumniOrganization.AlumniId);
                ViewData["UserRole"] = Constants.AlumniRole;
            }
            else { await PopulateAlumniDropdown(alumniOrganization.AlumniId); ViewData["UserRole"] = "Admin"; }
            ViewData["OrganizationTypeId"] = new SelectList(_context.OrganizationTypes, "OrganizationTypeId", "OrganizationName", alumniOrganization.OrganizationTypeId);
            return View(alumniOrganization);
        }

        [Authorize(Roles = "Admin, Alumni")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AlumniOrganizationId,AlumniId,OrganizationTypeId,OfficerRoles")] AlumniOrganization alumniOrganization)
        {
            if (id != alumniOrganization.AlumniOrganizationId) return NotFound();
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);
            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                if (alumni == null || alumniOrganization.AlumniId != alumni.AlumniId)
                { TempData["ErrorMessage"] = "You can only edit your own organization records."; return RedirectToAction(nameof(Index)); }
            }
            if (ModelState.IsValid)
            {
                try { _context.Update(alumniOrganization); await _context.SaveChangesAsync(); TempData["SuccessMessage"] = "Organization record updated successfully!"; }
                catch (DbUpdateConcurrencyException) { if (!AlumniOrganizationExists(alumniOrganization.AlumniOrganizationId)) return NotFound(); else throw; }
                return RedirectToAction(nameof(Index));
            }
            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                ViewData["CurrentAlumniId"] = alumni?.AlumniId;
                ViewData["AlumniId"] = new SelectList(new[] { new SelectListItem { Value = alumni?.AlumniId.ToString(), Text = $"{alumni?.JagId} — {alumni?.FirstName} {alumni?.LastName}" } }, "Value", "Text", alumniOrganization.AlumniId);
                ViewData["UserRole"] = Constants.AlumniRole;
            }
            else { await PopulateAlumniDropdown(alumniOrganization.AlumniId); ViewData["UserRole"] = "Admin"; }
            ViewData["OrganizationTypeId"] = new SelectList(_context.OrganizationTypes, "OrganizationTypeId", "OrganizationName", alumniOrganization.OrganizationTypeId);
            return View(alumniOrganization);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var alumniOrganization = await _context.AlumniOrganizations.Include(a => a.Alumni).Include(a => a.OrganizationType).FirstOrDefaultAsync(m => m.AlumniOrganizationId == id);
            if (alumniOrganization == null) return NotFound();
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);
            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                if (alumni == null || alumniOrganization.AlumniId != alumni.AlumniId)
                { TempData["ErrorMessage"] = "You can only delete your own organization records."; return RedirectToAction(nameof(Index)); }
            }
            return View(alumniOrganization);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var alumniOrganization = await _context.AlumniOrganizations.FindAsync(id);
            if (alumniOrganization != null)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var roles = await _userManager.GetRolesAsync(currentUser);
                if (roles.Contains(Constants.AlumniRole))
                {
                    var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                    if (alumni == null || alumniOrganization.AlumniId != alumni.AlumniId)
                    { TempData["ErrorMessage"] = "You can only delete your own organization records."; return RedirectToAction(nameof(Index)); }
                }
                _context.AlumniOrganizations.Remove(alumniOrganization);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Organization record deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool AlumniOrganizationExists(int id) => _context.AlumniOrganizations.Any(e => e.AlumniOrganizationId == id);
    }
}
