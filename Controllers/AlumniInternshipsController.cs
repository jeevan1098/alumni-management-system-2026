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
    public class AlumniInternshipsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public AlumniInternshipsController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);
            IQueryable<AlumniInternship> query = _context.AlumniInternships
                .Include(a => a.Alumni).Include(a => a.Employer);
            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                if (alumni != null) query = query.Where(ai => ai.AlumniId == alumni.AlumniId);
                else return View(new List<AlumniInternship>());
            }
            return View(await query.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var alumniInternship = await _context.AlumniInternships
                .Include(a => a.Alumni).Include(a => a.Employer)
                .FirstOrDefaultAsync(m => m.AlumniInternshipId == id);
            if (alumniInternship == null) return NotFound();
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);
            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                if (alumni == null || alumniInternship.AlumniId != alumni.AlumniId)
                {
                    TempData["ErrorMessage"] = "You can only view your own internship records.";
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(alumniInternship);
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

        private async Task PopulateEmployerDropdown(object selectedId = null)
        {
            var employers = await _context.Employers.OrderBy(e => e.EmployerName).ToListAsync();
            var list = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "-- Select Employer --" },
                new SelectListItem { Value = "0", Text = "Other (Add New)" }
            };
            list.AddRange(employers.Select(e => new SelectListItem
            {
                Value = e.EmployerId.ToString(),
                Text = e.EmployerName,
                Selected = selectedId != null && e.EmployerId.ToString() == selectedId.ToString()
            }));
            ViewData["EmployerId"] = list;
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
            else
            {
                await PopulateAlumniDropdown();
                ViewData["UserRole"] = "Admin";
            }
            await PopulateEmployerDropdown();
            return View();
        }

        [Authorize(Roles = "Admin, Alumni")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AlumniInternshipId,AlumniId,EmployerId,InternshipType,Title,StartDate,EndDate")] AlumniInternship alumniInternship, string OtherEmployerName)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);
            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                if (alumni == null || alumniInternship.AlumniId != alumni.AlumniId)
                { TempData["ErrorMessage"] = "You can only create internship records for yourself."; return RedirectToAction(nameof(Index)); }
            }
            if (alumniInternship.EmployerId == 0 && !string.IsNullOrWhiteSpace(OtherEmployerName))
            {
                var existing = await _context.Employers.FirstOrDefaultAsync(e => e.EmployerName.ToLower() == OtherEmployerName.Trim().ToLower());
                if (existing != null) alumniInternship.EmployerId = existing.EmployerId;
                else { var n = new Employer { EmployerName = OtherEmployerName.Trim() }; _context.Employers.Add(n); await _context.SaveChangesAsync(); alumniInternship.EmployerId = n.EmployerId; }
            }
            if (alumniInternship.StartDate > DateOnly.FromDateTime(DateTime.Now))
                ModelState.AddModelError("StartDate", "Start Date cannot be in the future.");
            if (alumniInternship.EndDate < alumniInternship.StartDate)
                ModelState.AddModelError("EndDate", "End Date cannot be before Start Date.");
            if (ModelState.IsValid)
            {
                _context.Add(alumniInternship);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Internship record added successfully!";
                return RedirectToAction(nameof(Index));
            }
            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                ViewData["CurrentAlumniId"] = alumni?.AlumniId;
                ViewData["AlumniId"] = new SelectList(new[] { new SelectListItem { Value = alumni?.AlumniId.ToString(), Text = $"{alumni?.JagId} — {alumni?.FirstName} {alumni?.LastName}" } }, "Value", "Text", alumniInternship.AlumniId);
                ViewData["UserRole"] = Constants.AlumniRole;
            }
            else { await PopulateAlumniDropdown(alumniInternship.AlumniId); ViewData["UserRole"] = "Admin"; }
            await PopulateEmployerDropdown(alumniInternship.EmployerId);
            return View(alumniInternship);
        }

        [Authorize(Roles = "Admin, Alumni")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var alumniInternship = await _context.AlumniInternships.FindAsync(id);
            if (alumniInternship == null) return NotFound();
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);
            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                if (alumni == null || alumniInternship.AlumniId != alumni.AlumniId)
                { TempData["ErrorMessage"] = "You can only edit your own internship records."; return RedirectToAction(nameof(Index)); }
                ViewData["CurrentAlumniId"] = alumni.AlumniId;
                ViewData["AlumniId"] = new SelectList(new[] { new SelectListItem { Value = alumni.AlumniId.ToString(), Text = $"{alumni.JagId} — {alumni.FirstName} {alumni.LastName}" } }, "Value", "Text", alumniInternship.AlumniId);
                ViewData["UserRole"] = Constants.AlumniRole;
            }
            else { await PopulateAlumniDropdown(alumniInternship.AlumniId); ViewData["UserRole"] = "Admin"; }
            await PopulateEmployerDropdown(alumniInternship.EmployerId);
            return View(alumniInternship);
        }

        [Authorize(Roles = "Admin, Alumni")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AlumniInternshipId,AlumniId,EmployerId,InternshipType,Title,StartDate,EndDate")] AlumniInternship alumniInternship, string OtherEmployerName)
        {
            if (id != alumniInternship.AlumniInternshipId) return NotFound();
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);
            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                if (alumni == null || alumniInternship.AlumniId != alumni.AlumniId)
                { TempData["ErrorMessage"] = "You can only edit your own internship records."; return RedirectToAction(nameof(Index)); }
            }
            if (alumniInternship.EmployerId == 0 && !string.IsNullOrWhiteSpace(OtherEmployerName))
            {
                var existing = await _context.Employers.FirstOrDefaultAsync(e => e.EmployerName.ToLower() == OtherEmployerName.Trim().ToLower());
                if (existing != null) alumniInternship.EmployerId = existing.EmployerId;
                else { var n = new Employer { EmployerName = OtherEmployerName.Trim() }; _context.Employers.Add(n); await _context.SaveChangesAsync(); alumniInternship.EmployerId = n.EmployerId; }
            }
            if (alumniInternship.StartDate > DateOnly.FromDateTime(DateTime.Now))
                ModelState.AddModelError("StartDate", "Start Date cannot be in the future.");
            if (alumniInternship.EndDate < alumniInternship.StartDate)
                ModelState.AddModelError("EndDate", "End Date cannot be before Start Date.");
            if (ModelState.IsValid)
            {
                try { _context.Update(alumniInternship); await _context.SaveChangesAsync(); TempData["SuccessMessage"] = "Internship record updated successfully!"; }
                catch (DbUpdateConcurrencyException) { if (!AlumniInternshipExists(alumniInternship.AlumniInternshipId)) return NotFound(); else throw; }
                return RedirectToAction(nameof(Index));
            }
            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                ViewData["CurrentAlumniId"] = alumni?.AlumniId;
                ViewData["AlumniId"] = new SelectList(new[] { new SelectListItem { Value = alumni?.AlumniId.ToString(), Text = $"{alumni?.JagId} — {alumni?.FirstName} {alumni?.LastName}" } }, "Value", "Text", alumniInternship.AlumniId);
                ViewData["UserRole"] = Constants.AlumniRole;
            }
            else { await PopulateAlumniDropdown(alumniInternship.AlumniId); ViewData["UserRole"] = "Admin"; }
            await PopulateEmployerDropdown(alumniInternship.EmployerId);
            return View(alumniInternship);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var alumniInternship = await _context.AlumniInternships.Include(a => a.Alumni).Include(a => a.Employer).FirstOrDefaultAsync(m => m.AlumniInternshipId == id);
            if (alumniInternship == null) return NotFound();
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);
            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                if (alumni == null || alumniInternship.AlumniId != alumni.AlumniId)
                { TempData["ErrorMessage"] = "You can only delete your own internship records."; return RedirectToAction(nameof(Index)); }
            }
            return View(alumniInternship);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var alumniInternship = await _context.AlumniInternships.FindAsync(id);
            if (alumniInternship != null)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var roles = await _userManager.GetRolesAsync(currentUser);
                if (roles.Contains(Constants.AlumniRole))
                {
                    var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                    if (alumni == null || alumniInternship.AlumniId != alumni.AlumniId)
                    { TempData["ErrorMessage"] = "You can only delete your own internship records."; return RedirectToAction(nameof(Index)); }
                }
                _context.AlumniInternships.Remove(alumniInternship);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Internship record deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool AlumniInternshipExists(int id) => _context.AlumniInternships.Any(e => e.AlumniInternshipId == id);
    }
}
