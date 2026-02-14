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
    public class AlumniRegistriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AlumniRegistriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: AlumniRegistries
        public async Task<IActionResult> Index()
        {
            return View(await _context.AlumniRegistries.ToListAsync());
        }

        // GET: AlumniRegistries/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alumniRegistry = await _context.AlumniRegistries
                .FirstOrDefaultAsync(m => m.RegistryId == id);
            if (alumniRegistry == null)
            {
                return NotFound();
            }

            return View(alumniRegistry);
        }

        // GET: AlumniRegistries/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AlumniRegistries/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RegistryId,JagId,FirstName,LastName,GraduationYear,DegreeProgram,EmailOnRecord,AccountCreated")] AlumniRegistry alumniRegistry)
        {
            if (ModelState.IsValid)
            {
                _context.Add(alumniRegistry);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(alumniRegistry);
        }

        // GET: AlumniRegistries/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alumniRegistry = await _context.AlumniRegistries.FindAsync(id);
            if (alumniRegistry == null)
            {
                return NotFound();
            }
            return View(alumniRegistry);
        }

        // POST: AlumniRegistries/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RegistryId,JagId,FirstName,LastName,GraduationYear,DegreeProgram,EmailOnRecord,AccountCreated")] AlumniRegistry alumniRegistry)
        {
            if (id != alumniRegistry.RegistryId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(alumniRegistry);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AlumniRegistryExists(alumniRegistry.RegistryId))
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
            return View(alumniRegistry);
        }

        // GET: AlumniRegistries/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alumniRegistry = await _context.AlumniRegistries
                .FirstOrDefaultAsync(m => m.RegistryId == id);
            if (alumniRegistry == null)
            {
                return NotFound();
            }

            return View(alumniRegistry);
        }

        // POST: AlumniRegistries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var alumniRegistry = await _context.AlumniRegistries.FindAsync(id);
            if (alumniRegistry != null)
            {
                _context.AlumniRegistries.Remove(alumniRegistry);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AlumniRegistryExists(int id)
        {
            return _context.AlumniRegistries.Any(e => e.RegistryId == id);
        }
    }
}
