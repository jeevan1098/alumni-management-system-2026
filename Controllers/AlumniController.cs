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

            // Read-only listing - AsNoTracking so HideContactDetails() below can
            // never be saved back by accident.
            IQueryable<Alumni> alumniQuery = _context.Alumni.AsNoTracking().Include(a => a.College);

            // Role-based filtering. Checked Admin/Staff-first so an account
            // that holds both Alumni and Staff/Admin (see UsersController.AddRole)
            // gets its full Staff/Admin visibility, not the restricted
            // Alumni-only view - Alumni-only restriction applies only to
            // accounts that don't also hold a Staff/Admin role.
            var alumniOnly = IsAlumniOnly(roles);
            if (!alumniOnly)
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

            // Search functionality. Alumni can't search by the email of someone
            // who keeps their contact details private - that would reveal it.
            if (!string.IsNullOrEmpty(searchString))
            {
                alumniQuery = alumniQuery.Where(a =>
                    a.FirstName.Contains(searchString) ||
                    a.LastName.Contains(searchString) ||
                    a.JagId.Contains(searchString) ||
                    ((!alumniOnly || !a.Privacy) && a.PermanentEmail.Contains(searchString)));
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

            var alumniList = await alumniQuery.ToListAsync();
            if (alumniOnly)
            {
                foreach (var a in alumniList.Where(a => a.Privacy && a.AlumniId != currentAlumni?.AlumniId))
                {
                    a.HideContactDetails();
                }
            }

            return View(alumniList);
        }

        // Every alumnus is listed in the directory for other alumni (name,
        // degrees, etc.). Privacy only hides contact details, and
        // SolicitationCode only controls messages - see Alumni.PrivacyHelp /
        // SolicitationHelp. An account that also holds Admin/Staff sees everything.
        private static bool IsAlumniOnly(IList<string> roles) =>
            roles.Contains(Constants.AlumniRole) && !roles.Contains(Constants.AdminRole) && !roles.Contains(Constants.StaffRole);

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
                .AsNoTracking()
                .Include(a => a.College)
                .FirstOrDefaultAsync(m => m.AlumniId == id);
            if (alumni == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (alumni.Privacy && currentUser != null && currentUser.JagId != alumni.JagId
                && IsAlumniOnly(await _userManager.GetRolesAsync(currentUser)))
            {
                alumni.HideContactDetails();
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

            ViewData["UserRole"] = roles.Contains(Constants.AdminRole) ? Constants.AdminRole
                : roles.Contains(Constants.StaffRole) ? Constants.StaffRole
                : roles.FirstOrDefault();

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

        // POST: Alumni/BulkDelete
        // Same as DeleteConfirmed, for the rows ticked on the Index page, all
        // in one transaction. One safety difference: a linked login account
        // that also holds the Admin or Staff role (one account, multiple
        // roles) only loses its Alumni role instead of being deleted, and the
        // signed-in admin's own account is never deleted from here.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BulkDelete(int[] ids)
        {
            if (ids == null || ids.Length == 0)
            {
                TempData["ErrorMessage"] = "No alumni were selected.";
                return RedirectToAction(nameof(Index));
            }

            var alumniToDelete = await _context.Alumni.Where(a => ids.Contains(a.AlumniId)).ToListAsync();
            var currentUserId = _userManager.GetUserId(User);
            int accountsDeleted = 0;
            var accountsKept = new List<string>();

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    foreach (var alumni in alumniToDelete)
                    {
                        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.JagId == alumni.JagId);

                        // Removing the Alumni profile cascades to its history
                        // tables (Degrees, Employment, etc.) in the database.
                        _context.Alumni.Remove(alumni);

                        if (user == null) continue;

                        var isStaffOrAdmin = await _userManager.IsInRoleAsync(user, "Admin") || await _userManager.IsInRoleAsync(user, "Staff");
                        if (isStaffOrAdmin || user.Id == currentUserId)
                        {
                            if (await _userManager.IsInRoleAsync(user, "Alumni"))
                            {
                                var roleResult = await _userManager.RemoveFromRoleAsync(user, "Alumni");
                                if (!roleResult.Succeeded)
                                {
                                    throw new Exception($"Failed to remove the Alumni role from {user.UserName}.");
                                }
                            }
                            accountsKept.Add(user.UserName);
                            continue;
                        }

                        var result = await _userManager.DeleteAsync(user);
                        if (!result.Succeeded)
                        {
                            throw new Exception($"Failed to delete the login account for {alumni.JagId}.");
                        }
                        accountsDeleted++;
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    var message = $"Deleted {alumniToDelete.Count} alumni record{(alumniToDelete.Count == 1 ? "" : "s")}";
                    message += accountsDeleted > 0 ? $" and {accountsDeleted} linked login account{(accountsDeleted == 1 ? "" : "s")}." : ".";
                    if (accountsKept.Any())
                    {
                        message += $" Kept the login account{(accountsKept.Count == 1 ? "" : "s")} for {string.Join(", ", accountsKept)} (Admin/Staff) - only the Alumni role was removed.";
                    }
                    TempData["SuccessMessage"] = System.Net.WebUtility.HtmlEncode(message);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    TempData["ErrorMessage"] = "Error during bulk delete - nothing was deleted: " + ex.Message;
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
