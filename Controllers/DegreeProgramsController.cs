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
using System.IO;
using OfficeOpenXml;

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

        [Authorize(Roles = "Admin")]
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

        [Authorize(Roles = "Admin")]
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

        [Authorize(Roles = "Admin")] // Only Admin can bulk import
        public IActionResult BulkImport()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // Only Admin can bulk import
        public async Task<IActionResult> BulkImport(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Please select a file to upload.");
                return View();
            }

            var errors = new List<string>();
            int successCount = 0;
            int errorCount = 0;

            try
            {
                var fileExtension = Path.GetExtension(file.FileName).ToLower();

                if (fileExtension == ".xlsx" || fileExtension == ".xls")
                {
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                    using (var stream = new MemoryStream())
                    {
                        await file.CopyToAsync(stream);
                        using (var package = new ExcelPackage(stream))
                        {
                            var worksheet = package.Workbook.Worksheets[0];
                            var rowCount = worksheet.Dimension?.Rows ?? 0;

                            if (rowCount < 2)
                            {
                                ModelState.AddModelError("", "The Excel file must contain at least a header row and one data row.");
                                return View();
                            }

                            for (int row = 2; row <= rowCount; row++)
                            {
                                try
                                {
                                    var institution = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                                    var degreeType = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                                    var majorFieldOfStudy = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
                                    var department = worksheet.Cells[row, 4].Value?.ToString()?.Trim();

                                    if (string.IsNullOrWhiteSpace(institution) || string.IsNullOrWhiteSpace(degreeType) ||
                                        string.IsNullOrWhiteSpace(majorFieldOfStudy) || string.IsNullOrWhiteSpace(department))
                                    {
                                        errors.Add($"Row {row} skipped - missing required fields (Institution, Degree Type, Major Field of Study, Department)");
                                        errorCount++;
                                        continue;
                                    }

                                    // Check for duplicates based on all fields combination
                                    if (await _context.DegreePrograms.AnyAsync(dp =>
                                        dp.Institution == institution &&
                                        dp.DegreeType == degreeType &&
                                        dp.MajorFieldOfStudy == majorFieldOfStudy &&
                                        dp.Department == department))
                                    {
                                        errors.Add($"Row {row}: This degree program already exists - {institution}, {degreeType}, {majorFieldOfStudy}, {department}. Entry was not inserted.");
                                        errorCount++;
                                        continue;
                                    }

                                    var degreeProgram = new DegreeProgram
                                    {
                                        Institution = institution,
                                        DegreeType = degreeType,
                                        MajorFieldOfStudy = majorFieldOfStudy,
                                        Department = department
                                    };

                                    _context.DegreePrograms.Add(degreeProgram);
                                    successCount++;
                                }
                                catch (Exception ex)
                                {
                                    errors.Add($"Row {row}: {ex.Message}");
                                    errorCount++;
                                }
                            }
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Invalid file format. Please upload an Excel file (.xlsx or .xls).");
                    return View();
                }

                await _context.SaveChangesAsync();

                // Only redirect to Index if there were successful imports and no errors
                if (successCount > 0 && errorCount == 0)
                {
                    TempData["SuccessMessage"] = $"Import completed: {successCount} records imported successfully.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    // Stay on bulk import page if there were errors or no successful imports
                    TempData["SuccessMessage"] = $"Import completed: {successCount} records imported successfully, {errorCount} errors.";
                    if (errors.Any())
                    {
                        TempData["ErrorMessages"] = string.Join("<br/>", errors);
                    }
                    return View();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error processing file: {ex.Message}");
                return View();
            }
        }
    }
}
