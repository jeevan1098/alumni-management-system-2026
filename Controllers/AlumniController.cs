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
using Alumni_Management_System.Models.ViewModels;
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

            IQueryable<Alumni> alumniQuery = _context.Alumni.Include(a => a.College);

            // Role-based filtering. Checked Admin/Staff-first so an account
            // that holds both Alumni and Staff/Admin (see UsersController.AddRole)
            // gets its full Staff/Admin visibility, not the restricted
            // Alumni-only view - Alumni-only restriction applies only to
            // accounts that don't also hold a Staff/Admin role.
            var hasElevatedRole = roles.Contains(Constants.AdminRole) || roles.Contains(Constants.StaffRole);
            if (roles.Contains(Constants.AlumniRole) && !hasElevatedRole)
            {
                // Alumni can only see other alumni who have SolicitationCode = true
                alumniQuery = alumniQuery.Where(a => a.SolicitationCode == true);
            }
            else
            {
                // Admin/Staff see everything, unless an admin has scoped them
                // to specific college(s) via Access Scopes.
                var allowedCollegeIds = await Services.AccessScopeService.GetAllowedCollegeIdsAsync(_context, currentUser, roles);
                if (allowedCollegeIds != null)
                {
                    alumniQuery = alumniQuery.Where(a => a.CollegeId != null && allowedCollegeIds.Contains(a.CollegeId.Value));

                    var scopedCollegeNames = await _context.Colleges
                        .Where(c => allowedCollegeIds.Contains(c.CollegeId))
                        .Select(c => c.CollegeName)
                        .ToListAsync();
                    ViewData["ScopeLabel"] = string.Join(", ", scopedCollegeNames);
                }
            }

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
            // Same priority as the filtering above - Admin > Staff > Alumni -
            // so the page renders the admin/staff layout for a dual-role
            // account instead of picking whichever role happened to load first.
            ViewData["UserRole"] = roles.Contains(Constants.AdminRole) ? Constants.AdminRole
                : roles.Contains(Constants.StaffRole) ? Constants.StaffRole
                : roles.FirstOrDefault();

            // Pass current user's alumni ID for "My Profile" button
            var currentAlumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
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

            var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == currentUser.JagId);
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
                .Include(a => a.College)
                .FirstOrDefaultAsync(m => m.AlumniId == id);
            if (alumni == null)
            {
                return NotFound();
            }

            return View(alumni);
        }

        // GET: Alumni/Create
        [Authorize(Roles = "Admin")] // Only Admin can create alumni manually
        public async Task<IActionResult> Create()
        {
            // No need for IdentityUserId dropdown since we're using JagId now
            ViewData["CollegeId"] = new SelectList(await _context.Colleges.Where(c => c.IsActive).ToListAsync(), "CollegeId", "CollegeName");
            return View();
        }

        // POST: Alumni/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // Only Admin can create alumni manually
        public async Task<IActionResult> Create([Bind("AlumniId,JagId,Prefix,FirstName,PreferredFirstName,MiddleName,LastName,Suffix,Gender,DateOfBirth,CollegeId,StudentEmail,PermanentEmail,Phone,Address,City,State,Postcode,Country,GraduationYear,SolicitationCode,SocialMediaAccount,Privacy,IsActive,LastUpdated")] Alumni alumni)
        {

            if (ModelState.IsValid)
            {
                alumni.LastUpdated = DateTime.Now;
                alumni.IsActive = false;
                _context.Add(alumni);

                // Every Alumni record needs a matching Registry entry, or
                // this person can never self-register a login (VerifyJagId
                // checks the Registry, not the Alumni table - see the same
                // fix already applied to bulk import).
                if (!await _context.AlumniRegistries.AnyAsync(r => r.JagId == alumni.JagId))
                {
                    _context.AlumniRegistries.Add(new AlumniRegistry
                    {
                        JagId = alumni.JagId,
                        FirstName = alumni.FirstName,
                        LastName = alumni.LastName,
                        AccountCreated = false
                    });
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Alumni created successfully!";
                return RedirectToAction(nameof(Index));
            }
            ViewData["CollegeId"] = new SelectList(await _context.Colleges.Where(c => c.IsActive).ToListAsync(), "CollegeId", "CollegeName", alumni.CollegeId);
            return View(alumni);
        }

        // GET: Alumni/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.AlumniId == id);
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
                if (alumni.JagId != currentUser.JagId)
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

            // No need for IdentityUserId since we're using JagId now
            ViewData["CollegeId"] = new SelectList(await _context.Colleges.Where(c => c.IsActive).ToListAsync(), "CollegeId", "CollegeName", alumni.CollegeId);

            // A person can hold several degrees, each tied to its own
            // Department/College via the DegreeProgram - that can differ
            // from (and outnumber) the single "College" field above, so
            // show them separately as a read-only summary.
            ViewData["DegreeColleges"] = await _context.AlumniDegrees
                .Where(ad => ad.AlumniId == alumni.AlumniId)
                .Select(ad => new AlumniDegreeCollegeViewModel
                {
                    DegreeType = ad.Degree.DegreeType,
                    MajorFieldOfStudy = ad.Degree.MajorFieldOfStudy,
                    DepartmentName = ad.Degree.Department.DepartmentName,
                    CollegeName = ad.Degree.Department.College.CollegeName
                })
                .ToListAsync();

            return View(alumni);
        }

        // POST: Alumni/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AlumniId,JagId,Prefix,FirstName,PreferredFirstName,MiddleName,LastName,Suffix,Gender,DateOfBirth,CollegeId,StudentEmail,PermanentEmail,Phone,Address,City,State,Postcode,Country,GraduationYear,SolicitationCode,SocialMediaAccount,Privacy,IsActive,LastUpdated")] Alumni alumni)
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
                if (alumni.JagId != currentUser.JagId)
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
            // No need for ViewData["IdentityUserId"] since we're using JagId now
            ViewData["CollegeId"] = new SelectList(await _context.Colleges.Where(c => c.IsActive).ToListAsync(), "CollegeId", "CollegeName", alumni.CollegeId);
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
                .Include(a => a.College)
                .FirstOrDefaultAsync(m => m.AlumniId == id);
            if (alumni == null)
            {
                return NotFound();
            }

            // JagId<->AppUser is a logical link, not a DB relationship - look
            // it up manually so the Delete confirmation page can still show
            // which login account (if any) will be removed alongside it.
            alumni.User = await _userManager.Users.FirstOrDefaultAsync(u => u.JagId == alumni.JagId);

            return View(alumni);
        }

        // POST: Alumni/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // Only Admin can delete
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var alumni = await _context.Alumni
                .Include(a => a.College)
                .FirstOrDefaultAsync(m => m.AlumniId == id);

            if (alumni == null)
            {
                return NotFound();
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // 2. Identify the linked AppUser (logical link via JagId)
                    var user = await _userManager.Users.FirstOrDefaultAsync(u => u.JagId == alumni.JagId);

                    // 3. Remove the Alumni profile first
                    // This triggers the database CASCADE to history tables (Degrees, etc.)
                    _context.Alumni.Remove(alumni);

                    // 4. Remove the linked Identity User if they have one
                    // This is the "Reverse Cascade" manual step
                    if (user != null)
                    {
                        var result = await _userManager.DeleteAsync(user);
                        if (!result.Succeeded)
                        {
                            throw new Exception("Failed to delete associated user account.");
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["SuccessMessage"] = "Alumni and associated user account deleted successfully!";
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    TempData["ErrorMessage"] = "Error during deletion: " + ex.Message;
                }
            }

            return RedirectToAction(nameof(Index));
        }

        private bool AlumniExists(int id)
        {
            return _context.Alumni.Any(e => e.AlumniId == id);
        }
    }
}
