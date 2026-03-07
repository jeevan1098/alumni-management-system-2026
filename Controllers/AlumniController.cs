using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Alumni_Management_System.Data;
using Alumni_Management_System.Models;
using Microsoft.AspNetCore.Identity;

namespace Alumni_Management_System.Controllers
{
    [Authorize]
    public class AlumniController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public AlumniController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Alumni
        public async Task<IActionResult> Index(string searchString)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var roles = await _userManager.GetRolesAsync(currentUser);

            IQueryable<Alumni> alumniQuery = _context.Alumni.Include(a => a.User);

            // Role-based filtering
            if (roles.Contains(Constants.AlumniRole))
            {
                // Alumni can only see other alumni who have SolicitationCode = true
                alumniQuery = alumniQuery.Where(a => a.SolicitationCode == true);
            }
            // Admin and Staff can see all alumni (no filtering)

            // Search functionality
            if (!string.IsNullOrEmpty(searchString))
            {
                alumniQuery = alumniQuery.Where(a =>
                    a.FirstName.Contains(searchString) ||
                    a.LastName.Contains(searchString) ||
                    a.JagId.Contains(searchString) ||
                    a.PermanentEmail.Contains(searchString));
            }

            ViewData["CurrentFilter"] = searchString;
            ViewData["UserRole"] = roles.FirstOrDefault();

            // Pass current user's alumni ID for "My Profile" button
            var currentAlumni = await _context.Alumni.FirstOrDefaultAsync(a => a.UserId == currentUser.Id);
            ViewData["CurrentAlumniId"] = currentAlumni?.AlumniId;

            return View(await alumniQuery.ToListAsync());
        }

        // GET: Alumni/MyProfile - Redirect to current user's profile edit page
        public async Task<IActionResult> MyProfile()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.UserId == currentUser.Id);
            if (alumni == null)
            {
                TempData["ErrorMessage"] = "Alumni profile not found.";
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("Edit", new { id = alumni.AlumniId });
        }

        // GET: Alumni/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alumni = await _context.Alumni
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.AlumniId == id);
            if (alumni == null)
            {
                return NotFound();
            }

            return View(alumni);
        }

        // GET: Alumni/Create
        [Authorize(Roles = "Admin")] // Only Admin can create alumni manually
        public IActionResult Create()
        {
            //ViewData["IdentityUserId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["IdentityUserId"] = new SelectList(_context.Users.OrderBy(u => u.Email), "Id", "Email");
            return View();
        }

        // POST: Alumni/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // Only Admin can create alumni manually
        public async Task<IActionResult> Create([Bind("AlumniId,UserId,JagId,Prefix,FirstName,PreferredFirstName,LastName,Gender,DateOfBirth,StudentEmail,PermanentEmail,Phone,Address,City,State,Postcode,Country,GraduationYear,SolicitationCode,SocialMediaAccount,Privacy,IsActive,LastUpdated,IdentityUserId")] Alumni alumni)
        {
            // Validate: DOB cannot be after Graduation Year
            if (alumni.DateOfBirth.HasValue && alumni.GraduationYear > 0)
            {
                if (alumni.DateOfBirth.Value.Year > alumni.GraduationYear)
                {
                    ModelState.AddModelError("DateOfBirth", "Date of Birth cannot be after Graduation Year.");
                }
            }

            if (ModelState.IsValid)
            {
                alumni.LastUpdated = DateTime.Now;
                alumni.IsActive = false;
                _context.Add(alumni);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Alumni created successfully!";
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdentityUserId"] = new SelectList(_context.Users, "Id", "Id", alumni.User);
            return View(alumni);
        }

        // GET: Alumni/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alumni = await _context.Alumni.Include(a => a.User).FirstOrDefaultAsync(a => a.AlumniId == id);
            if (alumni == null)
            {
                return NotFound();
            }

            // Check if user has permission to edit
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var roles = await _userManager.GetRolesAsync(currentUser);

            if (roles.Contains(Constants.AlumniRole))
            {
                // Alumni can only edit their own profile
                if (alumni.UserId != currentUser.Id)
                {
                    TempData["ErrorMessage"] = "You can only edit your own profile.";
                    return RedirectToAction(nameof(Index));
                }
            }
            else if (roles.Contains(Constants.StaffRole))
            {
                // Staff cannot edit any profiles
                TempData["ErrorMessage"] = "Staff members have read-only access.";
                return RedirectToAction(nameof(Index));
            }
            // Admin can edit any profile

            ViewData["IdentityUserId"] = new SelectList(_context.Users, "Id", "Id", alumni.UserId);
            return View(alumni);
        }

        // POST: Alumni/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AlumniId,UserId,JagId,Prefix,FirstName,PreferredFirstName,LastName,Gender,DateOfBirth,StudentEmail,PermanentEmail,Phone,Address,City,State,Postcode,Country,GraduationYear,SolicitationCode,SocialMediaAccount,Privacy,IsActive,LastUpdated,IdentityUserId")] Alumni alumni)
        {
            if (id != alumni.AlumniId)
            {
                return NotFound();
            }

            // Check if user has permission to edit
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var roles = await _userManager.GetRolesAsync(currentUser);

            if (roles.Contains(Constants.AlumniRole))
            {
                // Alumni can only edit their own profile
                if (alumni.UserId != currentUser.Id)
                {
                    TempData["ErrorMessage"] = "You can only edit your own profile.";
                    return RedirectToAction(nameof(Index));
                }
            }
            else if (roles.Contains(Constants.StaffRole))
            {
                // Staff cannot edit any profiles
                TempData["ErrorMessage"] = "Staff members have read-only access.";
                return RedirectToAction(nameof(Index));
            }

            // Validate: DOB cannot be after Graduation Year
            if (alumni.DateOfBirth.HasValue && alumni.GraduationYear > 0)
            {
                if (alumni.DateOfBirth.Value.Year > alumni.GraduationYear)
                {
                    ModelState.AddModelError("DateOfBirth", "Date of Birth cannot be after Graduation Year.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    alumni.LastUpdated = DateTime.Now;
                    _context.Update(alumni);

                    // Mark first login as complete for Alumni users
                    if (roles.Contains(Constants.AlumniRole) && currentUser.IsFirstLogin)
                    {
                        currentUser.IsFirstLogin = false;
                        await _userManager.UpdateAsync(currentUser);
                        TempData["SuccessMessage"] = "Profile updated successfully! Welcome to the Alumni Management System.";
                    }
                    else
                    {
                        TempData["SuccessMessage"] = "Profile updated successfully!";
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AlumniExists(alumni.AlumniId))
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
            ViewData["IdentityUserId"] = new SelectList(_context.Users, "Id", "Id", alumni.UserId);
            return View(alumni);
        }

        // GET: Alumni/Delete/5
        [Authorize(Roles = "Admin")] // Only Admin can delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alumni = await _context.Alumni
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.AlumniId == id);
            if (alumni == null)
            {
                return NotFound();
            }

            return View(alumni);
        }

        // POST: Alumni/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // Only Admin can delete
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var alumni = await _context.Alumni.FindAsync(id);
            if (alumni != null)
            {
                _context.Alumni.Remove(alumni);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Alumni deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool AlumniExists(int id)
        {
            return _context.Alumni.Any(e => e.AlumniId == id);
        }
    }
}
