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
using System.Text;
using System.Globalization;
using OfficeOpenXml;

namespace Alumni_Management_System.Controllers
{
    [Authorize(Roles = "Admin,Staff")] // Alumni cannot access Alumni Registry
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
        [Authorize(Roles = "Admin")] // Only Admin can create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AlumniRegistries/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // Only Admin can create
        public async Task<IActionResult> Create([Bind("RegistryId,JagId,FirstName,LastName,GraduationYear,DegreeProgram,EmailOnRecord,AccountCreated")] AlumniRegistry alumniRegistry)
        {
            if (ModelState.IsValid)
            {
                _context.Add(alumniRegistry);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Alumni Registry entry created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(alumniRegistry);
        }

        // GET: AlumniRegistries/Edit/5
        [Authorize(Roles = "Admin")] // Only Admin can edit
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
        [Authorize(Roles = "Admin")] // Only Admin can edit
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
                    TempData["SuccessMessage"] = "Alumni Registry entry updated successfully!";
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
        [Authorize(Roles = "Admin")] // Only Admin can delete
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
        [Authorize(Roles = "Admin")] // Only Admin can delete
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var alumniRegistry = await _context.AlumniRegistries.FindAsync(id);
            if (alumniRegistry != null)
            {
                _context.AlumniRegistries.Remove(alumniRegistry);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Alumni Registry entry deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool AlumniRegistryExists(int id)
        {
            return _context.AlumniRegistries.Any(e => e.RegistryId == id);
        }

        // GET: AlumniRegistries/BulkImport
        [Authorize(Roles = "Admin")] // Only Admin can bulk import
        public IActionResult BulkImport()
        {
            return View();
        }

        // POST: AlumniRegistries/BulkImport
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

            var importResults = new List<string>();
            var errors = new List<string>();
            int successCount = 0;
            int errorCount = 0;

            try
            {
                var fileExtension = Path.GetExtension(file.FileName).ToLower();

                if (fileExtension == ".csv")
                {
                    using (var reader = new StreamReader(file.OpenReadStream()))
                    {
                        // Skip header
                        var header = await reader.ReadLineAsync();

                        while (!reader.EndOfStream)
                        {
                            var line = await reader.ReadLineAsync();
                            if (string.IsNullOrWhiteSpace(line)) continue;

                            var values = ParseCsvLine(line);

                            if (values.Length < 3)
                            {
                                errors.Add($"Row skipped - insufficient columns: {line}");
                                errorCount++;
                                continue;
                            }

                            try
                            {
                                var registry = new AlumniRegistry
                                {
                                    JagId = values[0].Trim(),
                                    FirstName = values[1].Trim(),
                                    LastName = values[2].Trim(),
                                    GraduationYear = values.Length > 3 && int.TryParse(values[3], out int year) ? year : (int?)null,
                                    DegreeProgram = values.Length > 4 ? values[4].Trim() : null,
                                    EmailOnRecord = values.Length > 5 ? values[5].Trim() : null,
                                    AccountCreated = false
                                };

                                // Validate JAG ID format
                                if (!System.Text.RegularExpressions.Regex.IsMatch(registry.JagId, @"^J00\d+$"))
                                {
                                    errors.Add($"Invalid JAG ID format '{registry.JagId}' - must start with J00");
                                    errorCount++;
                                    continue;
                                }

                                // Check for duplicate JAG ID
                                if (await _context.AlumniRegistries.AnyAsync(a => a.JagId == registry.JagId))
                                {
                                    errors.Add($"Duplicate JAG ID: {registry.JagId}");
                                    errorCount++;
                                    continue;
                                }

                                _context.AlumniRegistries.Add(registry);
                                successCount++;
                            }
                            catch (Exception ex)
                            {
                                errors.Add($"Error processing row: {line} - {ex.Message}");
                                errorCount++;
                            }
                        }
                    }
                }
                else if (fileExtension == ".xlsx" || fileExtension == ".xls")
                {
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                    using (var stream = new MemoryStream())
                    {
                        await file.CopyToAsync(stream);
                        using (var package = new ExcelPackage(stream))
                        {
                            var worksheet = package.Workbook.Worksheets[0];
                            var rowCount = worksheet.Dimension?.Rows ?? 0;

                            // Start from row 2 (skip header)
                            for (int row = 2; row <= rowCount; row++)
                            {
                                try
                                {
                                    var jagId = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                                    var firstName = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                                    var lastName = worksheet.Cells[row, 3].Value?.ToString()?.Trim();

                                    if (string.IsNullOrWhiteSpace(jagId) || string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
                                    {
                                        errors.Add($"Row {row} skipped - missing required fields");
                                        errorCount++;
                                        continue;
                                    }

                                    var registry = new AlumniRegistry
                                    {
                                        JagId = jagId,
                                        FirstName = firstName,
                                        LastName = lastName,
                                        GraduationYear = int.TryParse(worksheet.Cells[row, 4].Value?.ToString(), out int year) ? year : (int?)null,
                                        DegreeProgram = worksheet.Cells[row, 5].Value?.ToString()?.Trim(),
                                        EmailOnRecord = worksheet.Cells[row, 6].Value?.ToString()?.Trim(),
                                        AccountCreated = false
                                    };

                                    // Validate JAG ID format
                                    if (!System.Text.RegularExpressions.Regex.IsMatch(registry.JagId, @"^J00\d+$"))
                                    {
                                        errors.Add($"Row {row}: Invalid JAG ID format '{registry.JagId}' - must start with J00");
                                        errorCount++;
                                        continue;
                                    }

                                    // Check for duplicate JAG ID
                                    if (await _context.AlumniRegistries.AnyAsync(a => a.JagId == registry.JagId))
                                    {
                                        errors.Add($"Row {row}: Duplicate JAG ID: {registry.JagId}");
                                        errorCount++;
                                        continue;
                                    }

                                    _context.AlumniRegistries.Add(registry);
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
                    ModelState.AddModelError("", "Invalid file format. Please upload a CSV or Excel file.");
                    return View();
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Import completed: {successCount} records imported successfully, {errorCount} errors.";
                if (errors.Any())
                {
                    TempData["ErrorMessages"] = string.Join("<br/>", errors);
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error processing file: {ex.Message}");
                return View();
            }
        }

        private string[] ParseCsvLine(string line)
        {
            var result = new List<string>();
            var currentField = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(currentField.ToString());
                    currentField.Clear();
                }
                else
                {
                    currentField.Append(c);
                }
            }

            result.Add(currentField.ToString());
            return result.ToArray();
        }
    }
}
