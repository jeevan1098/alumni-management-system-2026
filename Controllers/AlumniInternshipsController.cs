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
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
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
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
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

        // POST: AlumniInternships/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AlumniInternshipId,AlumniId,EmployerId,InternshipType,Title,StartDate,EndDate")] AlumniInternship alumniInternship, string OtherEmployerName)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);

            // Validate: Alumni can only create internships for themselves
            if (roles.Contains(Constants.AlumniRole))
            {
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                if (alumni == null || alumniInternship.AlumniId != alumni.AlumniId)
                {
                    TempData["ErrorMessage"] = "You can only create internship records for yourself.";
                    return RedirectToAction(nameof(Index));
                }
            }

            // Handle "Other" employer - create new employer if needed
            if (alumniInternship.EmployerId == 0 && !string.IsNullOrWhiteSpace(OtherEmployerName))
            {
                // Check if employer already exists (case-insensitive)
                var existingEmployer = await _context.Employers
                    .FirstOrDefaultAsync(e => e.EmployerName.ToLower() == OtherEmployerName.Trim().ToLower());

                if (existingEmployer != null)
                {
                    // Use existing employer
                    alumniInternship.EmployerId = existingEmployer.EmployerId;
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
                    alumniInternship.EmployerId = newEmployer.EmployerId;
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
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                ViewData["CurrentAlumniId"] = alumni?.AlumniId;
                ViewData["AlumniId"] = Services.AlumniSelectList.Build(new[] { alumni }, alumniInternship.AlumniId);
                ViewData["UserRole"] = Constants.AlumniRole;
            }
            else
            {
                ViewData["AlumniId"] = Services.AlumniSelectList.Build(_context.Alumni, alumniInternship.AlumniId);
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
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                if (alumni == null || alumniInternship.AlumniId != alumni.AlumniId)
                {
                    TempData["ErrorMessage"] = "You can only edit your own internship records.";
                    return RedirectToAction(nameof(Index));
                }
                ViewData["CurrentAlumniId"] = alumni.AlumniId;
                ViewData["AlumniId"] = Services.AlumniSelectList.Build(new[] { alumni }, alumniInternship.AlumniId);
                ViewData["UserRole"] = Constants.AlumniRole;
            }
            else
            {
                ViewData["AlumniId"] = Services.AlumniSelectList.Build(_context.Alumni, alumniInternship.AlumniId);
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

            return View(alumniInternship);
        }

        // POST: AlumniInternships/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AlumniInternshipId,AlumniId,EmployerId,InternshipType,Title,StartDate,EndDate")] AlumniInternship alumniInternship, string OtherEmployerName)
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
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                if (alumni == null || alumniInternship.AlumniId != alumni.AlumniId)
                {
                    TempData["ErrorMessage"] = "You can only edit your own internship records.";
                    return RedirectToAction(nameof(Index));
                }
            }

            // Handle "Other" employer - create new employer if needed
            if (alumniInternship.EmployerId == 0 && !string.IsNullOrWhiteSpace(OtherEmployerName))
            {
                // Check if employer already exists (case-insensitive)
                var existingEmployer = await _context.Employers
                    .FirstOrDefaultAsync(e => e.EmployerName.ToLower() == OtherEmployerName.Trim().ToLower());

                if (existingEmployer != null)
                {
                    // Use existing employer
                    alumniInternship.EmployerId = existingEmployer.EmployerId;
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
                    alumniInternship.EmployerId = newEmployer.EmployerId;
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
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
                ViewData["CurrentAlumniId"] = alumni?.AlumniId;
                ViewData["AlumniId"] = Services.AlumniSelectList.Build(new[] { alumni }, alumniInternship.AlumniId);
                ViewData["UserRole"] = Constants.AlumniRole;
            }
            else
            {
                ViewData["AlumniId"] = Services.AlumniSelectList.Build(_context.Alumni, alumniInternship.AlumniId);
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
                var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
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
                    var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
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
