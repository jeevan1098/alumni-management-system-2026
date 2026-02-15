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
    [Authorize]
    public class AlumniDegreesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AlumniDegreesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: AlumniDegrees
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.AlumniDegrees.Include(a => a.Alumni).Include(a => a.Degree);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: AlumniDegrees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alumniDegree = await _context.AlumniDegrees
                .Include(a => a.Alumni)
                .Include(a => a.Degree)
                .FirstOrDefaultAsync(m => m.AlumniDegreeId == id);
            if (alumniDegree == null)
            {
                return NotFound();
            }

            return View(alumniDegree);
        }

        // GET: AlumniDegrees/Create
        public IActionResult Create()
        {
            ViewData["AlumniId"] = new SelectList(_context.Alumni, "AlumniId", "FirstName");
            ViewData["DegreeId"] = new SelectList(_context.DegreePrograms, "DegreeId", "DegreeType");
            return View();
        }

        // POST: AlumniDegrees/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AlumniDegreeId,AlumniId,DegreeId,DateConferred,YearsToCompleteDegree,Gpa,EmploymentWhileStudying,DegreeSpecificJob,ParticipatedInResearch,JobSecuredUponGraduation,AttendedOrPlansGradSchool")] AlumniDegree alumniDegree)
        {
            if (ModelState.IsValid)
            {
                _context.Add(alumniDegree);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AlumniId"] = new SelectList(_context.Alumni, "AlumniId", "FirstName", alumniDegree.AlumniId);
            ViewData["DegreeId"] = new SelectList(_context.DegreePrograms, "DegreeId", "DegreeType", alumniDegree.DegreeId);
            return View(alumniDegree);
        }

        // GET: AlumniDegrees/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alumniDegree = await _context.AlumniDegrees.FindAsync(id);
            if (alumniDegree == null)
            {
                return NotFound();
            }
            ViewData["AlumniId"] = new SelectList(_context.Alumni, "AlumniId", "FirstName", alumniDegree.AlumniId);
            ViewData["DegreeId"] = new SelectList(_context.DegreePrograms, "DegreeId", "DegreeType", alumniDegree.DegreeId);
            return View(alumniDegree);
        }

        // POST: AlumniDegrees/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AlumniDegreeId,AlumniId,DegreeId,DateConferred,YearsToCompleteDegree,Gpa,EmploymentWhileStudying,DegreeSpecificJob,ParticipatedInResearch,JobSecuredUponGraduation,AttendedOrPlansGradSchool")] AlumniDegree alumniDegree)
        {
            if (id != alumniDegree.AlumniDegreeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(alumniDegree);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AlumniDegreeExists(alumniDegree.AlumniDegreeId))
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
            ViewData["AlumniId"] = new SelectList(_context.Alumni, "AlumniId", "FirstName", alumniDegree.AlumniId);
            ViewData["DegreeId"] = new SelectList(_context.DegreePrograms, "DegreeId", "DegreeType", alumniDegree.DegreeId);
            return View(alumniDegree);
        }

        // GET: AlumniDegrees/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alumniDegree = await _context.AlumniDegrees
                .Include(a => a.Alumni)
                .Include(a => a.Degree)
                .FirstOrDefaultAsync(m => m.AlumniDegreeId == id);
            if (alumniDegree == null)
            {
                return NotFound();
            }

            return View(alumniDegree);
        }

        // POST: AlumniDegrees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var alumniDegree = await _context.AlumniDegrees.FindAsync(id);
            if (alumniDegree != null)
            {
                _context.AlumniDegrees.Remove(alumniDegree);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AlumniDegreeExists(int id)
        {
            return _context.AlumniDegrees.Any(e => e.AlumniDegreeId == id);
        }
    }
}
