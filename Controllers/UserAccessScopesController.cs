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
    // Lets an Admin grant a user's role either system-wide access or access
    // scoped to one college (and optionally one department within it).
    [Authorize(Roles = "Admin")]
    public class UserAccessScopesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserAccessScopesController(ApplicationDbContext context, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: UserAccessScopes
        public async Task<IActionResult> Index()
        {
            var scopes = await _context.UserAccessScopes
                .Include(s => s.User)
                .Include(s => s.Role)
                .Include(s => s.College)
                .Include(s => s.Department)
                .ToListAsync();
            return View(scopes);
        }

        // GET: UserAccessScopes/Create
        public async Task<IActionResult> Create()
        {
            await PopulateDropdownsAsync();
            return View();
        }

        // POST: UserAccessScopes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserAccessScopeId,UserId,RoleId,CollegeId,DepartmentId,AccessLevel,IsActive")] UserAccessScope userAccessScope)
        {
            // System-wide access has no college/department attached.
            if (userAccessScope.CollegeId == null)
            {
                userAccessScope.DepartmentId = null;
            }

            if (ModelState.IsValid)
            {
                _context.Add(userAccessScope);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Access scope granted successfully!";
                return RedirectToAction(nameof(Index));
            }
            await PopulateDropdownsAsync(userAccessScope);
            return View(userAccessScope);
        }

        // GET: UserAccessScopes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userAccessScope = await _context.UserAccessScopes.FindAsync(id);
            if (userAccessScope == null)
            {
                return NotFound();
            }
            await PopulateDropdownsAsync(userAccessScope);
            return View(userAccessScope);
        }

        // POST: UserAccessScopes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserAccessScopeId,UserId,RoleId,CollegeId,DepartmentId,AccessLevel,IsActive")] UserAccessScope userAccessScope)
        {
            if (id != userAccessScope.UserAccessScopeId)
            {
                return NotFound();
            }

            if (userAccessScope.CollegeId == null)
            {
                userAccessScope.DepartmentId = null;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(userAccessScope);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserAccessScopeExists(userAccessScope.UserAccessScopeId))
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
            await PopulateDropdownsAsync(userAccessScope);
            return View(userAccessScope);
        }

        // GET: UserAccessScopes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userAccessScope = await _context.UserAccessScopes
                .Include(s => s.User)
                .Include(s => s.Role)
                .Include(s => s.College)
                .Include(s => s.Department)
                .FirstOrDefaultAsync(m => m.UserAccessScopeId == id);
            if (userAccessScope == null)
            {
                return NotFound();
            }

            return View(userAccessScope);
        }

        // POST: UserAccessScopes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userAccessScope = await _context.UserAccessScopes.FindAsync(id);
            if (userAccessScope != null)
            {
                _context.UserAccessScopes.Remove(userAccessScope);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserAccessScopeExists(int id)
        {
            return _context.UserAccessScopes.Any(e => e.UserAccessScopeId == id);
        }

        private async Task PopulateDropdownsAsync(UserAccessScope current = null)
        {
            var users = _userManager.Users.OrderBy(u => u.UserName).ToList();
            ViewData["UserId"] = new SelectList(users, "Id", "UserName", current?.UserId);

            var roles = _roleManager.Roles.OrderBy(r => r.Name).ToList();
            ViewData["RoleId"] = new SelectList(roles, "Id", "Name", current?.RoleId);

            ViewData["CollegeId"] = new SelectList(await _context.Colleges.Where(c => c.IsActive).ToListAsync(), "CollegeId", "CollegeName", current?.CollegeId);
            ViewData["DepartmentId"] = new SelectList(await _context.Departments.Where(d => d.IsActive).ToListAsync(), "DepartmentId", "DepartmentName", current?.DepartmentId);
        }
    }
}
