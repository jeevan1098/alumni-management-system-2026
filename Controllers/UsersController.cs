using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Alumni_Management_System.Data;
using Alumni_Management_System.Models;
using Alumni_Management_System.Models.ViewModels;

namespace Alumni_Management_System.Controllers
{
    // Admin-only screen to create Staff/Admin accounts directly. Alumni
    // accounts are intentionally NOT created here - they go through the
    // existing Alumni Registry -> VerifyJagId -> RegisterAlumni self-service
    // flow, since an Alumni account must link to a real Alumni/JagId record.
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<UsersController> _logger;

        public UsersController(ApplicationDbContext context, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, IEmailSender emailSender, ILogger<UsersController> logger)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.OrderBy(u => u.UserName).ToListAsync();
            var rolesByUserId = new Dictionary<string, IList<string>>();
            foreach (var user in users)
            {
                rolesByUserId[user.Id] = await _userManager.GetRolesAsync(user);
            }
            ViewData["RolesByUserId"] = rolesByUserId;

            // A user can be scoped to several colleges/departments - one row each.
            var scopesByUserId = (await _context.UserAccessScopes
                    .Include(s => s.College)
                    .Include(s => s.Department)
                    .Include(s => s.Role)
                    .Where(s => s.IsActive)
                    .ToListAsync())
                .GroupBy(s => s.UserId)
                .ToDictionary(g => g.Key, g => g.ToList());
            ViewData["ScopesByUserId"] = scopesByUserId;

            // A limited admin only sees the accounts they're allowed to
            // manage (see IsWithinScope) - never themselves, full admins or
            // people outside their colleges/departments.
            var actorScope = await GetActorScopeAsync();
            if (actorScope != null)
            {
                var actorId = _userManager.GetUserId(User);
                var collegeOfDepartment = await GetCollegeOfDepartmentAsync();
                var visibleAlumniJagIds = (await Services.AccessScopeService
                        .ApplyTo(_context.Alumni, actorScope)
                        .Select(a => a.JagId)
                        .ToListAsync())
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                users = users
                    .Where(u => u.Id != actorId && IsWithinScope(
                        rolesByUserId[u.Id],
                        scopesByUserId.GetValueOrDefault(u.Id),
                        u.JagId != null && visibleAlumniJagIds.Contains(u.JagId),
                        actorScope,
                        collegeOfDepartment))
                    .ToList();
            }

            await PopulateScopeViewDataAsync(actorScope);

            return View(users);
        }

        // POST: Users/SetScope - inline scope management from Manage Users,
        // so an admin doesn't have to leave this page to grant/change one.
        // Nothing ticked = system-wide.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetScope(string userId, int[] collegeIds, int[] departmentIds, string accessLevel)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var actorScope = await GetActorScopeAsync();
            if (!await CanManageAsync(user, actorScope))
            {
                return OutsideScope();
            }

            collegeIds = (collegeIds ?? Array.Empty<int>()).Distinct().ToArray();
            departmentIds = (departmentIds ?? Array.Empty<int>()).Distinct().ToArray();
            var scopeError = await ValidateGrantAsync(collegeIds, departmentIds, actorScope);
            if (scopeError != null)
            {
                TempData["ErrorMessage"] = scopeError;
                return RedirectToAction(nameof(Index));
            }

            var roles = await _userManager.GetRolesAsync(user);
            var primaryRole = roles.FirstOrDefault(r => r == Constants.AdminRole || r == Constants.StaffRole);
            if (primaryRole == null)
            {
                TempData["ErrorMessage"] = "Access scopes only apply to Admin/Staff accounts.";
                return RedirectToAction(nameof(Index));
            }

            await ReplaceScopesAsync(userId, primaryRole, collegeIds, departmentIds, accessLevel);

            TempData["SuccessMessage"] = $"Access scope updated for {user.UserName}.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Users/RemoveScope - clears every scope row, which makes the
        // user unrestricted, so only an unrestricted admin may do it.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveScope(string userId)
        {
            if (await GetActorScopeAsync() != null)
            {
                TempData["ErrorMessage"] = "Your own access is limited, so you can't remove a scope - that would give the user system-wide access.";
                return RedirectToAction(nameof(Index));
            }

            var scopes = await _context.UserAccessScopes.Where(s => s.UserId == userId).ToListAsync();
            if (scopes.Count > 0)
            {
                _context.UserAccessScopes.RemoveRange(scopes);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Access scope removed.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Users/AddRole - grants an ADDITIONAL role to an existing
        // account (e.g. an Alumni who is also becoming Staff), so a person
        // never needs a second account for the same JAG ID.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRole(string userId, string role)
        {
            if (role != Constants.AdminRole && role != Constants.StaffRole)
            {
                TempData["ErrorMessage"] = "Role must be Admin or Staff.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var actorScope = await GetActorScopeAsync();
            if (!await CanManageAsync(user, actorScope))
            {
                return OutsideScope();
            }

            if (await _userManager.IsInRoleAsync(user, role))
            {
                TempData["ErrorMessage"] = $"{user.UserName} already has the {role} role.";
                return RedirectToAction(nameof(Index));
            }

            await _userManager.AddToRoleAsync(user, role);
            TempData["SuccessMessage"] = $"{role} role added for {user.UserName}.";

            // No scope rows = system-wide. A limited admin can't hand that
            // out, so the account starts with the admin's own scope (they
            // can narrow it afterwards with Manage Scope).
            if (actorScope != null && !await _context.UserAccessScopes.AnyAsync(s => s.UserId == user.Id && s.IsActive))
            {
                await ReplaceScopesAsync(user.Id, role, actorScope.CollegeIds, actorScope.DepartmentIds, "Full");
                TempData["SuccessMessage"] += " Their access is limited to your own scope - use Manage Scope to narrow it.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Users/RemoveRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveRole(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            if (!await CanManageAsync(user, await GetActorScopeAsync()))
            {
                return OutsideScope();
            }

            if (user.UserName == "admin" && role == Constants.AdminRole)
            {
                TempData["ErrorMessage"] = "The default admin account must keep the Admin role.";
                return RedirectToAction(nameof(Index));
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Count <= 1)
            {
                TempData["ErrorMessage"] = $"Can't remove {user.UserName}'s last role - they'd be left without any access.";
                return RedirectToAction(nameof(Index));
            }

            await _userManager.RemoveFromRoleAsync(user, role);

            // Scope rows hang off a role. Move the removed role's rows to the
            // Admin/Staff role the user still holds - otherwise they'd stop
            // counting and the user would silently become system-wide.
            var removedRole = await _roleManager.FindByNameAsync(role);
            var orphanedScopes = removedRole == null
                ? new List<UserAccessScope>()
                : await _context.UserAccessScopes.Where(s => s.UserId == user.Id && s.RoleId == removedRole.Id).ToListAsync();
            if (orphanedScopes.Count > 0)
            {
                var remainingRoleName = currentRoles.FirstOrDefault(r => r != role && (r == Constants.AdminRole || r == Constants.StaffRole));
                var remainingRole = remainingRoleName == null ? null : await _roleManager.FindByNameAsync(remainingRoleName);
                if (remainingRole == null)
                {
                    _context.UserAccessScopes.RemoveRange(orphanedScopes);
                }
                else
                {
                    foreach (var scope in orphanedScopes)
                    {
                        scope.RoleId = remainingRole.Id;
                    }
                }
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = $"{role} role removed from {user.UserName}.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Users/Create
        public async Task<IActionResult> Create()
        {
            await PopulateScopeViewDataAsync(await GetActorScopeAsync());
            return View(new CreateUserViewModel { JagId = await GenerateStaffJagIdAsync() });
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            var actorScope = await GetActorScopeAsync();
            await PopulateScopeViewDataAsync(actorScope);

            if (model.Role != Constants.AdminRole && model.Role != Constants.StaffRole)
            {
                ModelState.AddModelError(nameof(model.Role), "Role must be Admin or Staff.");
            }

            // The scope is picked up front: an account with no scope is
            // system-wide, which a limited admin must never be able to create.
            var collegeIds = (model.CollegeIds ?? new List<int>()).Distinct().ToArray();
            var departmentIds = (model.DepartmentIds ?? new List<int>()).Distinct().ToArray();
            var hasScope = collegeIds.Length > 0 || departmentIds.Length > 0;
            var scopeError = await ValidateGrantAsync(collegeIds, departmentIds, actorScope);
            if (scopeError != null)
            {
                ModelState.AddModelError(nameof(model.CollegeIds), scopeError);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // One JagId -> one account: if this JagId already has an account
            // (of any role), grant it the newly requested role instead of
            // creating a second account for the same person - same outcome
            // as clicking "Add Role" for them on the Manage Users page.
            var existingJagId = await _userManager.Users.FirstOrDefaultAsync(u => u.JagId == model.JagId);
            if (existingJagId != null)
            {
                if (!await CanManageAsync(existingJagId, actorScope))
                {
                    ModelState.AddModelError(nameof(model.JagId), "This JAG ID belongs to an account outside your access scope.");
                    return View(model);
                }

                if (await _userManager.IsInRoleAsync(existingJagId, model.Role))
                {
                    ModelState.AddModelError(nameof(model.JagId), $"{existingJagId.UserName} already has the {model.Role} role for this JAG ID.");
                    return View(model);
                }

                await _userManager.AddToRoleAsync(existingJagId, model.Role);

                // An account that already has a scope keeps it; one without
                // (e.g. an Alumni becoming Staff) gets the scope chosen here.
                if (hasScope && !await _context.UserAccessScopes.AnyAsync(s => s.UserId == existingJagId.Id && s.IsActive))
                {
                    await ReplaceScopesAsync(existingJagId.Id, model.Role, collegeIds, departmentIds, "Full");
                }

                TempData["SuccessMessage"] = $"An account already exists for this JAG ID ({existingJagId.UserName}) - added the {model.Role} role to it instead of creating a new account.";
                return RedirectToAction(nameof(Index));
            }

            // Past this point we're actually creating a brand-new account,
            // so Email/Password (optional on the model - see
            // CreateUserViewModel) become required.
            if (string.IsNullOrWhiteSpace(model.Email))
            {
                ModelState.AddModelError(nameof(model.Email), "Email is required to create a new account.");
            }
            if (string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError(nameof(model.Password), "A temporary password is required to create a new account.");
            }
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Two accounts sharing an email crashes any code path that does
            // an email lookup expecting a single match (e.g. Forgot
            // Password) - block it here rather than let it happen silently.
            var normalizedEmail = _userManager.NormalizeEmail(model.Email);
            if (await _userManager.Users.AnyAsync(u => u.NormalizedEmail == normalizedEmail))
            {
                ModelState.AddModelError(nameof(model.Email), "This email is already used by another account.");
                return View(model);
            }

            // Username is a placeholder, not chosen by the admin - the new
            // hire picks their real username during the forced first-login
            // setup flow (see AccountController.CompleteSetup).
            var placeholderUsername = $"pending_{model.JagId}";

            var user = new AppUser
            {
                UserName = placeholderUsername,
                Email = model.Email,
                EmailConfirmed = true,
                JagId = model.JagId,
                CreatedAt = DateTime.Now,
                IsFirstLogin = true,
                TwoFactorEnabled = false // opt-in - user can enable it later in Two-Factor Auth settings
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            await _userManager.AddToRoleAsync(user, model.Role);

            if (hasScope)
            {
                await ReplaceScopesAsync(user.Id, model.Role, collegeIds, departmentIds, "Full");
            }

            try
            {
                await _emailSender.SendEmailAsync(
                    user.Email,
                    "Your Alumni Management System account",
                    $"<p>A {model.Role} account has been created for you.</p>" +
                    $"<p><strong>Temporary username:</strong> {placeholderUsername}<br/>" +
                    $"<strong>Temporary password:</strong> {model.Password}</p>" +
                    "<p>Log in with these at the site, then you'll be asked to choose your own username and password.</p>");
            }
            catch (Exception ex)
            {
                // The account exists and the admin knows the temp password
                // they just typed, so keep it and let them pass it on.
                _logger.LogError(ex, "Sending the new-account email to '{Email}' failed.", user.Email);
                TempData["ErrorMessage"] = $"{model.Role} account created, but the email to {user.Email} could not be sent. Give them the temporary username '{placeholderUsername}' and the temporary password you entered.";
                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] = $"{model.Role} account created. Temporary login credentials were emailed to {user.Email}.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            if (!await CanManageAsync(user, await GetActorScopeAsync()))
            {
                return OutsideScope();
            }

            var inUse = await InUseReasonAsync(user);
            if (inUse != null)
            {
                TempData["ErrorMessage"] = inUse;
                return RedirectToAction(nameof(Index));
            }

            var roles = await _userManager.GetRolesAsync(user);
            ViewData["Roles"] = string.Join(", ", roles);
            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            if (!await CanManageAsync(user, await GetActorScopeAsync()))
            {
                return OutsideScope();
            }

            if (user.UserName == "admin")
            {
                TempData["ErrorMessage"] = "The default admin account cannot be deleted.";
                return RedirectToAction(nameof(Index));
            }

            var inUse = await InUseReasonAsync(user);
            if (inUse != null)
            {
                TempData["ErrorMessage"] = inUse;
                return RedirectToAction(nameof(Index));
            }

            await _userManager.DeleteAsync(user);

            // Their alumni profile (if any) stays, so reopen their Registry
            // entry - otherwise they could never register a login again.
            var registry = await _context.AlumniRegistries.FirstOrDefaultAsync(r => r.JagId == user.JagId && r.AccountCreated);
            if (registry != null)
            {
                registry.AccountCreated = false;
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "User account deleted.";
            return RedirectToAction(nameof(Index));
        }

        // Messages keep their author, so a login that wrote any can't be deleted.
        private async Task<string> InUseReasonAsync(AppUser user)
        {
            var written = await _context.Messages.CountAsync(m => m.CreatedBy == user.Id);
            return written == 0 ? null
                : $"{user.UserName} can't be deleted - they wrote {written} message{(written == 1 ? "" : "s")}, which keep them as the author.";
        }

        // POST: Users/ResetPassword - triggered by an admin after a
        // forgot-password request (see AccountController.ForgotPassword).
        // Generates a fresh temp password, forces the user to change it on
        // next sign-in, and emails it to them directly (the admin never sees
        // it, same as the temp password on account creation).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            if (!await CanManageAsync(user, await GetActorScopeAsync()))
            {
                return OutsideScope();
            }

            // Remembered so the reset can be undone if the email below doesn't
            // go out - otherwise the old password stops working and the new
            // one never reaches the user, locking them out.
            var previousPasswordHash = user.PasswordHash;
            var previousMustChangePassword = user.MustChangePassword;

            var tempPassword = Services.PasswordGenerator.GenerateTempPassword();
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, tempPassword);

            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = "Could not reset password: " + string.Join(" ", result.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(Index));
            }

            user.MustChangePassword = true;
            await _userManager.UpdateAsync(user);

            try
            {
                await _emailSender.SendEmailAsync(
                    user.Email,
                    "Your password has been reset - Alumni Management System",
                    $"<p>Your administrator reset your password.</p>" +
                    $"<p><strong>Temporary password:</strong> {tempPassword}</p>" +
                    $"<p>Log in with your existing username (<strong>{user.UserName}</strong>) and this temporary password - you'll be asked to set your own right after.</p>");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sending the temporary password email to '{UserName}' failed; restoring their previous password.", user.UserName);
                user.PasswordHash = previousPasswordHash;
                user.MustChangePassword = previousMustChangePassword;
                await _userManager.UpdateAsync(user);
                TempData["ErrorMessage"] = $"The email to {user.Email} could not be sent, so the password for {user.UserName} was NOT changed. Check the email settings and try again.";
                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] = $"Password reset for {user.UserName}. A temporary password was emailed to {user.Email}.";
            return RedirectToAction(nameof(Index));
        }

        // ---- Access-scope rules for Manage Users ----
        // An admin can only hand out access they hold themselves: a limited
        // admin grants colleges/departments inside their own scope, never
        // system-wide, can't touch their own scope, and only manages accounts
        // whose access sits inside theirs. An unrestricted admin can do
        // everything. A department is inside a scope when the scope holds that
        // department or its whole college.

        // The signed-in admin's scope, or null if unrestricted.
        private async Task<Services.AccessScope> GetActorScopeAsync()
        {
            var actor = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(actor);
            return await Services.AccessScopeService.GetScopeAsync(_context, actor, roles);
        }

        private Task<Dictionary<int, int>> GetCollegeOfDepartmentAsync() =>
            _context.Departments.ToDictionaryAsync(d => d.DepartmentId, d => d.CollegeId);

        // Whether an account falls inside a limited admin's scope. Admin/Staff
        // accounts must themselves be limited to a subset of it; Alumni-only
        // accounts count when their profile is one the admin can see (same
        // rule as the Alumni list - see AccessScopeService.ApplyTo).
        private static bool IsWithinScope(IList<string> targetRoles, IEnumerable<UserAccessScope> targetScopes, bool alumniProfileVisible,
            Services.AccessScope actorScope, IReadOnlyDictionary<int, int> collegeOfDepartment)
        {
            if (targetRoles.Contains(Constants.AdminRole) || targetRoles.Contains(Constants.StaffRole))
            {
                var targetScope = Services.AccessScopeService.Resolve(targetScopes ?? Enumerable.Empty<UserAccessScope>(), targetRoles);
                return targetScope != null
                    && Services.AccessScopeService.Covers(actorScope, targetScope.CollegeIds, targetScope.DepartmentIds, collegeOfDepartment);
            }

            return alumniProfileVisible;
        }

        private async Task<bool> CanManageAsync(AppUser target, Services.AccessScope actorScope)
        {
            if (actorScope == null)
            {
                return true;
            }

            if (target.Id == _userManager.GetUserId(User))
            {
                return false;
            }

            var roles = await _userManager.GetRolesAsync(target);
            var scopes = await _context.UserAccessScopes
                .Include(s => s.Role)
                .Where(s => s.UserId == target.Id && s.IsActive)
                .ToListAsync();
            var alumniProfileVisible = await Services.AccessScopeService
                .ApplyTo(_context.Alumni.Where(a => a.JagId == target.JagId), actorScope)
                .AnyAsync();

            return IsWithinScope(roles, scopes, alumniProfileVisible, actorScope, await GetCollegeOfDepartmentAsync());
        }

        private IActionResult OutsideScope()
        {
            TempData["ErrorMessage"] = "You can only manage accounts whose access is inside your own scope, and you can't change your own.";
            return RedirectToAction(nameof(Index));
        }

        // Returns an error message when the signed-in admin isn't allowed to
        // grant these colleges/departments (both empty = system-wide),
        // otherwise null. External isn't grantable - it's visible to everyone.
        private async Task<string> ValidateGrantAsync(int[] collegeIds, int[] departmentIds, Services.AccessScope actorScope)
        {
            if (actorScope != null && collegeIds.Length == 0 && departmentIds.Length == 0)
            {
                return "Your own access is limited, so you must pick at least one college or department from your scope - you can't grant system-wide access.";
            }

            if (await _context.Colleges.CountAsync(c => collegeIds.Contains(c.CollegeId) && c.IsInternal) != collegeIds.Length
                || await _context.Departments.CountAsync(d => departmentIds.Contains(d.DepartmentId) && d.College.IsInternal) != departmentIds.Length)
            {
                return "One of the selected colleges or departments doesn't exist or can't be granted (External is already visible to everyone).";
            }

            if (!Services.AccessScopeService.Covers(actorScope, collegeIds, departmentIds, await GetCollegeOfDepartmentAsync()))
            {
                return "You can only grant colleges and departments inside your own scope.";
            }

            return null;
        }

        // Replaces all of a user's scope rows with one row per whole college
        // plus one per department, or a single system-wide row when none are
        // given. A department whose whole college is also picked is dropped -
        // the college already includes it.
        private async Task ReplaceScopesAsync(string userId, string roleName, IEnumerable<int> collegeIds, IEnumerable<int> departmentIds, string accessLevel)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            var level = string.IsNullOrWhiteSpace(accessLevel) ? "Full" : accessLevel;
            var colleges = collegeIds.Distinct().ToList();
            var collegeOfDepartment = await GetCollegeOfDepartmentAsync();

            _context.UserAccessScopes.RemoveRange(await _context.UserAccessScopes.Where(s => s.UserId == userId).ToListAsync());

            var rows = colleges
                .Select(c => new UserAccessScope { CollegeId = c })
                .Concat(departmentIds.Distinct()
                    .Where(d => !colleges.Contains(collegeOfDepartment[d]))
                    .Select(d => new UserAccessScope { CollegeId = collegeOfDepartment[d], DepartmentId = d }))
                .ToList();
            if (rows.Count == 0)
            {
                rows.Add(new UserAccessScope());
            }

            foreach (var row in rows)
            {
                row.UserId = userId;
                row.RoleId = role.Id;
                row.AccessLevel = level;
                row.IsActive = true;
                _context.UserAccessScopes.Add(row);
            }

            await _context.SaveChangesAsync();
        }

        // The college/department tree the signed-in admin may grant, for the
        // scope pickers. University colleges only - External needs no grant.
        private async Task PopulateScopeViewDataAsync(Services.AccessScope actorScope)
        {
            var colleges = await _context.Colleges
                .Where(c => c.IsActive && c.IsInternal)
                .Include(c => c.Departments)
                .OrderBy(c => c.CollegeName)
                .ToListAsync();

            var options = new List<ScopeCollegeOption>();
            foreach (var college in colleges)
            {
                var wholeCollege = actorScope == null || actorScope.CollegeIds.Contains(college.CollegeId);
                var departments = college.Departments
                    .Where(d => d.IsActive && (wholeCollege || actorScope.DepartmentIds.Contains(d.DepartmentId)))
                    .OrderBy(d => d.DepartmentName)
                    .Select(d => new ScopeDepartmentOption { DepartmentId = d.DepartmentId, DepartmentName = d.DepartmentName })
                    .ToList();

                if (wholeCollege || departments.Count > 0)
                {
                    options.Add(new ScopeCollegeOption
                    {
                        CollegeId = college.CollegeId,
                        CollegeName = college.CollegeName,
                        CanGrantWholeCollege = wholeCollege,
                        Departments = departments
                    });
                }
            }

            ViewData["ScopeOptions"] = options;
            ViewData["ActorIsLimited"] = actorScope != null;
            ViewData["ActorScopeLabel"] = await Services.AccessScopeService.DescribeAsync(_context, actorScope);
        }

        private async Task<string> GenerateStaffJagIdAsync()
        {
            var existingIds = await _userManager.Users
                .Select(u => u.JagId)
                .Where(j => j != null && j.StartsWith("J00"))
                .ToListAsync();

            var maxNumber = existingIds
                .Select(j => int.TryParse(j.Substring(3), out var n) ? n : 0)
                .DefaultIfEmpty(0)
                .Max();

            return $"J00{(maxNumber + 1):D6}";
        }
    }
}
