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

        // Columns the importer understands, in the order the downloadable
        // template uses. Shared by the template download and the instructions
        // on the Bulk Import page so the two can't drift apart.
        public static readonly IReadOnlyList<(string Header, string Sample, bool Required, string Description)> TemplateColumns = new[]
        {
            ("ID", "J00123456", true, "JAG ID - \"J\" followed by numbers. Existing JAG IDs are updated, new ones are created."),
            ("First Name", "John", true, "First name"),
            ("Last Name", "Doe", true, "Last name"),
            ("UNIV Email", "jd1234@jagmail.southalabama.edu", false, "University email - saved as the email on record"),
            ("OTH Email", "john.doe@example.com", false, "Other email - used only when UNIV Email is blank"),
            ("CL", "CS", false, "College code (e.g. CS = School of Computing) - needs Major too"),
            ("Major", "Computer Science", false, "Major / department - needs CL too"),
            ("Degree", "BSCSC", false, "Degree code (defaults to the Major when blank)"),
            ("Grad", "202610", false, "Graduation term (202610) or year (2026)"),
            ("Inst GPA", "3.45", false, "Institutional GPA for the degree"),
            ("Street Line 1", "123 Main Street", false, "Street address"),
            ("Street Line 2", "Apt 4", false, "Apartment / suite (joined to Street Line 1)"),
            ("City", "Mobile", false, "City"),
            ("State", "AL", false, "State"),
            ("Zip", "36608", false, "Zip / postal code"),
            ("Country", "USA", false, "Country"),
            ("Phone", "251-555-0123", false, "Phone number"),
        };

        // GET: AlumniRegistries/DownloadTemplate?format=xlsx|csv
        [Authorize(Roles = "Admin")]
        public IActionResult DownloadTemplate(string format = "xlsx")
        {
            if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
            {
                string Csv(string v) => v.Contains(',') || v.Contains('"') ? $"\"{v.Replace("\"", "\"\"")}\"" : v;
                var csv = new StringBuilder();
                csv.AppendLine(string.Join(",", TemplateColumns.Select(c => Csv(c.Header))));
                csv.AppendLine(string.Join(",", TemplateColumns.Select(c => Csv(c.Sample))));
                return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "Alumni_Bulk_Import_Template.csv");
            }

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Alumni");
            for (int i = 0; i < TemplateColumns.Count; i++)
            {
                var header = sheet.Cells[1, i + 1];
                header.Value = TemplateColumns[i].Header;
                header.Style.Font.Bold = true;
                // Stored as text so values like the JAG ID, Zip and Grad term
                // keep their exact form instead of being turned into numbers.
                sheet.Cells[2, i + 1].Style.Numberformat.Format = "@";
                sheet.Cells[2, i + 1].Value = TemplateColumns[i].Sample;
                if (TemplateColumns[i].Required)
                {
                    header.AddComment("Required", "Alumni Management System");
                }
            }
            sheet.View.FreezePanes(2, 1);
            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();

            return File(package.GetAsByteArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Alumni_Bulk_Import_Template.xlsx");
        }

        // One parsed row from either the CSV or Excel file.
        private sealed class ImportRow
        {
            public string JagId, FirstName, LastName, GradRaw, Email, Address, City, State, Postcode, Country, Phone;
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

            var errors = new List<string>();
            var updates = new List<string>();
            int createdCount = 0;
            int updatedCount = 0;
            int unchangedCount = 0;
            int errorCount = 0;

            // A JAG ID that's already in the system (or appears twice in the
            // same file) isn't skipped - the existing record is updated with
            // the file's values and the change is reported back. These caches
            // hold the records touched so far in this import, since nothing is
            // saved until the end and a DB lookup wouldn't see them yet.
            var registryCache = new Dictionary<string, AlumniRegistry>(StringComparer.OrdinalIgnoreCase);
            var alumniCache = new Dictionary<string, Alumni>(StringComparer.OrdinalIgnoreCase);
            var firstRowForJagId = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var emailOwners = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // Alumni whose College field was touched by a degree row during
            // this import - re-synced to their most-recently-conferred
            // degree's college once everything is saved (see below), instead
            // of just keeping whichever row happened to be processed last.
            var alumniWithDegreeRows = new List<Alumni>();

            // Creates the Registry + Alumni records for a new JAG ID, or
            // updates the existing ones with any non-blank values from the
            // file (blank cells never wipe out existing data). Returns null
            // when the row was rejected; otherwise the Alumni plus a list of
            // the changes made, which the caller reports once it's done with
            // the row.
            async Task<(Alumni Alumni, bool IsNew, List<string> Changes)?> UpsertAsync(string rowLabel, ImportRow r)
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(r.JagId, @"^J\d+$"))
                {
                    errors.Add($"{rowLabel}: Invalid JAG ID format '{r.JagId}' - must start with J followed by numbers");
                    return null;
                }

                if (!registryCache.TryGetValue(r.JagId, out var registry))
                {
                    registry = await _context.AlumniRegistries.FirstOrDefaultAsync(a => a.JagId == r.JagId);
                }
                if (!alumniCache.TryGetValue(r.JagId, out var alumni))
                {
                    alumni = await _context.Alumni.Include(a => a.AlumniDegrees).FirstOrDefaultAsync(a => a.JagId == r.JagId);
                }

                // A JAG ID tied to an existing non-Alumni account (e.g. an
                // Admin/Staff account) can't become an Alumni record - it's a
                // one-JAG-ID-per-account system.
                if (alumni == null && await _context.Users.AnyAsync(u => u.JagId == r.JagId))
                {
                    errors.Add($"{rowLabel}: JAG ID {r.JagId} is already in use by a non-alumni account - skipped");
                    return null;
                }

                var isNew = registry == null && alumni == null;
                var changes = new List<string>();

                firstRowForJagId.TryAdd(r.JagId, rowLabel);

                // Emails must stay unique per person. A new record can't be
                // created with someone else's email; an existing record just
                // keeps its current email and the conflict is reported.
                var email = r.Email;
                if (!string.IsNullOrWhiteSpace(email))
                {
                    var takenInFile = emailOwners.TryGetValue(email, out var owner) && !owner.Equals(r.JagId, StringComparison.OrdinalIgnoreCase);
                    var takenInDb = !takenInFile && await _context.Alumni.AnyAsync(a => a.PermanentEmail == email && a.JagId != r.JagId);
                    if (takenInFile || takenInDb)
                    {
                        if (isNew)
                        {
                            errors.Add($"{rowLabel}: Email {email} already belongs to another alumni record - skipped");
                            return null;
                        }
                        errors.Add($"{rowLabel}: Email {email} already belongs to another alumni record - kept the existing email for {r.JagId}, other fields still updated");
                        email = null;
                    }
                    else
                    {
                        emailOwners[email] = r.JagId;
                    }
                }

                void Apply(string label, string current, string incoming, Action<string> set)
                {
                    if (string.IsNullOrWhiteSpace(incoming) || string.Equals(current?.Trim(), incoming, StringComparison.Ordinal))
                    {
                        return;
                    }
                    set(incoming);
                    changes.Add(string.IsNullOrWhiteSpace(current) ? $"{label} set to '{incoming}'" : $"{label} '{current}' → '{incoming}'");
                }

                if (registry == null)
                {
                    registry = new AlumniRegistry { JagId = r.JagId, FirstName = r.FirstName, LastName = r.LastName, AccountCreated = false };
                    _context.AlumniRegistries.Add(registry);
                    if (!isNew) changes.Add("registry entry created");
                }
                else
                {
                    Apply("Registry first name", registry.FirstName, r.FirstName, v => registry.FirstName = v);
                    Apply("Registry last name", registry.LastName, r.LastName, v => registry.LastName = v);
                }
                registryCache[r.JagId] = registry;

                // "Graduation Year" can arrive as a plain year ("2023") or, from the
                // richer registrar export, a 6-digit term code ("202610") - either
                // way the year is the first 4 digits.
                var gradYear = 0;
                if (!string.IsNullOrWhiteSpace(r.GradRaw))
                {
                    var yearPart = r.GradRaw.Length >= 4 ? r.GradRaw.Substring(0, 4) : r.GradRaw;
                    int.TryParse(yearPart, out gradYear);
                }

                // Bulk import populates the actual Alumni table too (not just
                // the Registry stub) so the JagId -> RegisterAlumni self-service
                // flow (which requires an existing Alumni row) works, and so
                // Admin/Staff can see them under Alumni immediately.
                if (alumni == null)
                {
                    alumni = new Alumni
                    {
                        JagId = r.JagId,
                        FirstName = r.FirstName,
                        LastName = r.LastName,
                        PermanentEmail = string.IsNullOrWhiteSpace(email) ? $"{r.JagId.ToLower()}@pending.import" : email,
                        GraduationYear = gradYear,
                        Address = r.Address,
                        City = r.City,
                        State = r.State,
                        Postcode = r.Postcode,
                        Country = r.Country,
                        Phone = r.Phone,
                        IsActive = false,
                        Privacy = true,
                        SolicitationCode = false,
                        LastUpdated = DateTime.Now
                    };
                    _context.Alumni.Add(alumni);
                    if (!isNew) changes.Add("alumni record created");
                }
                else
                {
                    var a = alumni;
                    Apply("First name", a.FirstName, r.FirstName, v => a.FirstName = v);
                    Apply("Last name", a.LastName, r.LastName, v => a.LastName = v);
                    Apply("Email", a.PermanentEmail, email, v => a.PermanentEmail = v);
                    Apply("Address", a.Address, r.Address, v => a.Address = v);
                    Apply("City", a.City, r.City, v => a.City = v);
                    Apply("State", a.State, r.State, v => a.State = v);
                    Apply("Zip", a.Postcode, r.Postcode, v => a.Postcode = v);
                    Apply("Country", a.Country, r.Country, v => a.Country = v);
                    Apply("Phone", a.Phone, r.Phone, v => a.Phone = v);
                    if (gradYear > 0 && gradYear != a.GraduationYear)
                    {
                        changes.Add(a.GraduationYear > 0 ? $"Graduation year {a.GraduationYear} → {gradYear}" : $"Graduation year set to {gradYear}");
                        a.GraduationYear = gradYear;
                    }
                }
                alumniCache[r.JagId] = alumni;

                return (alumni, isNew, changes);
            }

            void Report(string rowLabel, string jagId, bool isNew, List<string> changes)
            {
                if (isNew)
                {
                    createdCount++;
                }
                else if (changes.Count > 0)
                {
                    updatedCount++;
                    var repeatNote = firstRowForJagId.TryGetValue(jagId, out var firstRow) && firstRow != rowLabel
                        ? $" [also on {firstRow.ToLower()} - this row's values applied]"
                        : "";
                    updates.Add($"{rowLabel} ({jagId}){repeatNote}: {string.Join("; ", changes)}");
                }
                else
                {
                    unchangedCount++;
                }
            }

            try
            {
                var fileExtension = Path.GetExtension(file.FileName).ToLower();

                if (fileExtension == ".csv")
                {
                    using (var reader = new StreamReader(file.OpenReadStream()))
                    {
                        var headerLine = await reader.ReadLineAsync();

                        // The 6-column simple template has no header names worth
                        // mapping (and is handled by fixed position below), but a
                        // richer CSV export can carry extra columns - like Address,
                        // City, State, Postcode, Country, Phone - in any order, so
                        // map header name to column index the same way the Excel
                        // path does, and fall back to fixed position when absent.
                        var csvHeaders = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                        if (!string.IsNullOrWhiteSpace(headerLine))
                        {
                            var headerValues = ParseCsvLine(headerLine);
                            for (int c = 0; c < headerValues.Length; c++)
                            {
                                var h = headerValues[c]?.Trim();
                                if (!string.IsNullOrEmpty(h) && !csvHeaders.ContainsKey(h))
                                {
                                    csvHeaders[h] = c;
                                }
                            }
                        }

                        string GetCsvField(string[] values, params string[] headerNames)
                        {
                            foreach (var name in headerNames)
                            {
                                if (csvHeaders.TryGetValue(name, out var idx) && idx < values.Length)
                                {
                                    var v = values[idx]?.Trim();
                                    if (!string.IsNullOrWhiteSpace(v)) return v;
                                }
                            }
                            return null;
                        }

                        // Header is line 1, so the first data line is line 2.
                        int lineNumber = 1;
                        while (!reader.EndOfStream)
                        {
                            var line = await reader.ReadLineAsync();
                            lineNumber++;
                            if (string.IsNullOrWhiteSpace(line)) continue;

                            var rowLabel = $"Row {lineNumber}";
                            var values = ParseCsvLine(line);

                            if (values.Length < 3)
                            {
                                errors.Add($"{rowLabel} skipped - insufficient columns: {line}");
                                errorCount++;
                                continue;
                            }

                            try
                            {
                                var importRow = new ImportRow
                                {
                                    JagId = GetCsvField(values, "JAG ID", "ID") ?? values[0].Trim(),
                                    FirstName = GetCsvField(values, "First Name") ?? values[1].Trim(),
                                    LastName = GetCsvField(values, "Last Name") ?? values[2].Trim(),
                                    GradRaw = GetCsvField(values, "Grad", "Graduation Year") ?? (values.Length > 3 ? values[3].Trim() : null),
                                    Email = GetCsvField(values, "UNIV Email", "Email On Record", "OTH Email") ?? (values.Length > 5 ? values[5].Trim() : null),
                                    Address = CombineAddressLines(
                                        GetCsvField(values, "Address", "Street Address", "Address Line 1", "Street Line 1", "Mailing Address"),
                                        GetCsvField(values, "Address Line 2", "Street Line 2")),
                                    City = GetCsvField(values, "City"),
                                    State = GetCsvField(values, "State"),
                                    Postcode = GetCsvField(values, "Zip", "Zip Code", "Postal Code", "Postcode"),
                                    Country = GetCsvField(values, "Country"),
                                    Phone = GetCsvField(values, "Phone", "Phone Number")
                                };

                                var result = await UpsertAsync(rowLabel, importRow);
                                if (result == null)
                                {
                                    errorCount++;
                                    continue;
                                }

                                Report(rowLabel, importRow.JagId, result.Value.IsNew, result.Value.Changes);
                            }
                            catch (Exception ex)
                            {
                                errors.Add($"{rowLabel}: Error processing row: {line} - {ex.Message}");
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
                                var rowLabel = $"Row {row}";
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
                                        errors.Add($"{rowLabel} skipped - missing required field(s) (JAG ID/First Name/Last Name)");
                                        errorCount++;
                                        continue;
                                    }

                                    var importRow = new ImportRow
                                    {
                                        JagId = jagId,
                                        FirstName = firstName,
                                        LastName = lastName,
                                        GradRaw = GetCell(row, "Grad", "Graduation Year"),
                                        Email = GetCell(row, "UNIV Email", "Email On Record", "OTH Email"),
                                        Address = CombineAddressLines(
                                            GetCell(row, "Address", "Street Address", "Address Line 1", "Street Line 1", "Mailing Address"),
                                            GetCell(row, "Address Line 2", "Street Line 2")),
                                        City = GetCell(row, "City"),
                                        State = GetCell(row, "State"),
                                        Postcode = GetCell(row, "Zip", "Zip Code", "Postal Code", "Postcode"),
                                        Country = GetCell(row, "Country"),
                                        Phone = GetCell(row, "Phone", "Phone Number")
                                    };

                                    var result = await UpsertAsync(rowLabel, importRow);
                                    if (result == null)
                                    {
                                        errorCount++;
                                        continue;
                                    }

                                    var (alumni, isNew, changes) = result.Value;

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

                                        var gradRaw = importRow.GradRaw;
                                        if (gradRaw != null && gradRaw.Length >= 4 && int.TryParse(gradRaw.Substring(0, 4), out var gradYearForDegree))
                                        {
                                            decimal? gpa = null;
                                            var gpaRaw = GetCell(row, "Inst GPA");
                                            if (gpaRaw != null && decimal.TryParse(gpaRaw, out var parsedGpa))
                                            {
                                                gpa = parsedGpa;
                                            }

                                            var existingLink = alumni.AlumniDegrees.FirstOrDefault(ad => ad.Degree == degreeProgram)
                                                ?? (alumni.AlumniId != 0
                                                    ? await _context.AlumniDegrees.FirstOrDefaultAsync(ad => ad.AlumniId == alumni.AlumniId && ad.Degree == degreeProgram)
                                                    : null);

                                            if (existingLink == null)
                                            {
                                                _context.AlumniDegrees.Add(new AlumniDegree
                                                {
                                                    Alumni = alumni,
                                                    Degree = degreeProgram,
                                                    DateConferred = new DateOnly(gradYearForDegree, 5, 1),
                                                    Gpa = gpa
                                                });
                                                if (!isNew) changes.Add($"Degree {degreeCode} ({major}) added");
                                            }
                                            else
                                            {
                                                var conferred = new DateOnly(gradYearForDegree, 5, 1);
                                                if (existingLink.DateConferred != conferred)
                                                {
                                                    changes.Add($"{degreeCode} conferred date {existingLink.DateConferred} → {conferred}");
                                                    existingLink.DateConferred = conferred;
                                                }
                                                if (gpa != null && existingLink.Gpa != gpa)
                                                {
                                                    changes.Add(existingLink.Gpa == null ? $"{degreeCode} GPA set to {gpa}" : $"{degreeCode} GPA {existingLink.Gpa} → {gpa}");
                                                    existingLink.Gpa = gpa;
                                                }
                                            }
                                        }
                                    }

                                    Report(rowLabel, jagId, isNew, changes);
                                }
                                catch (Exception ex)
                                {
                                    errors.Add($"{rowLabel}: {ex.Message}");
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

                TempData["SuccessMessage"] = $"Import completed: {createdCount} new, {updatedCount} updated, {unchangedCount} already up to date, {errorCount} errors.";
                if (updates.Any())
                {
                    TempData["UpdateMessages"] = string.Join("<br/>", updates.Select(System.Net.WebUtility.HtmlEncode));
                }
                if (errors.Any())
                {
                    TempData["ErrorMessages"] = string.Join("<br/>", errors.Select(System.Net.WebUtility.HtmlEncode));
                }

                return RedirectToAction("Index", "Alumni");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error processing file: {ex.Message}");
                return View();
            }
        }

        // The registrar export splits the street across "Street Line 1" /
        // "Street Line 2" (e.g. "41 Bob Avenue" + "Apt 12") - Alumni has a
        // single Address field, so join them.
        private static string CombineAddressLines(string line1, string line2)
        {
            if (string.IsNullOrWhiteSpace(line2)) return line1;
            if (string.IsNullOrWhiteSpace(line1)) return line2;
            return $"{line1}, {line2}";
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
