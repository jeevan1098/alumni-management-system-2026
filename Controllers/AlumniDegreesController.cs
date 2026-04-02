using Alumni_Management_System.Data;
using Alumni_Management_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Alumni_Management_System.Controllers
{
    [Authorize(Roles = "Alumni,Admin,Staff")]
    public class AlumniDegreesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public AlumniDegreesController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);

            IQueryable<AlumniDegree> query = _context.AlumniDegrees.Include(a => a.Alumni).Include(a => a.Degree);

            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                if (alumni != null)
                {
                    query = query.Where(ad => ad.AlumniId == alumni.AlumniId);
                }
                else
                {
                    return View(new List<AlumniDegree>());
                }
            }

            return View(await query.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alumniDegree = await _context.AlumniDegrees
                .Include(a => a.Alumni)
                .Include(a => a.Degree)
                .FirstOrDefaultAsync(m => m.AlumniDegreeId == id);
            if (alumniDegree == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);
            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                if (alumni == null || alumniDegree.AlumniId != alumni.AlumniId)
                {
                    TempData["ErrorMessage"] = "You can only view your own degrees.";
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(alumniDegree);
        }

        public async Task<IActionResult> Create()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);

            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                if (alumni == null)
                {
                    TempData["ErrorMessage"] = "Alumni profile not found.";
                    return RedirectToAction("Index", "Home");
                }
                ViewData["CurrentAlumniId"] = alumni.AlumniId;
                ViewData["AlumniId"] = new SelectList(new[] { alumni }, "AlumniId", "FirstName", alumni.AlumniId);
                ViewData["UserRole"] = Constants.AlumniRole;
            }
            else
            {

                ViewData["AlumniId"] = new SelectList(_context.Alumni, "AlumniId", "FirstName");
                ViewData["UserRole"] = "Admin";
            }

            ViewData["DegreeId"] = new SelectList( _context.DegreePrograms .Select(d => new
            {
                d.DegreeId,
                DisplayText = d.DegreeType + ", " +
                              d.Institution + ", " +
                              d.MajorFieldOfStudy + ", " +
                              d.Department
            }),
                "DegreeId",
                "DisplayText");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AlumniDegreeId,AlumniId,DegreeId,DateConferred,YearsToCompleteDegree,Gpa,EmploymentWhileStudying,DegreeSpecificJob,ParticipatedInResearch,JobSecuredUponGraduation,AttendedOrPlansGradSchool")] AlumniDegree alumniDegree)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);

            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                if (alumni == null || alumniDegree.AlumniId != alumni.AlumniId)
                {
                    TempData["ErrorMessage"] = "You can only create degrees for yourself.";
                    return RedirectToAction(nameof(Index));
                }
            }

            if (alumniDegree.DateConferred > DateOnly.FromDateTime(DateTime.Now))
            {
                ModelState.AddModelError("DateConferred", "Date Conferred cannot be in the future.");
            }

            if (alumniDegree.YearsToCompleteDegree.HasValue)
            {
                if (alumniDegree.YearsToCompleteDegree < 0)
                {
                    ModelState.AddModelError("YearsToCompleteDegree", "Years to complete degree cannot be negative.");
                }
                else if (alumniDegree.YearsToCompleteDegree > 10)
                {
                    ModelState.AddModelError("YearsToCompleteDegree", "Years to complete degree cannot exceed 10 years. Please contact administrator.");
                }
            }

            if (alumniDegree.Gpa.HasValue)
            {
                if (alumniDegree.Gpa < 0 || alumniDegree.Gpa > 4.00m)
                {
                    ModelState.AddModelError("Gpa", "GPA must be between 0.00 and 4.00.");
                }
            }

            var duplicateExists = await _context.AlumniDegrees
                .AnyAsync(ad => ad.AlumniId == alumniDegree.AlumniId &&
                               ad.DegreeId == alumniDegree.DegreeId);
            if (duplicateExists)
            {
                ModelState.AddModelError("DegreeId", "You already have this degree recorded.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(alumniDegree);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Degree added successfully!";
                return RedirectToAction(nameof(Index));
            }

            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                ViewData["CurrentAlumniId"] = alumni?.AlumniId;
                ViewData["AlumniId"] = new SelectList(new[] { alumni }, "AlumniId", "FirstName", alumniDegree.AlumniId);
            }
            else
            {
                ViewData["AlumniId"] = new SelectList(_context.Alumni, "AlumniId", "FirstName", alumniDegree.AlumniId);
            }
            ViewData["DegreeId"] = new SelectList(_context.DegreePrograms, "DegreeId", "DegreeType", alumniDegree.DegreeId);
            return View(alumniDegree);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alumniDegree = await _context.AlumniDegrees.FindAsync(id);
            if (alumniDegree == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);
            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                if (alumni == null || alumniDegree.AlumniId != alumni.AlumniId)
                {
                    TempData["ErrorMessage"] = "You can only edit your own degrees.";
                    return RedirectToAction(nameof(Index));
                }
                ViewData["CurrentAlumniId"] = alumni.AlumniId;
                ViewData["AlumniId"] = new SelectList(new[] { alumni }, "AlumniId", "FirstName", alumniDegree.AlumniId);
            }
            else
            {
                ViewData["AlumniId"] = new SelectList(_context.Alumni, "AlumniId", "FirstName", alumniDegree.AlumniId);
            }

            ViewData["DegreeId"] = new SelectList(_context.DegreePrograms, "DegreeId", "DegreeType", alumniDegree.DegreeId);
            return View(alumniDegree);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AlumniDegreeId,AlumniId,DegreeId,DateConferred,YearsToCompleteDegree,Gpa,EmploymentWhileStudying,DegreeSpecificJob,ParticipatedInResearch,JobSecuredUponGraduation,AttendedOrPlansGradSchool")] AlumniDegree alumniDegree)
        {
            if (id != alumniDegree.AlumniDegreeId)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);

            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                if (alumni == null || alumniDegree.AlumniId != alumni.AlumniId)
                {
                    TempData["ErrorMessage"] = "You can only edit your own degrees.";
                    return RedirectToAction(nameof(Index));
                }
            }

            if (alumniDegree.DateConferred > DateOnly.FromDateTime(DateTime.Now))
            {
                ModelState.AddModelError("DateConferred", "Date Conferred cannot be in the future.");
            }

            if (alumniDegree.YearsToCompleteDegree.HasValue)
            {
                if (alumniDegree.YearsToCompleteDegree < 0)
                {
                    ModelState.AddModelError("YearsToCompleteDegree", "Years to complete degree cannot be negative.");
                }
                else if (alumniDegree.YearsToCompleteDegree > 10)
                {
                    ModelState.AddModelError("YearsToCompleteDegree", "Years to complete degree cannot exceed 10 years. Please contact administrator.");
                }
            }

            if (alumniDegree.Gpa.HasValue)
            {
                if (alumniDegree.Gpa < 0 || alumniDegree.Gpa > 4.00m)
                {
                    ModelState.AddModelError("Gpa", "GPA must be between 0.00 and 4.00.");
                }
            }

            var duplicateExists = await _context.AlumniDegrees
                .AnyAsync(ad => ad.AlumniId == alumniDegree.AlumniId &&
                               ad.DegreeId == alumniDegree.DegreeId &&
                               ad.AlumniDegreeId != alumniDegree.AlumniDegreeId);
            if (duplicateExists)
            {
                ModelState.AddModelError("DegreeId", "You already have this degree recorded.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(alumniDegree);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Degree updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AlumniDegreeExists(alumniDegree.AlumniDegreeId))
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

            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                ViewData["CurrentAlumniId"] = alumni?.AlumniId;
                ViewData["AlumniId"] = new SelectList(new[] { alumni }, "AlumniId", "FirstName", alumniDegree.AlumniId);
            }
            else
            {
                ViewData["AlumniId"] = new SelectList(_context.Alumni, "AlumniId", "FirstName", alumniDegree.AlumniId);
            }
            ViewData["DegreeId"] = new SelectList(_context.DegreePrograms, "DegreeId", "DegreeType", alumniDegree.DegreeId);
            return View(alumniDegree);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alumniDegree = await _context.AlumniDegrees
                .Include(a => a.Alumni)
                .Include(a => a.Degree)
                .FirstOrDefaultAsync(m => m.AlumniDegreeId == id);
            if (alumniDegree == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);
            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                if (alumni == null || alumniDegree.AlumniId != alumni.AlumniId)
                {
                    TempData["ErrorMessage"] = "You can only delete your own degrees.";
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(alumniDegree);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var alumniDegree = await _context.AlumniDegrees.FindAsync(id);
            if (alumniDegree != null)
            {

                var currentUser = await _userManager.GetUserAsync(User);
                var roles = await _userManager.GetRolesAsync(currentUser);
                if (roles.Contains(Constants.AlumniRole))
                {
                    var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                    if (alumni == null || alumniDegree.AlumniId != alumni.AlumniId)
                    {
                        TempData["ErrorMessage"] = "You can only delete your own degrees.";
                        return RedirectToAction(nameof(Index));
                    }
                }

                _context.AlumniDegrees.Remove(alumniDegree);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Degree deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool AlumniDegreeExists(int id)
        {
            return _context.AlumniDegrees.Any(e => e.AlumniDegreeId == id);
        }
    }
}
