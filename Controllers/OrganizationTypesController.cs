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

        public async Task<IActionResult> Index()
        {
            return View(await _context.OrganizationTypes.ToListAsync());
        }

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

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
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

        [Authorize(Roles = "Admin")]
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

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
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
