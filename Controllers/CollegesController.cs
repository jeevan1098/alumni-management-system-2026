using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Alumni_Management_System.Data;
using Alumni_Management_System.Models;

namespace Alumni_Management_System.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CollegesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CollegesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Colleges
        public async Task<IActionResult> Index()
        {
            return View(await _context.Colleges.ToListAsync());
        }

        // GET: Colleges/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var college = await _context.Colleges
                .Include(c => c.Departments)
                .FirstOrDefaultAsync(m => m.CollegeId == id);
            if (college == null)
            {
                return NotFound();
            }

            return View(college);
        }

        // GET: Colleges/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Colleges/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CollegeId,CollegeName,IsInternal,IsActive")] College college)
        {
            // College names must be unique (case-insensitive), so pickers and
            // reports never show two identical colleges.
            if (!string.IsNullOrWhiteSpace(college.CollegeName)
                && await _context.Colleges.AnyAsync(c => c.CollegeName.Trim() == college.CollegeName.Trim() && c.CollegeId != college.CollegeId))
            {
                ModelState.AddModelError(nameof(College.CollegeName), "A college with this name already exists.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(college);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(college);
        }

        // GET: Colleges/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var college = await _context.Colleges.FindAsync(id);
            if (college == null)
            {
                return NotFound();
            }
            return View(college);
        }

        // POST: Colleges/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CollegeId,CollegeName,IsInternal,IsActive")] College college)
        {
            if (id != college.CollegeId)
            {
                return NotFound();
            }

            // College names must be unique (case-insensitive), so pickers and
            // reports never show two identical colleges.
            if (!string.IsNullOrWhiteSpace(college.CollegeName)
                && await _context.Colleges.AnyAsync(c => c.CollegeName.Trim() == college.CollegeName.Trim() && c.CollegeId != college.CollegeId))
            {
                ModelState.AddModelError(nameof(College.CollegeName), "A college with this name already exists.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(college);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CollegeExists(college.CollegeId))
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
            return View(college);
        }

        // GET: Colleges/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var college = await _context.Colleges
                .FirstOrDefaultAsync(m => m.CollegeId == id);
            if (college == null)
            {
                return NotFound();
            }

            var inUse = await InUseReasonAsync(college);
            if (inUse != null)
            {
                TempData["ErrorMessage"] = inUse;
                return RedirectToAction(nameof(Index));
            }

            return View(college);
        }

        // POST: Colleges/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var college = await _context.Colleges.FindAsync(id);
            if (college != null)
            {
                var inUse = await InUseReasonAsync(college);
                if (inUse != null)
                {
                    TempData["ErrorMessage"] = inUse;
                    return RedirectToAction(nameof(Index));
                }

                _context.Colleges.Remove(college);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Why this college can't be deleted yet, or null if nothing points to
        // it. The database refuses the delete while any of these exist.
        private async Task<string> InUseReasonAsync(College college)
        {
            var inUse = Services.InUseMessage.Describe(
                (await _context.Departments.CountAsync(d => d.CollegeId == college.CollegeId), "department", "departments"),
                (await _context.Alumni.CountAsync(a => a.CollegeId == college.CollegeId), "alumnus", "alumni"),
                (await _context.StudentOrganizations.CountAsync(o => o.CollegeId == college.CollegeId), "student organization", "student organizations"),
                (await _context.UserAccessScopes.CountAsync(s => s.CollegeId == college.CollegeId), "user access scope", "user access scopes"));
            return inUse == null ? null
                : $"{college.CollegeName} can't be deleted - it still has {inUse}. Move or remove those first, or mark the college inactive.";
        }

        private bool CollegeExists(int id)
        {
            return _context.Colleges.Any(e => e.CollegeId == id);
        }
    }
}
