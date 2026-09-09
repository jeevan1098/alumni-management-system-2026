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

namespace Alumni_Management_System.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class StudentOrganizationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentOrganizationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: StudentOrganizations
        public async Task<IActionResult> Index()
        {
            return View(await _context.StudentOrganizations
                .Include(o => o.College)
                .Include(o => o.Department)
                .ToListAsync());
        }

        // GET: StudentOrganizations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var studentOrganization = await _context.StudentOrganizations
                .Include(o => o.College)
                .Include(o => o.Department)
                .FirstOrDefaultAsync(m => m.OrganizationId == id);
            if (studentOrganization == null)
            {
                return NotFound();
            }

            return View(studentOrganization);
        }

        // GET: StudentOrganizations/Create
        public async Task<IActionResult> Create()
        {
            ViewData["CollegeId"] = new SelectList(await _context.Colleges.Where(c => c.IsActive).ToListAsync(), "CollegeId", "CollegeName");
            ViewData["DepartmentId"] = new SelectList(await _context.Departments.Where(d => d.IsActive).ToListAsync(), "DepartmentId", "DepartmentName");
            return View();
        }

        // POST: StudentOrganizations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrganizationId,OrganizationName,CollegeId,DepartmentId,IsActive")] StudentOrganization studentOrganization)
        {
            if (ModelState.IsValid)
            {
                _context.Add(studentOrganization);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CollegeId"] = new SelectList(await _context.Colleges.Where(c => c.IsActive).ToListAsync(), "CollegeId", "CollegeName", studentOrganization.CollegeId);
            ViewData["DepartmentId"] = new SelectList(await _context.Departments.Where(d => d.IsActive).ToListAsync(), "DepartmentId", "DepartmentName", studentOrganization.DepartmentId);
            return View(studentOrganization);
        }

        // GET: StudentOrganizations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var studentOrganization = await _context.StudentOrganizations.FindAsync(id);
            if (studentOrganization == null)
            {
                return NotFound();
            }
            ViewData["CollegeId"] = new SelectList(await _context.Colleges.Where(c => c.IsActive).ToListAsync(), "CollegeId", "CollegeName", studentOrganization.CollegeId);
            ViewData["DepartmentId"] = new SelectList(await _context.Departments.Where(d => d.IsActive).ToListAsync(), "DepartmentId", "DepartmentName", studentOrganization.DepartmentId);
            return View(studentOrganization);
        }

        // POST: StudentOrganizations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrganizationId,OrganizationName,CollegeId,DepartmentId,IsActive")] StudentOrganization studentOrganization)
        {
            if (id != studentOrganization.OrganizationId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(studentOrganization);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentOrganizationExists(studentOrganization.OrganizationId))
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
            ViewData["CollegeId"] = new SelectList(await _context.Colleges.Where(c => c.IsActive).ToListAsync(), "CollegeId", "CollegeName", studentOrganization.CollegeId);
            ViewData["DepartmentId"] = new SelectList(await _context.Departments.Where(d => d.IsActive).ToListAsync(), "DepartmentId", "DepartmentName", studentOrganization.DepartmentId);
            return View(studentOrganization);
        }

        // GET: StudentOrganizations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var studentOrganization = await _context.StudentOrganizations
                .Include(o => o.College)
                .Include(o => o.Department)
                .FirstOrDefaultAsync(m => m.OrganizationId == id);
            if (studentOrganization == null)
            {
                return NotFound();
            }

            return View(studentOrganization);
        }

        // POST: StudentOrganizations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var studentOrganization = await _context.StudentOrganizations.FindAsync(id);
            if (studentOrganization != null)
            {
                _context.StudentOrganizations.Remove(studentOrganization);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StudentOrganizationExists(int id)
        {
            return _context.StudentOrganizations.Any(e => e.OrganizationId == id);
        }
    }
}
