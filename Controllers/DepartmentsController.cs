using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Alumni_Management_System.Data;
using Alumni_Management_System.Models;

namespace Alumni_Management_System.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DepartmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DepartmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Departments
        public async Task<IActionResult> Index()
        {
            return View(await _context.Departments.Include(d => d.College).ToListAsync());
        }

        // GET: Departments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Departments
                .Include(d => d.College)
                .FirstOrDefaultAsync(m => m.DepartmentId == id);
            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        // GET: Departments/Create
        public async Task<IActionResult> Create()
        {
            ViewData["CollegeId"] = new SelectList(await _context.Colleges.Where(c => c.IsActive).ToListAsync(), "CollegeId", "CollegeName");
            return View();
        }

        // POST: Departments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DepartmentId,CollegeId,DepartmentName,IsActive")] Department department)
        {
            // Department names must be unique within their college.
            if (!string.IsNullOrWhiteSpace(department.DepartmentName)
                && await _context.Departments.AnyAsync(d => d.CollegeId == department.CollegeId
                    && d.DepartmentName.Trim() == department.DepartmentName.Trim() && d.DepartmentId != department.DepartmentId))
            {
                ModelState.AddModelError(nameof(Department.DepartmentName), "This college already has a department with this name.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(department);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CollegeId"] = new SelectList(await _context.Colleges.Where(c => c.IsActive).ToListAsync(), "CollegeId", "CollegeName", department.CollegeId);
            return View(department);
        }

        // GET: Departments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Departments.FindAsync(id);
            if (department == null)
            {
                return NotFound();
            }
            ViewData["CollegeId"] = new SelectList(await _context.Colleges.Where(c => c.IsActive).ToListAsync(), "CollegeId", "CollegeName", department.CollegeId);
            return View(department);
        }

        // POST: Departments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DepartmentId,CollegeId,DepartmentName,IsActive")] Department department)
        {
            if (id != department.DepartmentId)
            {
                return NotFound();
            }

            // Department names must be unique within their college.
            if (!string.IsNullOrWhiteSpace(department.DepartmentName)
                && await _context.Departments.AnyAsync(d => d.CollegeId == department.CollegeId
                    && d.DepartmentName.Trim() == department.DepartmentName.Trim() && d.DepartmentId != department.DepartmentId))
            {
                ModelState.AddModelError(nameof(Department.DepartmentName), "This college already has a department with this name.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(department);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DepartmentExists(department.DepartmentId))
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
            ViewData["CollegeId"] = new SelectList(await _context.Colleges.Where(c => c.IsActive).ToListAsync(), "CollegeId", "CollegeName", department.CollegeId);
            return View(department);
        }

        // GET: Departments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Departments
                .Include(d => d.College)
                .FirstOrDefaultAsync(m => m.DepartmentId == id);
            if (department == null)
            {
                return NotFound();
            }

            var inUse = await InUseReasonAsync(department);
            if (inUse != null)
            {
                TempData["ErrorMessage"] = inUse;
                return RedirectToAction(nameof(Index));
            }

            return View(department);
        }

        // POST: Departments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department != null)
            {
                var inUse = await InUseReasonAsync(department);
                if (inUse != null)
                {
                    TempData["ErrorMessage"] = inUse;
                    return RedirectToAction(nameof(Index));
                }

                _context.Departments.Remove(department);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Why this department can't be deleted yet, or null if nothing points
        // to it. The database refuses the delete while any of these exist.
        private async Task<string> InUseReasonAsync(Department department)
        {
            var inUse = Services.InUseMessage.Describe(
                (await _context.DegreePrograms.CountAsync(d => d.DepartmentId == department.DepartmentId), "degree program", "degree programs"),
                (await _context.StudentOrganizations.CountAsync(o => o.DepartmentId == department.DepartmentId), "student organization", "student organizations"),
                (await _context.UserAccessScopes.CountAsync(s => s.DepartmentId == department.DepartmentId), "user access scope", "user access scopes"));
            return inUse == null ? null
                : $"{department.DepartmentName} can't be deleted - it still has {inUse}. Move or remove those first, or mark the department inactive.";
        }

        private bool DepartmentExists(int id)
        {
            return _context.Departments.Any(e => e.DepartmentId == id);
        }
    }
}
