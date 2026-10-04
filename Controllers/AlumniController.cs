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
using Alumni_Management_System.Services;
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
                // to specific colleges/departments (see AccessScopeService.ApplyTo).
                var scope = await Services.AccessScopeService.GetScopeAsync(_context, currentUser, roles);
                if (scope != null)
                {
                    alumniQuery = Services.AccessScopeService.ApplyTo(alumniQuery, scope);
                    ViewData["ScopeLabel"] = await Services.AccessScopeService.DescribeAsync(_context, scope);
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

        // Whether a scoped Staff/Admin may open this alumnus at all - the same
        // rule as the list, so a profile outside their scope can't be reached
        // just by typing its URL. Alumni-only accounts browse the whole
        // directory and are unaffected.
        private async Task<bool> IsInViewerScopeAsync(AppUser viewer, IList<string> roles, int alumniId)
        {
            if (IsAlumniOnly(roles))
            {
                return true;
            }

            var scope = await Services.AccessScopeService.GetScopeAsync(_context, viewer, roles);
            return scope == null
                || await _context.Alumni.AnyAsync(a => a.AlumniId == alumniId && a.JagId == viewer.JagId) // their own profile
                || await Services.AccessScopeService.ApplyTo(_context.Alumni.Where(a => a.AlumniId == alumniId), scope).AnyAsync();
        }

        // Who may edit a profile: its owner (any account holding the Alumni
        // role, even if it also has Staff/Admin), or an Admin within their
        // scope. Staff are otherwise read-only. Decided from the stored JAG
        // ID, never the one posted in the form. Returns null when allowed.
        private async Task<IActionResult> CheckCanEditAsync(AppUser user, IList<string> roles, int alumniId, string storedJagId)
        {
            if (roles.Contains(Constants.AlumniRole) && storedJagId == user.JagId)
            {
                return null;
            }

            if (!roles.Contains(Constants.AdminRole))
            {
                TempData["ErrorMessage"] = roles.Contains(Constants.StaffRole)
                    ? "Staff members have read-only access."
                    : "You can only edit your own profile.";
                return RedirectToAction(nameof(Index));
            }

            return await IsInViewerScopeAsync(user, roles, alumniId) ? null : OutsideScope();
        }

        private IActionResult OutsideScope()
        {
            TempData["ErrorMessage"] = "That alumnus is outside your access scope.";
            return RedirectToAction(nameof(Index));
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
                .AsNoTracking()
                .Include(a => a.College)
                .FirstOrDefaultAsync(m => m.AlumniId == id);
            if (alumni == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var currentRoles = currentUser == null ? new List<string>() : await _userManager.GetRolesAsync(currentUser);
            if (currentUser != null && !await IsInViewerScopeAsync(currentUser, currentRoles, alumni.AlumniId))
            {
                return OutsideScope();
            }

            // Edit button: the owner, or an Admin (already limited to their
            // scope above). Settings/status: everyone except other alumni.
            var isOwner = currentUser != null && currentUser.JagId == alumni.JagId && currentRoles.Contains(Constants.AlumniRole);
            ViewData["CanEdit"] = isOwner || currentRoles.Contains(Constants.AdminRole);
            ViewData["ShowSettings"] = isOwner || !IsAlumniOnly(currentRoles);

            if (alumni.Privacy && currentUser != null && currentUser.JagId != alumni.JagId
                && IsAlumniOnly(currentRoles))
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
            UseClearGraduationYearMessage();

            // JAG ID is unique - say so on the field instead of letting the
            // database's unique rule crash the page.
            if (!string.IsNullOrWhiteSpace(alumni.JagId) && await _context.Alumni.AnyAsync(a => a.JagId == alumni.JagId))
            {
                ModelState.AddModelError(nameof(Alumni.JagId), "An alumni profile with this JAG ID already exists.");
            }

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

            var denied = await CheckCanEditAsync(currentUser, roles, alumni.AlumniId, alumni.JagId);
            if (denied != null)
            {
                return denied;
            }

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

            var storedJagId = await _context.Alumni
                .Where(a => a.AlumniId == id)
                .Select(a => a.JagId)
                .FirstOrDefaultAsync();
            if (storedJagId == null)
            {
                return NotFound();
            }

            var denied = await CheckCanEditAsync(currentUser, roles, id, storedJagId);
            if (denied != null)
            {
                return denied;
            }

            UseClearGraduationYearMessage();

            // JAG ID is read-only on this form, so the stored value always
            // wins over whatever was posted. Records created before the fixed
            // format was enforced may not match it - don't let that block
            // saving the rest of the profile.
            alumni.JagId = storedJagId;
            ModelState.Remove(nameof(Alumni.JagId));

            // Fields shown read-only to anyone but an Admin (name comes from
            // the Registry, graduation year and college from their degrees,
            // active flag is set by the office) - keep the stored values so
            // they can't be changed by editing the form before submitting.
            if (!roles.Contains(Constants.AdminRole))
            {
                var stored = await _context.Alumni.AsNoTracking()
                    .Where(a => a.AlumniId == id)
                    .Select(a => new { a.FirstName, a.LastName, a.GraduationYear, a.IsActive, a.CollegeId })
                    .FirstAsync();
                alumni.FirstName = stored.FirstName;
                alumni.LastName = stored.LastName;
                alumni.GraduationYear = stored.GraduationYear;
                alumni.IsActive = stored.IsActive;
                alumni.CollegeId = stored.CollegeId;
                foreach (var field in new[] { nameof(Alumni.FirstName), nameof(Alumni.LastName), nameof(Alumni.GraduationYear), nameof(Alumni.IsActive), nameof(Alumni.CollegeId) })
                {
                    ModelState.Remove(field);
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    alumni.LastUpdated = DateTime.Now;
                    _context.Update(alumni);

                    // Mark first login as complete once Alumni users save their own profile
                    if (roles.Contains(Constants.AlumniRole) && storedJagId == currentUser.JagId && currentUser.IsFirstLogin)
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

            var currentUser = await _userManager.GetUserAsync(User);
            if (!await IsInViewerScopeAsync(currentUser, await _userManager.GetRolesAsync(currentUser), alumni.AlumniId))
            {
                return OutsideScope();
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

            var currentUser = await _userManager.GetUserAsync(User);
            if (!await IsInViewerScopeAsync(currentUser, await _userManager.GetRolesAsync(currentUser), alumni.AlumniId))
            {
                return OutsideScope();
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
                    await ReopenRegistryEntriesAsync(new[] { alumni.JagId });

                    // 4. Remove the linked Identity User if they have one
                    // This is the "Reverse Cascade" manual step. Same rule as
                    // BulkDelete: an account that also holds Admin/Staff (or is
                    // the signed-in admin's own) only loses its Alumni role.
                    var keptAccount = false;
                    if (user != null)
                    {
                        if (await _userManager.IsInRoleAsync(user, Constants.AdminRole)
                            || await _userManager.IsInRoleAsync(user, Constants.StaffRole)
                            || user.Id == currentUser.Id)
                        {
                            if (await _userManager.IsInRoleAsync(user, Constants.AlumniRole))
                            {
                                var roleResult = await _userManager.RemoveFromRoleAsync(user, Constants.AlumniRole);
                                if (!roleResult.Succeeded)
                                {
                                    throw new Exception($"Failed to remove the Alumni role from {user.UserName}.");
                                }
                            }
                            keptAccount = true;
                        }
                        else
                        {
                            var result = await _userManager.DeleteAsync(user);
                            if (!result.Succeeded)
                            {
                                throw new Exception("Failed to delete associated user account.");
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["SuccessMessage"] = user == null ? "Alumni record deleted successfully!"
                        : keptAccount ? $"Alumni record deleted. The login account {user.UserName} also has Admin/Staff access, so it was kept - only its Alumni role was removed."
                        : "Alumni and associated user account deleted successfully!";
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

            // A scoped admin can only delete alumni inside their scope - ids
            // posted for anyone else are ignored, not trusted from the form.
            var deleter = await _userManager.GetUserAsync(User);
            var visibleAlumni = await Services.AccessScopeService.GetVisibleAlumniAsync(_context, deleter, await _userManager.GetRolesAsync(deleter));
            var alumniToDelete = await visibleAlumni.Where(a => ids.Contains(a.AlumniId)).ToListAsync();
            var skippedOutsideScope = ids.Distinct().Count() - alumniToDelete.Count;
            var currentUserId = _userManager.GetUserId(User);
            int accountsDeleted = 0;
            var accountsKept = new List<string>();

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    await ReopenRegistryEntriesAsync(alumniToDelete.Select(a => a.JagId));

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
                    if (skippedOutsideScope > 0)
                    {
                        message += $" Skipped {skippedOutsideScope} that {(skippedOutsideScope == 1 ? "is" : "are")} outside your access scope or no longer exist.";
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

        // A blank Graduation Year can't bind to the non-nullable int, so MVC
        // reports "The value '' is invalid." - swap in the readable message.
        private void UseClearGraduationYearMessage() =>
            ModelState.UseClearBlankMessage(nameof(Alumni.GraduationYear), Alumni.GraduationYearRequiredMessage);

        // A deleted alumnus stays on the Registry (the university's list of
        // graduates) but is marked as having no account, so they can register
        // again through Verify JAG ID.
        private async Task ReopenRegistryEntriesAsync(IEnumerable<string> jagIds)
        {
            var ids = jagIds.ToList();
            var entries = await _context.AlumniRegistries.Where(r => ids.Contains(r.JagId) && r.AccountCreated).ToListAsync();
            foreach (var entry in entries)
            {
                entry.AccountCreated = false;
            }
        }

        private bool AlumniExists(int id)
        {
            return _context.Alumni.Any(e => e.AlumniId == id);
        }
    }
}
