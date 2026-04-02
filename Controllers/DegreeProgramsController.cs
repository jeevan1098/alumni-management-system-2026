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
    public class DegreeProgramsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DegreeProgramsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.DegreePrograms.ToListAsync());
        }

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

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
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

        [Authorize(Roles = "Admin")]
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

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
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
