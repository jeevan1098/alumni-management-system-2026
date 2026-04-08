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

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);

            IQueryable<AlumniEmployment> query = _context.AlumniEmployments
                .Include(a => a.Alumni)
                .Include(a => a.Employer);

            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                if (alumni != null)
                    query = query.Where(ae => ae.AlumniId == alumni.AlumniId);
                else
                    return View(new List<AlumniEmployment>());
            }

            return View(await query.ToListAsync());
        }

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
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                if (alumni == null || alumniEmployment.AlumniId != alumni.AlumniId)
                {
                    TempData["ErrorMessage"] = "You can only view your own employment records.";
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(alumniEmployment);
        }

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
            var employerList = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "-- Select Employer --" },
                new SelectListItem { Value = "0", Text = "Other (Add New)" }
            };
            employerList.AddRange(employers.Select(e => new SelectListItem
            {
                Value = e.EmployerId.ToString(),
                Text = e.EmployerName,
                Selected = selectedId != null && e.EmployerId.ToString() == selectedId.ToString()
            }));
            ViewData["EmployerId"] = employerList;
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

            await PopulateEmployerDropdown();
            return View();
        }

        [Authorize(Roles = "Admin, Alumni")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AlumniEmploymentId,AlumniId,EmployerId,JobTitle,StartDate,EndDate,SalaryRange")] AlumniEmployment alumniEmployment, string OtherEmployerName)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);

            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                if (alumni == null || alumniEmployment.AlumniId != alumni.AlumniId)
                {
                    TempData["ErrorMessage"] = "You can only create employment records for yourself.";
                    return RedirectToAction(nameof(Index));
                }
            }

            if (alumniEmployment.EmployerId == 0 && !string.IsNullOrWhiteSpace(OtherEmployerName))
            {
                var existingEmployer = await _context.Employers
                    .FirstOrDefaultAsync(e => e.EmployerName.ToLower() == OtherEmployerName.Trim().ToLower());

                if (existingEmployer != null)
                    alumniEmployment.EmployerId = existingEmployer.EmployerId;
                else
                {
                    var newEmployer = new Employer { EmployerName = OtherEmployerName.Trim() };
                    _context.Employers.Add(newEmployer);
                    await _context.SaveChangesAsync();
                    alumniEmployment.EmployerId = newEmployer.EmployerId;
                }
            }

            if (alumniEmployment.StartDate > DateOnly.FromDateTime(DateTime.Now))
                ModelState.AddModelError("StartDate", "Start Date cannot be in the future.");

            if (alumniEmployment.EndDate.HasValue && alumniEmployment.EndDate < alumniEmployment.StartDate)
                ModelState.AddModelError("EndDate", "End Date cannot be before Start Date.");

            if (ModelState.IsValid)
            {
                _context.Add(alumniEmployment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Employment record added successfully!";
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
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var alumniEmployment = await _context.AlumniEmployments.FindAsync(id);
            if (alumniEmployment == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);

            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
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
        public async Task<IActionResult> Edit(int id, [Bind("AlumniEmploymentId,AlumniId,EmployerId,JobTitle,StartDate,EndDate,SalaryRange")] AlumniEmployment alumniEmployment, string OtherEmployerName)
        {
            if (id != alumniEmployment.AlumniEmploymentId) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);

            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                if (alumni == null || alumniEmployment.AlumniId != alumni.AlumniId)
                {
                    TempData["ErrorMessage"] = "You can only edit your own employment records.";
                    return RedirectToAction(nameof(Index));
                }
            }

            if (alumniEmployment.EmployerId == 0 && !string.IsNullOrWhiteSpace(OtherEmployerName))
            {
                var existingEmployer = await _context.Employers
                    .FirstOrDefaultAsync(e => e.EmployerName.ToLower() == OtherEmployerName.Trim().ToLower());

                if (existingEmployer != null)
                    alumniEmployment.EmployerId = existingEmployer.EmployerId;
                else
                {
                    var newEmployer = new Employer { EmployerName = OtherEmployerName.Trim() };
                    _context.Employers.Add(newEmployer);
                    await _context.SaveChangesAsync();
                    alumniEmployment.EmployerId = newEmployer.EmployerId;
                }
            }

            if (alumniEmployment.StartDate > DateOnly.FromDateTime(DateTime.Now))
                ModelState.AddModelError("StartDate", "Start Date cannot be in the future.");

            if (alumniEmployment.EndDate.HasValue && alumniEmployment.EndDate < alumniEmployment.StartDate)
                ModelState.AddModelError("EndDate", "End Date cannot be before Start Date.");

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
                    if (!AlumniEmploymentExists(alumniEmployment.AlumniEmploymentId)) return NotFound();
                    else throw;
                }
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

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
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
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                if (alumni == null || alumniEmployment.AlumniId != alumni.AlumniId)
                {
                    TempData["ErrorMessage"] = "You can only delete your own employment records.";
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(alumniEmployment);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var alumniEmployment = await _context.AlumniEmployments.FindAsync(id);
            if (alumniEmployment != null)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var roles = await _userManager.GetRolesAsync(currentUser);
                if (roles.Contains(Constants.AlumniRole))
                {
                    var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                    if (alumni == null || alumniEmployment.AlumniId != alumni.AlumniId)
                    {
                        TempData["ErrorMessage"] = "You can only delete your own employment records.";
                        return RedirectToAction(nameof(Index));
                    }
                }

                _context.AlumniEmployments.Remove(alumniEmployment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Employment record deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool AlumniEmploymentExists(int id)
            => _context.AlumniEmployments.Any(e => e.AlumniEmploymentId == id);
    }
}
