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
    public class AlumniEmploymentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public AlumniEmploymentsController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ══════════════════════════════════════════════════════════════
        //  VALIDATION HELPERS
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// Returns true if [aStart, aEnd] overlaps [bStart, bEnd].
        /// A null EndDate means "open / current" = today for overlap purposes.
        /// </summary>
        private static bool RangesOverlap(DateOnly aStart, DateOnly? aEnd,
                                          DateOnly bStart, DateOnly? bEnd)
        {
            var aE = aEnd ?? DateOnly.MaxValue;
            var bE = bEnd ?? DateOnly.MaxValue;
            // Overlap when: aStart <= bEnd  AND  bStart <= aEnd
            return aStart <= bE && bStart <= aE;
        }

        /// <summary>
        /// Validates overlap and single-current rules against all existing
        /// employment records for this alumni, excluding the record being edited
        /// (excludeId == null on Create).
        /// Adds errors directly to ModelState.
        /// </summary>
        private async Task ValidateEmploymentRules(
            int alumniId,
            DateOnly startDate,
            DateOnly? endDate,
            int? excludeId = null)
        {
            var existing = await _context.AlumniEmployments
                .Where(e => e.AlumniId == alumniId &&
                            (excludeId == null || e.AlumniEmploymentId != excludeId))
                .ToListAsync();

            // Rule 1 – only one "Current" (null EndDate) per alumni
            if (!endDate.HasValue)
            {
                bool alreadyHasCurrent = existing.Any(e => !e.EndDate.HasValue);
                if (alreadyHasCurrent)
                    ModelState.AddModelError("EndDate",
                        "You already have a current employment record (no end date). " +
                        "Please set an end date on the existing current job before adding a new one.");
            }

            // Rule 2 – no overlapping date ranges
            foreach (var record in existing)
            {
                if (RangesOverlap(startDate, endDate, record.StartDate, record.EndDate))
                {
                    var endLabel = record.EndDate.HasValue
                        ? record.EndDate.Value.ToString("MMM dd, yyyy")
                        : "Present";

                    ModelState.AddModelError("StartDate",
                        $"This period overlaps with an existing employment record: " +
                        $"\"{record.JobTitle}\" " +
                        $"({record.StartDate:MMM dd, yyyy} – {endLabel}). " +
                        $"Employment periods cannot overlap.");
                    break; // one error is enough; no need to list every conflict
                }
            }
        }

        // ══════════════════════════════════════════════════════════════
        //  INDEX
        // ══════════════════════════════════════════════════════════════

        public async Task<IActionResult> Index(string searchString, string sortOrder)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);

            IQueryable<AlumniEmployment> query = _context.AlumniEmployments
                .Include(a => a.Alumni)
                .Include(a => a.Employer);

            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni
                    .FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                if (alumni != null)
                    query = query.Where(ae => ae.AlumniId == alumni.AlumniId);
                else
                    return View(new List<AlumniEmployment>());
            }

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var s = searchString.ToLower();
                query = query.Where(e =>
                    e.Alumni.FirstName.ToLower().Contains(s) ||
                    e.Alumni.LastName.ToLower().Contains(s) ||
                    e.Alumni.JagId.ToLower().Contains(s) ||
                    e.JobTitle.ToLower().Contains(s) ||
                    e.Employer.EmployerName.ToLower().Contains(s));
            }

            query = sortOrder switch
            {
                "name" => query.OrderBy(e => e.Alumni.LastName).ThenBy(e => e.Alumni.FirstName),
                "name_desc" => query.OrderByDescending(e => e.Alumni.LastName).ThenByDescending(e => e.Alumni.FirstName),
                "jobtitle" => query.OrderBy(e => e.JobTitle),
                "jobtitle_desc" => query.OrderByDescending(e => e.JobTitle),
                "employer" => query.OrderBy(e => e.Employer.EmployerName),
                "employer_desc" => query.OrderByDescending(e => e.Employer.EmployerName),
                "start" => query.OrderBy(e => e.StartDate),
                "start_desc" => query.OrderByDescending(e => e.StartDate),
                "end" => query.OrderBy(e => e.EndDate),
                "end_desc" => query.OrderByDescending(e => e.EndDate),
                _ => query.OrderByDescending(e => e.StartDate),
            };

            ViewData["CurrentFilter"] = searchString;
            ViewData["SortOrder"] = sortOrder;

            return View(await query.ToListAsync());
        }

        // ══════════════════════════════════════════════════════════════
        //  DETAILS
        // ══════════════════════════════════════════════════════════════

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var alumniEmployment = await _context.AlumniEmployments
                .Include(a => a.Alumni)
                .Include(a => a.Employer)
                .FirstOrDefaultAsync(m => m.AlumniEmploymentId == id);
            if (alumniEmployment == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);
            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni
                    .FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                if (alumni == null || alumniEmployment.AlumniId != alumni.AlumniId)
                {
                    TempData["ErrorMessage"] = "You can only view your own employment records.";
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(alumniEmployment);
        }

        // ══════════════════════════════════════════════════════════════
        //  DROPDOWN HELPERS
        // ══════════════════════════════════════════════════════════════

        private async Task PopulateAlumniDropdown(object selectedId = null)
        {
            var alumniList = await _context.Alumni.OrderBy(a => a.LastName).ToListAsync();
            ViewData["AlumniId"] = new SelectList(
                alumniList.Select(a => new SelectListItem
                {
                    Value = a.AlumniId.ToString(),
                    Text = $"{a.JagId} — {a.FirstName} {a.LastName}"
                }),
                "Value", "Text", selectedId?.ToString());
        }

        private async Task PopulateEmployerDropdown(object selectedId = null)
        {
            var employers = await _context.Employers.OrderBy(e => e.EmployerName).ToListAsync();
            var list = new List<SelectListItem>
            {
                new SelectListItem { Value = "",  Text = "-- Select Employer --" },
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

        private async Task RepopulateDropdowns(
            AlumniEmployment model,
            AppUser currentUser,
            IList<string> roles)
        {
            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni
                    .FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                ViewData["CurrentAlumniId"] = alumni?.AlumniId;
                ViewData["AlumniId"] = new SelectList(new[]
                {
                    new SelectListItem
                    {
                        Value = alumni?.AlumniId.ToString(),
                        Text  = $"{alumni?.JagId} — {alumni?.FirstName} {alumni?.LastName}"
                    }
                }, "Value", "Text", model.AlumniId);
                ViewData["UserRole"] = Constants.AlumniRole;
            }
            else
            {
                await PopulateAlumniDropdown(model.AlumniId);
                ViewData["UserRole"] = "Admin";
            }
            await PopulateEmployerDropdown(model.EmployerId);
        }

        // ══════════════════════════════════════════════════════════════
        //  CREATE
        // ══════════════════════════════════════════════════════════════

        [Authorize(Roles = "Admin, Alumni")]
        public async Task<IActionResult> Create()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);

            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni
                    .FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
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

            await PopulateEmployerDropdown();
            return View();
        }

        [Authorize(Roles = "Admin, Alumni")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("AlumniEmploymentId,AlumniId,EmployerId,JobTitle,StartDate,EndDate,SalaryRange")]
            AlumniEmployment alumniEmployment,
            string OtherEmployerName)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);

            // Alumni can only create for themselves
            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni
                    .FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                if (alumni == null || alumniEmployment.AlumniId != alumni.AlumniId)
                {
                    TempData["ErrorMessage"] = "You can only create employment records for yourself.";
                    return RedirectToAction(nameof(Index));
                }
            }

            // Resolve "Other" employer
            await ResolveOtherEmployer(alumniEmployment, OtherEmployerName);

            // ── Exact duplicate check ──
            bool isDuplicate = await _context.AlumniEmployments.AnyAsync(e =>
                e.AlumniId == alumniEmployment.AlumniId &&
                e.EmployerId == alumniEmployment.EmployerId &&
                e.JobTitle == alumniEmployment.JobTitle &&
                e.StartDate == alumniEmployment.StartDate &&
                e.EndDate == alumniEmployment.EndDate);

            if (isDuplicate)
                ModelState.AddModelError(string.Empty,
                    "An identical employment record already exists for this alumni.");

            // ── Basic date checks ──
            if (alumniEmployment.StartDate > DateOnly.FromDateTime(DateTime.Now))
                ModelState.AddModelError("StartDate", "Start date cannot be in the future.");

            if (alumniEmployment.EndDate.HasValue &&
                alumniEmployment.EndDate < alumniEmployment.StartDate)
                ModelState.AddModelError("EndDate", "End date cannot be before start date.");

            // ── Timeline validation (overlap + single current) ──
            if (!ModelState.ContainsKey("StartDate") && !ModelState.ContainsKey("EndDate"))
                await ValidateEmploymentRules(
                    alumniEmployment.AlumniId,
                    alumniEmployment.StartDate,
                    alumniEmployment.EndDate,
                    excludeId: null);

            if (ModelState.IsValid)
            {
                _context.Add(alumniEmployment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Employment record added successfully!";
                return RedirectToAction(nameof(Index));
            }

            await RepopulateDropdowns(alumniEmployment, currentUser, roles);
            return View(alumniEmployment);
        }

        // ══════════════════════════════════════════════════════════════
        //  EDIT
        // ══════════════════════════════════════════════════════════════

        [Authorize(Roles = "Admin, Alumni")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var alumniEmployment = await _context.AlumniEmployments.FindAsync(id);
            if (alumniEmployment == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);

            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni
                    .FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                if (alumni == null || alumniEmployment.AlumniId != alumni.AlumniId)
                {
                    TempData["ErrorMessage"] = "You can only edit your own employment records.";
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
                }, "Value", "Text", alumniEmployment.AlumniId);
                ViewData["UserRole"] = Constants.AlumniRole;
            }
            else
            {
                await PopulateAlumniDropdown(alumniEmployment.AlumniId);
                ViewData["UserRole"] = "Admin";
            }

            await PopulateEmployerDropdown(alumniEmployment.EmployerId);
            return View(alumniEmployment);
        }

        [Authorize(Roles = "Admin, Alumni")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("AlumniEmploymentId,AlumniId,EmployerId,JobTitle,StartDate,EndDate,SalaryRange")]
            AlumniEmployment alumniEmployment,
            string OtherEmployerName)
        {
            if (id != alumniEmployment.AlumniEmploymentId) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);

            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni
                    .FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                if (alumni == null || alumniEmployment.AlumniId != alumni.AlumniId)
                {
                    TempData["ErrorMessage"] = "You can only edit your own employment records.";
                    return RedirectToAction(nameof(Index));
                }
            }

            // Resolve "Other" employer
            await ResolveOtherEmployer(alumniEmployment, OtherEmployerName);

            // ── Exact duplicate check (excluding self) ──
            bool isDuplicate = await _context.AlumniEmployments.AnyAsync(e =>
                e.AlumniEmploymentId != alumniEmployment.AlumniEmploymentId &&
                e.AlumniId == alumniEmployment.AlumniId &&
                e.EmployerId == alumniEmployment.EmployerId &&
                e.JobTitle == alumniEmployment.JobTitle &&
                e.StartDate == alumniEmployment.StartDate &&
                e.EndDate == alumniEmployment.EndDate);

            if (isDuplicate)
                ModelState.AddModelError(string.Empty,
                    "An identical employment record already exists for this alumni.");

            // ── Basic date checks ──
            if (alumniEmployment.StartDate > DateOnly.FromDateTime(DateTime.Now))
                ModelState.AddModelError("StartDate", "Start date cannot be in the future.");

            if (alumniEmployment.EndDate.HasValue &&
                alumniEmployment.EndDate < alumniEmployment.StartDate)
                ModelState.AddModelError("EndDate", "End date cannot be before start date.");

            // ── Timeline validation (overlap + single current), excluding self ──
            if (!ModelState.ContainsKey("StartDate") && !ModelState.ContainsKey("EndDate"))
                await ValidateEmploymentRules(
                    alumniEmployment.AlumniId,
                    alumniEmployment.StartDate,
                    alumniEmployment.EndDate,
                    excludeId: alumniEmployment.AlumniEmploymentId);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(alumniEmployment);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Employment record updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AlumniEmploymentExists(alumniEmployment.AlumniEmploymentId))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            await RepopulateDropdowns(alumniEmployment, currentUser, roles);
            return View(alumniEmployment);
        }

        // ══════════════════════════════════════════════════════════════
        //  DELETE
        // ══════════════════════════════════════════════════════════════

        [Authorize(Roles = "Admin, Alumni")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var alumniEmployment = await _context.AlumniEmployments
                .Include(a => a.Alumni)
                .Include(a => a.Employer)
                .FirstOrDefaultAsync(m => m.AlumniEmploymentId == id);
            if (alumniEmployment == null) return NotFound();

            return View(alumniEmployment);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Alumni")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var alumniEmployment = await _context.AlumniEmployments.FindAsync(id);
            if (alumniEmployment != null)
            {
                _context.AlumniEmployments.Remove(alumniEmployment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Employment record deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        // ══════════════════════════════════════════════════════════════
        //  PRIVATE UTILITIES
        // ══════════════════════════════════════════════════════════════

        private async Task ResolveOtherEmployer(AlumniEmployment model, string otherName)
        {
            if (model.EmployerId != 0 || string.IsNullOrWhiteSpace(otherName))
                return;

            var existing = await _context.Employers
                .FirstOrDefaultAsync(e =>
                    e.EmployerName.ToLower() == otherName.Trim().ToLower());

            if (existing != null)
            {
                model.EmployerId = existing.EmployerId;
            }
            else
            {
                var newEmployer = new Employer { EmployerName = otherName.Trim() };
                _context.Employers.Add(newEmployer);
                await _context.SaveChangesAsync();
                model.EmployerId = newEmployer.EmployerId;
            }
        }

        private bool AlumniEmploymentExists(int id)
            => _context.AlumniEmployments.Any(e => e.AlumniEmploymentId == id);
    }
}
