using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Alumni_Management_System.Data;
using Alumni_Management_System.Models;

namespace Alumni_Management_System.Controllers
{
    public class DegreeProgramsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DegreeProgramsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: DegreePrograms
        public async Task<IActionResult> Index()
        {
            return View(await _context.DegreePrograms.ToListAsync());
        }

        // GET: DegreePrograms/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var degreeProgram = await _context.DegreePrograms
                .FirstOrDefaultAsync(m => m.DegreeId == id);
            if (degreeProgram == null)
            {
                return NotFound();
            }

            return View(degreeProgram);
        }

        // GET: DegreePrograms/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: DegreePrograms/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DegreeId,Institution,DegreeType,MajorFieldOfStudy,Department")] DegreeProgram degreeProgram)
        {
            if (ModelState.IsValid)
            {
                _context.Add(degreeProgram);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(degreeProgram);
        }

        // GET: DegreePrograms/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var degreeProgram = await _context.DegreePrograms.FindAsync(id);
            if (degreeProgram == null)
            {
                return NotFound();
            }
            return View(degreeProgram);
        }

        // POST: DegreePrograms/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DegreeId,Institution,DegreeType,MajorFieldOfStudy,Department")] DegreeProgram degreeProgram)
        {
            if (id != degreeProgram.DegreeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(degreeProgram);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DegreeProgramExists(degreeProgram.DegreeId))
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
            return View(degreeProgram);
        }

        // GET: DegreePrograms/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var degreeProgram = await _context.DegreePrograms
                .FirstOrDefaultAsync(m => m.DegreeId == id);
            if (degreeProgram == null)
            {
                return NotFound();
            }

            return View(degreeProgram);
        }

        // POST: DegreePrograms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var degreeProgram = await _context.DegreePrograms.FindAsync(id);
            if (degreeProgram != null)
            {
                _context.DegreePrograms.Remove(degreeProgram);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DegreeProgramExists(int id)
        {
            return _context.DegreePrograms.Any(e => e.DegreeId == id);
        }
    }
}
