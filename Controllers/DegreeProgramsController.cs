using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            => View(await _context.DegreePrograms.ToListAsync());

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var degreeProgram = await _context.DegreePrograms.FirstOrDefaultAsync(m => m.DegreeId == id);
            if (degreeProgram == null) return NotFound();
            return View(degreeProgram);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("DegreeId,Institution,DegreeType,MajorFieldOfStudy,Department")] DegreeProgram degreeProgram)
        {
            // Normalize casing before anything else
            NormalizeProgram(degreeProgram);

            // Duplicate check (case-insensitive because values are now normalized)
            bool isDuplicate = await _context.DegreePrograms.AnyAsync(dp =>
                dp.Institution == degreeProgram.Institution &&
                dp.DegreeType == degreeProgram.DegreeType &&
                dp.MajorFieldOfStudy == degreeProgram.MajorFieldOfStudy &&
                dp.Department == degreeProgram.Department);

            if (isDuplicate)
                ModelState.AddModelError(string.Empty,
                    "A degree program with the same Institution, Type, Major, and Department already exists.");

            if (ModelState.IsValid)
            {
                _context.Add(degreeProgram);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Degree program created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(degreeProgram);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var degreeProgram = await _context.DegreePrograms.FindAsync(id);
            if (degreeProgram == null) return NotFound();
            return View(degreeProgram);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DegreeId,Institution,DegreeType,MajorFieldOfStudy,Department")] DegreeProgram degreeProgram)
        {
            if (id != degreeProgram.DegreeId) return NotFound();

            // Normalize casing before anything else
            NormalizeProgram(degreeProgram);

            // Duplicate check excluding self
            bool isDuplicate = await _context.DegreePrograms.AnyAsync(dp =>
                dp.DegreeId != degreeProgram.DegreeId &&
                dp.Institution == degreeProgram.Institution &&
                dp.DegreeType == degreeProgram.DegreeType &&
                dp.MajorFieldOfStudy == degreeProgram.MajorFieldOfStudy &&
                dp.Department == degreeProgram.Department);

            if (isDuplicate)
                ModelState.AddModelError(string.Empty,
                    "A degree program with the same Institution, Type, Major, and Department already exists.");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(degreeProgram);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Degree program updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DegreeProgramExists(degreeProgram.DegreeId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(degreeProgram);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var degreeProgram = await _context.DegreePrograms.FirstOrDefaultAsync(m => m.DegreeId == id);
            if (degreeProgram == null) return NotFound();

            // Check if any alumni are assigned to this degree program
            var alumniCount = await _context.AlumniDegrees.CountAsync(ad => ad.DegreeId == id);
            if (alumniCount > 0)
            {
                TempData["ErrorMessage"] = $"Cannot delete this degree program — {alumniCount} alumni(s) are assigned to it. Remove their degree records first.";
                return RedirectToAction(nameof(Index));
            }

            return View(degreeProgram);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Double-check before deleting
            var alumniCount = await _context.AlumniDegrees.CountAsync(ad => ad.DegreeId == id);
            if (alumniCount > 0)
            {
                TempData["ErrorMessage"] = $"Cannot delete this degree program — {alumniCount} alumni are assigned to it.";
                return RedirectToAction(nameof(Index));
            }

            var degreeProgram = await _context.DegreePrograms.FindAsync(id);
            if (degreeProgram != null)
            {
                _context.DegreePrograms.Remove(degreeProgram);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Degree program deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool DegreeProgramExists(int id)
            => _context.DegreePrograms.Any(e => e.DegreeId == id);

        /// <summary>
        /// Normalizes all fields on a DegreeProgram to prevent case-variant duplicates.
        /// Institution is matched to known values; unknown institutions use Title Case.
        /// DegreeType preserves known codes (BSCSC, MSCYS etc) and normalises common words.
        /// MajorFieldOfStudy and Department are always stored in Title Case.
        /// </summary>
        private static void NormalizeProgram(DegreeProgram dp)
        {
            dp.Institution = NormalizeInstitution(dp.Institution);
            dp.DegreeType = NormalizeDegreeType(dp.DegreeType);
            dp.MajorFieldOfStudy = ToTitleCase(dp.MajorFieldOfStudy);
            dp.Department = ToTitleCase(dp.Department);
        }

        private static string NormalizeInstitution(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return value;
            var trimmed = value.Trim();

            // Map known institutions to their canonical form
            var known = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "University of South Alabama", "University of South Alabama" },
                { "Other University",            "Other University" },
            };

            return known.TryGetValue(trimmed, out var canonical)
                ? canonical
                : ToTitleCase(trimmed); // unknown institutions → Title Case
        }

        private static string NormalizeDegreeType(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return value;
            var trimmed = value.Trim();

            // Map common words; preserve degree codes (BSCSC, MSCYS, etc.) as-is
            var known = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Bachelors",  "Bachelors" },
                { "Bachelor",   "Bachelors" },
                { "Masters",    "Masters"   },
                { "Master",     "Masters"   },
                { "PhD",        "PhD"       },
                { "Phd",        "PhD"       },
                { "PHD",        "PhD"       },
                { "Doctorate",  "PhD"       },
            };

            return known.TryGetValue(trimmed, out var canonical)
                ? canonical
                : trimmed.ToUpper(); // degree codes like BSCSC stored as uppercase
        }

        /// <summary>
        /// Converts a string to Title Case (e.g. "computer science" → "Computer Science").
        /// Returns null if the input is null or whitespace.
        /// </summary>
        private static string ToTitleCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return value;
            return System.Globalization.CultureInfo.CurrentCulture
                         .TextInfo.ToTitleCase(value.Trim().ToLower());
        }

        [Authorize(Roles = "Admin")]
        public IActionResult BulkImport() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
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
                if (fileExtension != ".xlsx" && fileExtension != ".xls")
                {
                    ModelState.AddModelError("", "Invalid file format. Please upload an Excel file (.xlsx or .xls).");
                    return View();
                }

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                using var package = new ExcelPackage(stream);
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
                            errors.Add($"Row {row} skipped - missing required fields.");
                            errorCount++;
                            continue;
                        }

                        // Institution and DegreeType: trim only (preserve intended casing)
                        // MajorFieldOfStudy and Department: convert to Title Case
                        institution = NormalizeInstitution(institution);
                        degreeType = NormalizeDegreeType(degreeType);
                        majorFieldOfStudy = ToTitleCase(majorFieldOfStudy);
                        department = ToTitleCase(department);

                        if (await _context.DegreePrograms.AnyAsync(dp =>
                            dp.Institution.ToLower() == institution.ToLower() &&
                            dp.DegreeType.ToLower() == degreeType.ToLower() &&
                            dp.MajorFieldOfStudy.ToLower() == majorFieldOfStudy.ToLower() &&
                            dp.Department.ToLower() == department.ToLower()))
                        {
                            errors.Add($"Row {row}: Duplicate — already exists.");
                            errorCount++;
                            continue;
                        }

                        _context.DegreePrograms.Add(new DegreeProgram
                        {
                            Institution = institution,
                            DegreeType = degreeType,
                            MajorFieldOfStudy = majorFieldOfStudy,
                            Department = department
                        });
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Row {row}: {ex.Message}");
                        errorCount++;
                    }
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Import completed: {successCount} records imported, {errorCount} errors.";
                if (errors.Any())
                    TempData["ErrorMessages"] = string.Join("<br/>", errors);

                return successCount > 0 && errorCount == 0
                    ? RedirectToAction(nameof(Index))
                    : View();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error processing file: {ex.Message}");
                return View();
            }
        }
    }
}
