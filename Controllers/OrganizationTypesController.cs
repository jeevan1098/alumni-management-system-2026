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
    public class OrganizationTypesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrganizationTypesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: OrganizationTypes
        public async Task<IActionResult> Index()
        {
            return View(await _context.OrganizationTypes.ToListAsync());
        }

        // GET: OrganizationTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var organizationType = await _context.OrganizationTypes
                .FirstOrDefaultAsync(m => m.OrganizationTypeId == id);
            if (organizationType == null)
            {
                return NotFound();
            }

            return View(organizationType);
        }

        // GET: OrganizationTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: OrganizationTypes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrganizationTypeId,OrganizationName")] OrganizationType organizationType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(organizationType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(organizationType);
        }

        // GET: OrganizationTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var organizationType = await _context.OrganizationTypes.FindAsync(id);
            if (organizationType == null)
            {
                return NotFound();
            }
            return View(organizationType);
        }

        // POST: OrganizationTypes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrganizationTypeId,OrganizationName")] OrganizationType organizationType)
        {
            if (id != organizationType.OrganizationTypeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(organizationType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrganizationTypeExists(organizationType.OrganizationTypeId))
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
            return View(organizationType);
        }

        // GET: OrganizationTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var organizationType = await _context.OrganizationTypes
                .FirstOrDefaultAsync(m => m.OrganizationTypeId == id);
            if (organizationType == null)
            {
                return NotFound();
            }

            return View(organizationType);
        }

        // POST: OrganizationTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var organizationType = await _context.OrganizationTypes.FindAsync(id);
            if (organizationType != null)
            {
                _context.OrganizationTypes.Remove(organizationType);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrganizationTypeExists(int id)
        {
            return _context.OrganizationTypes.Any(e => e.OrganizationTypeId == id);
        }
    }
}
