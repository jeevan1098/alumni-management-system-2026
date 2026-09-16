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

        // Registrar exports identify colleges by a short internal code rather
        // than the real college name - add to this as new codes show up.
        private static readonly Dictionary<string, string> CollegeCodeMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["CS"] = "School of Computing"
        };

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

            // Two different JAG IDs sharing the same email is still a
            // duplicate person, and two rows in the same file can duplicate
            // each other before anything is saved (so a DB-only check
            // wouldn't catch them) - track both within this import too.
            var seenJagIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var seenEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Alumni whose College field was touched by a degree row during
            // this import - re-synced to their most-recently-conferred
            // degree's college once everything is saved (see below), instead
            // of just keeping whichever row happened to be processed last.
            var alumniWithDegreeRows = new List<Alumni>();

            async Task<string> CheckDuplicateAsync(string jagId, string email)
            {
                if (seenJagIds.Contains(jagId))
                {
                    return $"Duplicate JAG ID within this file: {jagId}";
                }

                if (await _context.AlumniRegistries.AnyAsync(a => a.JagId == jagId))
                {
                    return $"Duplicate JAG ID: {jagId}";
                }

                // A JAG ID already tied to an existing account (e.g. an
                // Admin/Staff account) can't be reused for a bulk-imported
                // Alumni - it's a one-JAG-ID-per-account system.
                if (await _context.Users.AnyAsync(u => u.JagId == jagId))
                {
                    return $"JAG ID {jagId} is already in use by an existing account - skipped";
                }

                if (!string.IsNullOrWhiteSpace(email))
                {
                    if (seenEmails.Contains(email))
                    {
                        return $"Duplicate email within this file: {email}";
                    }

                    if (await _context.Alumni.AnyAsync(a => a.PermanentEmail == email))
                    {
                        return $"Email already in use by another Alumni record: {email}";
                    }
                }

                return null;
            }

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
                                    AccountCreated = false
                                };

                                // Validate JAG ID format
                                if (!System.Text.RegularExpressions.Regex.IsMatch(registry.JagId, @"^J\d+$"))
                                {
                                    errors.Add($"Invalid JAG ID format '{registry.JagId}' - must start with J followed by numbers");
                                    errorCount++;
                                    continue;
                                }

                                var gradYear = values.Length > 3 ? values[3].Trim() : null;
                                var emailOnRecord = values.Length > 5 ? values[5].Trim() : null;

                                var duplicateReason = await CheckDuplicateAsync(registry.JagId, emailOnRecord);
                                if (duplicateReason != null)
                                {
                                    errors.Add(duplicateReason);
                                    errorCount++;
                                    continue;
                                }

                                seenJagIds.Add(registry.JagId);
                                if (!string.IsNullOrWhiteSpace(emailOnRecord)) seenEmails.Add(emailOnRecord);

                                _context.AlumniRegistries.Add(registry);

                                await AddAlumniRecordIfMissingAsync(registry, gradYear, emailOnRecord);

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
                            var colCount = worksheet.Dimension?.Columns ?? 0;

                            // Map header names (whatever template is used - the
                            // simple 6-column sample, or a richer registrar
                            // export) to column indexes, instead of assuming a
                            // fixed position, so both layouts work.
                            var headers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                            for (int c = 1; c <= colCount; c++)
                            {
                                var h = worksheet.Cells[1, c].Value?.ToString()?.Trim();
                                if (!string.IsNullOrEmpty(h) && !headers.ContainsKey(h))
                                {
                                    headers[h] = c;
                                }
                            }

                            string GetCell(int row, params string[] headerNames)
                            {
                                foreach (var name in headerNames)
                                {
                                    if (headers.TryGetValue(name, out var col))
                                    {
                                        var v = worksheet.Cells[row, col].Value?.ToString()?.Trim();
                                        if (!string.IsNullOrWhiteSpace(v)) return v;
                                    }
                                }
                                return null;
                            }

                            // Caches so two rows with the same College/Department/
                            // Degree Program only create it once per import, and
                            // existing ones (seeded or from earlier imports) are reused.
                            var collegeCache = new Dictionary<string, College>(StringComparer.OrdinalIgnoreCase);
                            var departmentCache = new Dictionary<(int, string), Department>();
                            var degreeProgramCache = new Dictionary<(int, string, string), DegreeProgram>();
                            const string Institution = "University of South Alabama";

                            async Task<College> GetOrCreateCollegeAsync(string rawCode)
                            {
                                var name = CollegeCodeMap.TryGetValue(rawCode, out var mapped) ? mapped : rawCode;
                                if (collegeCache.TryGetValue(name, out var cached)) return cached;

                                var existing = await _context.Colleges.FirstOrDefaultAsync(c => c.CollegeName == name);
                                if (existing != null)
                                {
                                    collegeCache[name] = existing;
                                    return existing;
                                }

                                var created = new College { CollegeName = name, IsInternal = true, IsActive = true };
                                _context.Colleges.Add(created);
                                collegeCache[name] = created;
                                return created;
                            }

                            async Task<Department> GetOrCreateDepartmentAsync(College college, string deptName)
                            {
                                var key = (college.CollegeId, deptName);
                                if (departmentCache.TryGetValue(key, out var cached)) return cached;

                                var existing = college.CollegeId != 0
                                    ? await _context.Departments.FirstOrDefaultAsync(d => d.CollegeId == college.CollegeId && d.DepartmentName == deptName)
                                    : null;
                                if (existing != null)
                                {
                                    departmentCache[key] = existing;
                                    return existing;
                                }

                                var created = new Department { College = college, DepartmentName = deptName, IsActive = true };
                                _context.Departments.Add(created);
                                departmentCache[key] = created;
                                return created;
                            }

                            async Task<DegreeProgram> GetOrCreateDegreeProgramAsync(Department department, string degreeType, string major)
                            {
                                var key = (department.DepartmentId, degreeType, major);
                                if (degreeProgramCache.TryGetValue(key, out var cached)) return cached;

                                var existing = department.DepartmentId != 0
                                    ? await _context.DegreePrograms.FirstOrDefaultAsync(dp =>
                                        dp.DepartmentId == department.DepartmentId && dp.DegreeType == degreeType && dp.MajorFieldOfStudy == major)
                                    : null;
                                if (existing != null)
                                {
                                    degreeProgramCache[key] = existing;
                                    return existing;
                                }

                                var created = new DegreeProgram
                                {
                                    Department = department,
                                    Institution = Institution,
                                    DegreeType = degreeType,
                                    MajorFieldOfStudy = major,
                                    IsActive = true
                                };
                                _context.DegreePrograms.Add(created);
                                degreeProgramCache[key] = created;
                                return created;
                            }

                            // Start from row 2 (skip header)
                            for (int row = 2; row <= rowCount; row++)
                            {
                                try
                                {
                                    var jagId = GetCell(row, "JAG ID", "ID");
                                    var firstName = GetCell(row, "First Name");
                                    var lastName = GetCell(row, "Last Name");

                                    if (string.IsNullOrWhiteSpace(jagId) && string.IsNullOrWhiteSpace(firstName) && string.IsNullOrWhiteSpace(lastName))
                                    {
                                        // Fully blank row - not worth reporting as an error.
                                        continue;
                                    }

                                    if (string.IsNullOrWhiteSpace(jagId) || string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
                                    {
                                        errors.Add($"Row {row} skipped - missing required field(s) (JAG ID/First Name/Last Name)");
                                        errorCount++;
                                        continue;
                                    }

                                    var registry = new AlumniRegistry
                                    {
                                        JagId = jagId,
                                        FirstName = firstName,
                                        LastName = lastName,
                                        AccountCreated = false
                                    };

                                    // Validate JAG ID format
                                    if (!System.Text.RegularExpressions.Regex.IsMatch(registry.JagId, @"^J\d+$"))
                                    {
                                        errors.Add($"Row {row}: Invalid JAG ID format '{registry.JagId}' - must start with J followed by numbers");
                                        errorCount++;
                                        continue;
                                    }

                                    var gradRaw = GetCell(row, "Grad", "Graduation Year");
                                    var email = GetCell(row, "UNIV Email", "Email On Record", "OTH Email");

                                    var duplicateReason = await CheckDuplicateAsync(registry.JagId, email);
                                    if (duplicateReason != null)
                                    {
                                        errors.Add($"Row {row}: {duplicateReason}");
                                        errorCount++;
                                        continue;
                                    }

                                    seenJagIds.Add(registry.JagId);
                                    if (!string.IsNullOrWhiteSpace(email)) seenEmails.Add(email);

                                    _context.AlumniRegistries.Add(registry);

                                    var alumni = await AddAlumniRecordIfMissingAsync(registry, gradRaw, email);

                                    // Only the richer registrar export carries College (CL) +
                                    // Major - the simple 6-column sample doesn't, so this whole
                                    // College/Department/Degree Program linkage is skipped for it.
                                    var collegeCode = GetCell(row, "CL");
                                    var major = GetCell(row, "Major");
                                    if (!string.IsNullOrWhiteSpace(collegeCode) && !string.IsNullOrWhiteSpace(major))
                                    {
                                        var college = await GetOrCreateCollegeAsync(collegeCode);
                                        var department = await GetOrCreateDepartmentAsync(college, major);
                                        var degreeCode = GetCell(row, "Degree") ?? major;
                                        var degreeProgram = await GetOrCreateDegreeProgramAsync(department, degreeCode, major);

                                        alumniWithDegreeRows.Add(alumni);

                                        if (gradRaw != null && gradRaw.Length >= 4 && int.TryParse(gradRaw.Substring(0, 4), out var gradYearForDegree))
                                        {
                                            decimal? gpa = null;
                                            var gpaRaw = GetCell(row, "Inst GPA");
                                            if (gpaRaw != null && decimal.TryParse(gpaRaw, out var parsedGpa))
                                            {
                                                gpa = parsedGpa;
                                            }

                                            var alreadyLinked = alumni.AlumniDegrees.Any(ad => ad.Degree == degreeProgram)
                                                || (alumni.AlumniId != 0 && await _context.AlumniDegrees.AnyAsync(ad => ad.AlumniId == alumni.AlumniId && ad.Degree == degreeProgram));

                                            if (!alreadyLinked)
                                            {
                                                _context.AlumniDegrees.Add(new AlumniDegree
                                                {
                                                    Alumni = alumni,
                                                    Degree = degreeProgram,
                                                    DateConferred = new DateOnly(gradYearForDegree, 5, 1),
                                                    Gpa = gpa
                                                });
                                            }
                                        }
                                    }

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

                foreach (var alumniId in alumniWithDegreeRows.Select(a => a.AlumniId).Distinct())
                {
                    await Services.AlumniCollegeSyncService.SyncToMostRecentDegreeAsync(_context, alumniId);
                }

                TempData["SuccessMessage"] = $"Import completed: {successCount} records imported successfully, {errorCount} errors.";
                if (errors.Any())
                {
                    TempData["ErrorMessages"] = string.Join("<br/>", errors);
                }

                return RedirectToAction("Index", "Alumni");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error processing file: {ex.Message}");
                return View();
            }
        }

        // Bulk import used to populate the actual Alumni table too (not just
        // the Registry stub) - restoring that: each imported person gets a
        // real Alumni record so the JagId -> RegisterAlumni self-service
        // flow (which requires an existing Alumni row) actually works, and
        // so Admin/Staff can see them under Alumni immediately.
        private async Task<Alumni> AddAlumniRecordIfMissingAsync(AlumniRegistry registry, string gradYearRaw, string emailOnRecord)
        {
            var existing = await _context.Alumni.Include(a => a.AlumniDegrees).FirstOrDefaultAsync(a => a.JagId == registry.JagId);
            if (existing != null)
            {
                return existing;
            }

            // "Graduation Year" can arrive as a plain year ("2023") or, from the
            // richer registrar export, a 6-digit term code ("202610") - either
            // way the year is the first 4 digits.
            var gradYear = 0;
            if (!string.IsNullOrWhiteSpace(gradYearRaw))
            {
                var yearPart = gradYearRaw.Length >= 4 ? gradYearRaw.Substring(0, 4) : gradYearRaw;
                int.TryParse(yearPart, out gradYear);
            }

            var alumni = new Alumni
            {
                JagId = registry.JagId,
                FirstName = registry.FirstName,
                LastName = registry.LastName,
                PermanentEmail = string.IsNullOrWhiteSpace(emailOnRecord) ? $"{registry.JagId.ToLower()}@pending.import" : emailOnRecord,
                GraduationYear = gradYear,
                IsActive = false,
                Privacy = true,
                SolicitationCode = false,
                LastUpdated = DateTime.Now
            };
            _context.Alumni.Add(alumni);
            return alumni;
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
