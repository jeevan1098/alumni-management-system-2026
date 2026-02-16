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
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.UserId == currentUser.Id);
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
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.UserId == currentUser.Id);
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

        // POST: AlumniEmployments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AlumniEmploymentId,AlumniId,EmployerId,JobTitle,StartDate,EndDate,SalaryRange")] AlumniEmployment alumniEmployment)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);

            // Validate: Alumni can only create employments for themselves
            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.UserId == currentUser.Id);
                if (alumni == null || alumniEmployment.AlumniId != alumni.AlumniId)
                {
                    TempData["ErrorMessage"] = "You can only create employment records for yourself.";
                    return RedirectToAction(nameof(Index));
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

            // Validation 3: Salary validation (if SalaryRange is numeric, parse and validate)
            // Note: SalaryRange is a string in the model, so we'll validate if it contains a number
            if (!string.IsNullOrEmpty(alumniEmployment.SalaryRange))
            {
                // Try to extract numeric value from salary range
                var salaryNumbers = System.Text.RegularExpressions.Regex.Matches(alumniEmployment.SalaryRange, @"\d+");
                foreach (System.Text.RegularExpressions.Match match in salaryNumbers)
                {
                    if (decimal.TryParse(match.Value, out decimal salary))
                    {
                        if (salary < 0)
                        {
                            ModelState.AddModelError("SalaryRange", "Salary cannot be negative.");
                            break;
                        }
                        if (salary > 1000000000) // 1 billion
                        {
                            ModelState.AddModelError("SalaryRange", "Salary cannot exceed 1 billion.");
                            break;
                        }
                    }
                }
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
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.UserId == currentUser.Id);
                ViewData["CurrentAlumniId"] = alumni?.AlumniId;
                ViewData["AlumniId"] = new SelectList(new[] { alumni }, "AlumniId", "FirstName", alumniEmployment.AlumniId);
            }
            else
            {
                ViewData["AlumniId"] = new SelectList(_context.Alumni, "AlumniId", "FirstName", alumniEmployment.AlumniId);
            }
            ViewData["EmployerId"] = new SelectList(_context.Employers, "EmployerId", "EmployerName", alumniEmployment.EmployerId);
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
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.UserId == currentUser.Id);
                if (alumni == null || alumniEmployment.AlumniId != alumni.AlumniId)
                {
                    TempData["ErrorMessage"] = "You can only edit your own employment records.";
                    return RedirectToAction(nameof(Index));
                }
                ViewData["CurrentAlumniId"] = alumni.AlumniId;
                ViewData["AlumniId"] = new SelectList(new[] { alumni }, "AlumniId", "FirstName", alumniEmployment.AlumniId);
            }
            else
            {
                ViewData["AlumniId"] = new SelectList(_context.Alumni, "AlumniId", "FirstName", alumniEmployment.AlumniId);
            }

            ViewData["EmployerId"] = new SelectList(_context.Employers, "EmployerId", "EmployerName", alumniEmployment.EmployerId);
            return View(alumniEmployment);
        }

        // POST: AlumniEmployments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AlumniEmploymentId,AlumniId,EmployerId,JobTitle,StartDate,EndDate,SalaryRange")] AlumniEmployment alumniEmployment)
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
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.UserId == currentUser.Id);
                if (alumni == null || alumniEmployment.AlumniId != alumni.AlumniId)
                {
                    TempData["ErrorMessage"] = "You can only edit your own employment records.";
                    return RedirectToAction(nameof(Index));
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

            // Validation 3: Salary validation
            if (!string.IsNullOrEmpty(alumniEmployment.SalaryRange))
            {
                var salaryNumbers = System.Text.RegularExpressions.Regex.Matches(alumniEmployment.SalaryRange, @"\d+");
                foreach (System.Text.RegularExpressions.Match match in salaryNumbers)
                {
                    if (decimal.TryParse(match.Value, out decimal salary))
                    {
                        if (salary < 0)
                        {
                            ModelState.AddModelError("SalaryRange", "Salary cannot be negative.");
                            break;
                        }
                        if (salary > 1000000000)
                        {
                            ModelState.AddModelError("SalaryRange", "Salary cannot exceed 1 billion.");
                            break;
                        }
                    }
                }
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
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.UserId == currentUser.Id);
                ViewData["CurrentAlumniId"] = alumni?.AlumniId;
                ViewData["AlumniId"] = new SelectList(new[] { alumni }, "AlumniId", "FirstName", alumniEmployment.AlumniId);
            }
            else
            {
                ViewData["AlumniId"] = new SelectList(_context.Alumni, "AlumniId", "FirstName", alumniEmployment.AlumniId);
            }
            ViewData["EmployerId"] = new SelectList(_context.Employers, "EmployerId", "EmployerName", alumniEmployment.EmployerId);
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
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.UserId == currentUser.Id);
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
                    var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.UserId == currentUser.Id);
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
