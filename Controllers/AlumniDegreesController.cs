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

            IQueryable<AlumniDegree> query = _context.AlumniDegrees
                .Include(a => a.Alumni)
                .Include(a => a.Degree);

            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                if (alumni != null)
                    query = query.Where(ad => ad.AlumniId == alumni.AlumniId);
                else
                    return View(new List<AlumniDegree>());
            }

            return View(await query.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var alumniDegree = await _context.AlumniDegrees
                .Include(a => a.Alumni)
                .Include(a => a.Degree)
                .FirstOrDefaultAsync(m => m.AlumniDegreeId == id);
            if (alumniDegree == null) return NotFound();

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

        private async Task PopulateAlumniDropdown(object selectedId = null)
        {
            var alumniList = await _context.Alumni
                .OrderBy(a => a.LastName)
                .ToListAsync();

            ViewData["AlumniId"] = new SelectList(
                alumniList.Select(a => new SelectListItem
                {
                    Value = a.AlumniId.ToString(),
                    Text = $"{a.JagId} — {a.FirstName} {a.LastName}"
                }),
                "Value", "Text", selectedId?.ToString());
        }

        private async Task PopulateDegreeDropdown(object selectedId = null, bool skipAlumniFilter = false)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);

            IQueryable<DegreeProgram> degreeQuery = _context.DegreePrograms;

            // Apply the Alumni filter only for Create / Other University edits.
            // Skip it when we just need to display the current USA degree read-only.
            if (roles.Contains(Constants.AlumniRole) && !skipAlumniFilter)
            {
                degreeQuery = degreeQuery.Where(d =>
                    d.Institution == "Other University" &&
                    (d.DegreeType == "Bachelors" || d.DegreeType == "Masters" || d.DegreeType == "PhD") &&
                    d.MajorFieldOfStudy == "Other" &&
                    d.Department == "Other");
            }

            var degreeList = await degreeQuery.ToListAsync();

            ViewData["DegreeId"] = new SelectList(
                degreeList.Select(d => new SelectListItem
                {
                    Value = d.DegreeId.ToString(),
                    Text = $"{d.Institution}, {d.DegreeType}, {d.MajorFieldOfStudy}, {d.Department}"
                }),
                "Value", "Text", selectedId?.ToString());
        }

        [Authorize(Roles = "Admin, Alumni")]
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
                ViewData["AlumniId"] = new SelectList(new[]
                {
                    new SelectListItem
                    {
                        Value = alumni.AlumniId.ToString(),
                        Text  = $"{alumni.JagId} — {alumni.FirstName} {alumni.LastName}"
                    }
                }, "Value", "Text", alumni.AlumniId);
                ViewData["UserRole"] = Constants.AlumniRole;
            }
            else
            {
                await PopulateAlumniDropdown();
                ViewData["UserRole"] = "Admin";
            }

            await PopulateDegreeDropdown();
            return View();
        }

        [Authorize(Roles = "Admin, Alumni")]
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
                ModelState.AddModelError("DateConferred", "Date Conferred cannot be in the future.");

            if (alumniDegree.YearsToCompleteDegree.HasValue)
            {
                if (alumniDegree.YearsToCompleteDegree < 0)
                    ModelState.AddModelError("YearsToCompleteDegree", "Cannot be negative.");
                else if (alumniDegree.YearsToCompleteDegree > 10)
                    ModelState.AddModelError("YearsToCompleteDegree", "Cannot exceed 10 years.");
            }

            if (alumniDegree.Gpa.HasValue && (alumniDegree.Gpa < 0 || alumniDegree.Gpa > 4.00m))
                ModelState.AddModelError("Gpa", "GPA must be between 0.00 and 4.00.");

            if (await _context.AlumniDegrees.AnyAsync(ad =>
                    ad.AlumniId == alumniDegree.AlumniId &&
                    ad.DegreeId == alumniDegree.DegreeId))
                ModelState.AddModelError("DegreeId", "This alumni already has this degree recorded.");

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
                ViewData["AlumniId"] = new SelectList(new[]
                {
                    new SelectListItem
                    {
                        Value = alumni?.AlumniId.ToString(),
                        Text  = $"{alumni?.JagId} — {alumni?.FirstName} {alumni?.LastName}"
                    }
                }, "Value", "Text", alumniDegree.AlumniId);
                ViewData["UserRole"] = Constants.AlumniRole;
            }
            else
            {
                await PopulateAlumniDropdown(alumniDegree.AlumniId);
                ViewData["UserRole"] = "Admin";
            }

            await PopulateDegreeDropdown(alumniDegree.DegreeId);
            return View(alumniDegree);
        }

        [Authorize(Roles = "Admin, Alumni")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            // Include Degree to check the Institution type
            var alumniDegree = await _context.AlumniDegrees
                .Include(ad => ad.Degree)
                .FirstOrDefaultAsync(ad => ad.AlumniDegreeId == id);

            if (alumniDegree == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);

            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);

                // 1. Verify Ownership
                if (alumni == null || alumniDegree.AlumniId != alumni.AlumniId)
                {
                    TempData["ErrorMessage"] = "You can only edit your own degrees.";
                    return RedirectToAction(nameof(Index));
                }

                ViewData["CurrentAlumniId"] = alumni.AlumniId;
                ViewData["AlumniId"] = new SelectList(new[]
                {
                    new SelectListItem
                    {
                        Value = alumni.AlumniId.ToString(),
                        Text  = $"{alumni.JagId} — {alumni.FirstName} {alumni.LastName}"
                    }
                }, "Value", "Text", alumniDegree.AlumniId);
                ViewData["UserRole"] = Constants.AlumniRole;
            }
            else
            {
                await PopulateAlumniDropdown(alumniDegree.AlumniId);
                ViewData["UserRole"] = "Admin";
            }

            // Pass whether the degree belongs to "Other University" so the view can
            // conditionally enable/disable fields.
            bool isOtherUniversity = alumniDegree.Degree?.Institution == "Other University";
            ViewData["IsOtherUniversity"] = isOtherUniversity;

            // For a locked USA degree, skip the Alumni filter so the current degree
            // appears in the dropdown for read-only display.
            await PopulateDegreeDropdown(alumniDegree.DegreeId, skipAlumniFilter: !isOtherUniversity);
            return View(alumniDegree);
        }

        [Authorize(Roles = "Admin, Alumni")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AlumniDegreeId,AlumniId,DegreeId,DateConferred,YearsToCompleteDegree,Gpa,EmploymentWhileStudying,DegreeSpecificJob,ParticipatedInResearch,JobSecuredUponGraduation,AttendedOrPlansGradSchool")] AlumniDegree alumniDegree)
        {
            if (id != alumniDegree.AlumniDegreeId) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);

            bool isOtherUniversity = true; // default for Admin
            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                var existingRecord = await _context.AlumniDegrees
                    .Include(ad => ad.Degree)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ad => ad.AlumniDegreeId == id);

                // Re-verify Ownership on post to prevent URL manipulation
                if (alumni == null || existingRecord == null || existingRecord.AlumniId != alumni.AlumniId)
                {
                    TempData["ErrorMessage"] = "Unauthorized edit attempt.";
                    return RedirectToAction(nameof(Index));
                }

                isOtherUniversity = existingRecord.Degree?.Institution == "Other University";

                // If the degree is a University of South Alabama record, only the
                // six allowed fields may be changed. Overwrite all other fields from
                // the stored record so bound values cannot sneak in.
                if (!isOtherUniversity)
                {
                    alumniDegree.AlumniId = existingRecord.AlumniId;
                    alumniDegree.DegreeId = existingRecord.DegreeId;
                    alumniDegree.DateConferred = existingRecord.DateConferred;
                    alumniDegree.Gpa = existingRecord.Gpa;
                }
            }

            if (alumniDegree.DateConferred > DateOnly.FromDateTime(DateTime.Now))
                ModelState.AddModelError("DateConferred", "Date Conferred cannot be in the future.");

            if (alumniDegree.YearsToCompleteDegree.HasValue)
            {
                if (alumniDegree.YearsToCompleteDegree < 0)
                    ModelState.AddModelError("YearsToCompleteDegree", "Cannot be negative.");
                else if (alumniDegree.YearsToCompleteDegree > 10)
                    ModelState.AddModelError("YearsToCompleteDegree", "Cannot exceed 10 years.");
            }

            if (alumniDegree.Gpa.HasValue && (alumniDegree.Gpa < 0 || alumniDegree.Gpa > 4.00m))
                ModelState.AddModelError("Gpa", "GPA must be between 0.00 and 4.00.");

            if (await _context.AlumniDegrees.AnyAsync(ad =>
                    ad.AlumniId == alumniDegree.AlumniId &&
                    ad.DegreeId == alumniDegree.DegreeId &&
                    ad.AlumniDegreeId != alumniDegree.AlumniDegreeId))
                ModelState.AddModelError("DegreeId", "This alumniDegree already has this degree recorded.");

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
                    if (!AlumniDegreeExists(alumniDegree.AlumniDegreeId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            // Failure handling
            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                ViewData["CurrentAlumniId"] = alumni?.AlumniId;
                ViewData["AlumniId"] = new SelectList(new[]
                {
                    new SelectListItem
                    {
                        Value = alumni?.AlumniId.ToString(),
                        Text  = $"{alumni?.JagId} — {alumni?.FirstName} {alumni?.LastName}"
                    }
                }, "Value", "Text", alumniDegree.AlumniId);
                ViewData["UserRole"] = Constants.AlumniRole;
            }
            else
            {
                await PopulateAlumniDropdown(alumniDegree.AlumniId);
                ViewData["UserRole"] = "Admin";
            }

            ViewData["IsOtherUniversity"] = isOtherUniversity;
            await PopulateDegreeDropdown(alumniDegree.DegreeId, skipAlumniFilter: !isOtherUniversity);
            return View(alumniDegree);
        }

        [Authorize(Roles = "Admin, Alumni")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var alumniDegree = await _context.AlumniDegrees
                .Include(a => a.Alumni)
                .Include(a => a.Degree)
                .FirstOrDefaultAsync(m => m.AlumniDegreeId == id);
            if (alumniDegree == null) return NotFound();

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

                if (alumniDegree.Degree?.Institution != "Other University")
                {
                    TempData["ErrorMessage"] = "Only external (Other University) degrees can be deleted.";
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(alumniDegree);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Alumni")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var alumniDegree = await _context.AlumniDegrees
                .Include(a => a.Degree)
                .FirstOrDefaultAsync(a => a.AlumniDegreeId == id);

            if (alumniDegree == null)
                return RedirectToAction(nameof(Index));

            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);

            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);

                if (alumni == null || alumniDegree.AlumniId != alumni.AlumniId)
                {
                    TempData["ErrorMessage"] = "Unauthorized delete attempt.";
                    return RedirectToAction(nameof(Index));
                }

                if (alumniDegree.Degree?.Institution != "Other University")
                {
                    TempData["ErrorMessage"] = "Only external (Other University) degrees can be deleted.";
                    return RedirectToAction(nameof(Index));
                }
            }

            _context.AlumniDegrees.Remove(alumniDegree);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Degree deleted successfully!";

            return RedirectToAction(nameof(Index));
        }

        private bool AlumniDegreeExists(int id)
            => _context.AlumniDegrees.Any(e => e.AlumniDegreeId == id);
    }
}