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
    public class AlumniEmploymentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public AlumniEmploymentsController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: AlumniEmployments
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);

            IQueryable<AlumniEmployment> query = _context.AlumniEmployments.Include(a => a.Alumni).Include(a => a.Employer);

            // Alumni can only see their own employments
            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                if (alumni != null)
                {
                    query = query.Where(ae => ae.AlumniId == alumni.AlumniId);
                }
                else
                {
                    return View(new List<AlumniEmployment>());
                }
            }
            // Admin can see all employments

            return View(await query.ToListAsync());
        }

        // GET: AlumniEmployments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alumniEmployment = await _context.AlumniEmployments
                .Include(a => a.Alumni)
                .Include(a => a.Employer)
                .FirstOrDefaultAsync(m => m.AlumniEmploymentId == id);
            if (alumniEmployment == null)
            {
                return NotFound();
            }

            // Check if Alumni user is trying to view another alumni's employment
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

        // GET: AlumniEmployments/Create
        public async Task<IActionResult> Create()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);

            // For Alumni users, auto-select their own AlumniId
            if (roles.Contains(Constants.AlumniRole))
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
                ViewData["AlumniId"] = Services.AlumniSelectList.Build(_context.Alumni);
                ViewData["UserRole"] = "Admin";
            }

            // Add "Other" option to employers list
            var employers = await _context.Employers.OrderBy(e => e.EmployerName).ToListAsync();
            var employerList = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "-- Select Employer --" },
                new SelectListItem { Value = "0", Text = "Other (Add New)" }
            };
            employerList.AddRange(employers.Select(e => new SelectListItem { Value = e.EmployerId.ToString(), Text = e.EmployerName }));
            ViewData["EmployerId"] = employerList;

            return View();
        }

        // POST: AlumniEmployments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AlumniEmploymentId,AlumniId,EmployerId,JobTitle,StartDate,EndDate,SalaryRange")] AlumniEmployment alumniEmployment, string OtherEmployerName)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);

            // Validate: Alumni can only create employments for themselves
            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                if (alumni == null || alumniEmployment.AlumniId != alumni.AlumniId)
                {
                    TempData["ErrorMessage"] = "You can only create employment records for yourself.";
                    return RedirectToAction(nameof(Index));
                }
            }

            // Handle "Other" employer - create new employer if needed
            if (alumniEmployment.EmployerId == 0 && !string.IsNullOrWhiteSpace(OtherEmployerName))
            {
                // Check if employer already exists (case-insensitive)
                var existingEmployer = await _context.Employers
                    .FirstOrDefaultAsync(e => e.EmployerName.ToLower() == OtherEmployerName.Trim().ToLower());

                if (existingEmployer != null)
                {
                    // Use existing employer
                    alumniEmployment.EmployerId = existingEmployer.EmployerId;
                }
                else
                {
                    // Create new employer
                    var newEmployer = new Employer
                    {
                        EmployerName = OtherEmployerName.Trim()
                    };
                    _context.Employers.Add(newEmployer);
                    await _context.SaveChangesAsync();
                    alumniEmployment.EmployerId = newEmployer.EmployerId;
                }
            }

            // Validation 1: StartDate cannot be in future
            if (alumniEmployment.StartDate > DateOnly.FromDateTime(DateTime.Now))
            {
                ModelState.AddModelError("StartDate", "Start Date cannot be in the future.");
            }

            // Validation 2: EndDate cannot be before StartDate
            if (alumniEmployment.EndDate.HasValue && alumniEmployment.EndDate < alumniEmployment.StartDate)
            {
                ModelState.AddModelError("EndDate", "End Date cannot be before Start Date.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(alumniEmployment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Employment record added successfully!";
                return RedirectToAction(nameof(Index));
            }

            // Repopulate dropdowns
            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                ViewData["CurrentAlumniId"] = alumni?.AlumniId;
                ViewData["AlumniId"] = Services.AlumniSelectList.Build(new[] { alumni }, alumniEmployment.AlumniId);
                ViewData["UserRole"] = Constants.AlumniRole;
            }
            else
            {
                ViewData["AlumniId"] = Services.AlumniSelectList.Build(_context.Alumni, alumniEmployment.AlumniId);
                ViewData["UserRole"] = "Admin";
            }

            var employers = await _context.Employers.OrderBy(e => e.EmployerName).ToListAsync();
            var employerList = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "-- Select Employer --" },
                new SelectListItem { Value = "0", Text = "Other (Add New)" }
            };
            employerList.AddRange(employers.Select(e => new SelectListItem { Value = e.EmployerId.ToString(), Text = e.EmployerName }));
            ViewData["EmployerId"] = employerList;

            return View(alumniEmployment);
        }


        // GET: AlumniEmployments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alumniEmployment = await _context.AlumniEmployments.FindAsync(id);
            if (alumniEmployment == null)
            {
                return NotFound();
            }

            // Check if Alumni user is trying to edit another alumni's employment
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
                ViewData["AlumniId"] = Services.AlumniSelectList.Build(new[] { alumni }, alumniEmployment.AlumniId);
                ViewData["UserRole"] = Constants.AlumniRole;
            }
            else
            {
                ViewData["AlumniId"] = Services.AlumniSelectList.Build(_context.Alumni, alumniEmployment.AlumniId);
                ViewData["UserRole"] = "Admin";
            }

            // Add "Other" option to employers list
            var employers = await _context.Employers.OrderBy(e => e.EmployerName).ToListAsync();
            var employerList = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "-- Select Employer --" },
                new SelectListItem { Value = "0", Text = "Other (Add New)" }
            };
            employerList.AddRange(employers.Select(e => new SelectListItem { Value = e.EmployerId.ToString(), Text = e.EmployerName }));
            ViewData["EmployerId"] = employerList;

            return View(alumniEmployment);
        }

        // POST: AlumniEmployments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AlumniEmploymentId,AlumniId,EmployerId,JobTitle,StartDate,EndDate,SalaryRange")] AlumniEmployment alumniEmployment, string OtherEmployerName)
        {
            if (id != alumniEmployment.AlumniEmploymentId)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);

            // Validate: Alumni can only edit their own employments
            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                if (alumni == null || alumniEmployment.AlumniId != alumni.AlumniId)
                {
                    TempData["ErrorMessage"] = "You can only edit your own employment records.";
                    return RedirectToAction(nameof(Index));
                }
            }

            // Handle "Other" employer - create new employer if needed
            if (alumniEmployment.EmployerId == 0 && !string.IsNullOrWhiteSpace(OtherEmployerName))
            {
                // Check if employer already exists (case-insensitive)
                var existingEmployer = await _context.Employers
                    .FirstOrDefaultAsync(e => e.EmployerName.ToLower() == OtherEmployerName.Trim().ToLower());

                if (existingEmployer != null)
                {
                    // Use existing employer
                    alumniEmployment.EmployerId = existingEmployer.EmployerId;
                }
                else
                {
                    // Create new employer
                    var newEmployer = new Employer
                    {
                        EmployerName = OtherEmployerName.Trim()
                    };
                    _context.Employers.Add(newEmployer);
                    await _context.SaveChangesAsync();
                    alumniEmployment.EmployerId = newEmployer.EmployerId;
                }
            }

            // Validation 1: StartDate cannot be in future
            if (alumniEmployment.StartDate > DateOnly.FromDateTime(DateTime.Now))
            {
                ModelState.AddModelError("StartDate", "Start Date cannot be in the future.");
            }

            // Validation 2: EndDate cannot be before StartDate
            if (alumniEmployment.EndDate.HasValue && alumniEmployment.EndDate < alumniEmployment.StartDate)
            {
                ModelState.AddModelError("EndDate", "End Date cannot be before Start Date.");
            }

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
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                ViewData["CurrentAlumniId"] = alumni?.AlumniId;
                ViewData["AlumniId"] = Services.AlumniSelectList.Build(new[] { alumni }, alumniEmployment.AlumniId);
                ViewData["UserRole"] = Constants.AlumniRole;
            }
            else
            {
                ViewData["AlumniId"] = Services.AlumniSelectList.Build(_context.Alumni, alumniEmployment.AlumniId);
                ViewData["UserRole"] = "Admin";
            }

            var employers = await _context.Employers.OrderBy(e => e.EmployerName).ToListAsync();
            var employerList = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "-- Select Employer --" },
                new SelectListItem { Value = "0", Text = "Other (Add New)" }
            };
            employerList.AddRange(employers.Select(e => new SelectListItem { Value = e.EmployerId.ToString(), Text = e.EmployerName }));
            ViewData["EmployerId"] = employerList;

            return View(alumniEmployment);
        }

        // GET: AlumniEmployments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alumniEmployment = await _context.AlumniEmployments
                .Include(a => a.Alumni)
                .Include(a => a.Employer)
                .FirstOrDefaultAsync(m => m.AlumniEmploymentId == id);
            if (alumniEmployment == null)
            {
                return NotFound();
            }

            // Check if Alumni user is trying to delete another alumni's employment
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

        // POST: AlumniEmployments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var alumniEmployment = await _context.AlumniEmployments.FindAsync(id);
            if (alumniEmployment != null)
            {
                // Check if Alumni user is trying to delete another alumni's employment
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
        {
            return _context.AlumniEmployments.Any(e => e.AlumniEmploymentId == id);
        }
    }
}
