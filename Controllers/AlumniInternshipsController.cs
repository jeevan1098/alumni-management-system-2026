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
    [Authorize(Roles = "Alumni,Admin")]
    public class AlumniInternshipsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public AlumniInternshipsController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: AlumniInternships
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);

            IQueryable<AlumniInternship> query = _context.AlumniInternships.Include(a => a.Alumni).Include(a => a.Employer);

            // Alumni can only see their own internships
            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.UserId == currentUser.Id);
                if (alumni != null)
                {
                    query = query.Where(ai => ai.AlumniId == alumni.AlumniId);
                }
                else
                {
                    return View(new List<AlumniInternship>());
                }
            }
            // Admin can see all internships

            return View(await query.ToListAsync());
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

            // Check if Alumni user is trying to view another alumni's internship
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);
            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.UserId == currentUser.Id);
                if (alumni == null || alumniInternship.AlumniId != alumni.AlumniId)
                {
                    TempData["ErrorMessage"] = "You can only view your own internship records.";
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(alumniInternship);
        }

        // GET: AlumniInternships/Create
        public async Task<IActionResult> Create()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);

            // For Alumni users, auto-select their own AlumniId
            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.UserId == currentUser.Id);
                if (alumni == null)
                {
                    TempData["ErrorMessage"] = "Alumni profile not found.";
                    return RedirectToAction("Index", "Home");
                }
                ViewData["CurrentAlumniId"] = alumni.AlumniId;
                ViewData["AlumniId"] = new SelectList(new[] { alumni }, "AlumniId", "FirstName", alumni.AlumniId);
            }
            else
            {
                // Admin can select any alumni
                ViewData["AlumniId"] = new SelectList(_context.Alumni, "AlumniId", "FirstName");
            }

            ViewData["EmployerId"] = new SelectList(_context.Employers, "EmployerId", "EmployerName");
            return View();
        }

        // POST: AlumniInternships/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AlumniInternshipId,AlumniId,EmployerId,InternshipType,Title,StartDate,EndDate")] AlumniInternship alumniInternship)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);

            // Validate: Alumni can only create internships for themselves
            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.UserId == currentUser.Id);
                if (alumni == null || alumniInternship.AlumniId != alumni.AlumniId)
                {
                    TempData["ErrorMessage"] = "You can only create internship records for yourself.";
                    return RedirectToAction(nameof(Index));
                }
            }

            // Validation 1: StartDate cannot be in future
            if (alumniInternship.StartDate > DateOnly.FromDateTime(DateTime.Now))
            {
                ModelState.AddModelError("StartDate", "Start Date cannot be in the future.");
            }

            // Validation 2: EndDate cannot be before StartDate
            if (alumniInternship.EndDate < alumniInternship.StartDate)
            {
                ModelState.AddModelError("EndDate", "End Date cannot be before Start Date.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(alumniInternship);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Internship record added successfully!";
                return RedirectToAction(nameof(Index));
            }

            // Repopulate dropdowns
            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.UserId == currentUser.Id);
                ViewData["CurrentAlumniId"] = alumni?.AlumniId;
                ViewData["AlumniId"] = new SelectList(new[] { alumni }, "AlumniId", "FirstName", alumniInternship.AlumniId);
            }
            else
            {
                ViewData["AlumniId"] = new SelectList(_context.Alumni, "AlumniId", "FirstName", alumniInternship.AlumniId);
            }
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

            // Check if Alumni user is trying to edit another alumni's internship
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);
            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.UserId == currentUser.Id);
                if (alumni == null || alumniInternship.AlumniId != alumni.AlumniId)
                {
                    TempData["ErrorMessage"] = "You can only edit your own internship records.";
                    return RedirectToAction(nameof(Index));
                }
                ViewData["CurrentAlumniId"] = alumni.AlumniId;
                ViewData["AlumniId"] = new SelectList(new[] { alumni }, "AlumniId", "FirstName", alumniInternship.AlumniId);
            }
            else
            {
                ViewData["AlumniId"] = new SelectList(_context.Alumni, "AlumniId", "FirstName", alumniInternship.AlumniId);
            }

            ViewData["EmployerId"] = new SelectList(_context.Employers, "EmployerId", "EmployerName", alumniInternship.EmployerId);
            return View(alumniInternship);
        }

        // POST: AlumniInternships/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AlumniInternshipId,AlumniId,EmployerId,InternshipType,Title,StartDate,EndDate")] AlumniInternship alumniInternship)
        {
            if (id != alumniInternship.AlumniInternshipId)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);

            // Validate: Alumni can only edit their own internships
            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.UserId == currentUser.Id);
                if (alumni == null || alumniInternship.AlumniId != alumni.AlumniId)
                {
                    TempData["ErrorMessage"] = "You can only edit your own internship records.";
                    return RedirectToAction(nameof(Index));
                }
            }

            // Validation 1: StartDate cannot be in future
            if (alumniInternship.StartDate > DateOnly.FromDateTime(DateTime.Now))
            {
                ModelState.AddModelError("StartDate", "Start Date cannot be in the future.");
            }

            // Validation 2: EndDate cannot be before StartDate
            if (alumniInternship.EndDate < alumniInternship.StartDate)
            {
                ModelState.AddModelError("EndDate", "End Date cannot be before Start Date.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(alumniInternship);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Internship record updated successfully!";
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

            // Repopulate dropdowns
            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.UserId == currentUser.Id);
                ViewData["CurrentAlumniId"] = alumni?.AlumniId;
                ViewData["AlumniId"] = new SelectList(new[] { alumni }, "AlumniId", "FirstName", alumniInternship.AlumniId);
            }
            else
            {
                ViewData["AlumniId"] = new SelectList(_context.Alumni, "AlumniId", "FirstName", alumniInternship.AlumniId);
            }
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

            // Check if Alumni user is trying to delete another alumni's internship
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);
            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.UserId == currentUser.Id);
                if (alumni == null || alumniInternship.AlumniId != alumni.AlumniId)
                {
                    TempData["ErrorMessage"] = "You can only delete your own internship records.";
                    return RedirectToAction(nameof(Index));
                }
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
                // Check if Alumni user is trying to delete another alumni's internship
                var currentUser = await _userManager.GetUserAsync(User);
                var roles = await _userManager.GetRolesAsync(currentUser);
                if (roles.Contains(Constants.AlumniRole))
                {
                    var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.UserId == currentUser.Id);
                    if (alumni == null || alumniInternship.AlumniId != alumni.AlumniId)
                    {
                        TempData["ErrorMessage"] = "You can only delete your own internship records.";
                        return RedirectToAction(nameof(Index));
                    }
                }

                _context.AlumniInternships.Remove(alumniInternship);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Internship record deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool AlumniInternshipExists(int id)
        {
            return _context.AlumniInternships.Any(e => e.AlumniInternshipId == id);
        }
    }
}
