using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Alumni_Management_System.Data;
using Alumni_Management_System.Models;
using Microsoft.AspNetCore.Identity;
using OfficeOpenXml;

namespace Alumni_Management_System.Controllers
{
    [Authorize]
    public class AlumniController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public AlumniController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return RedirectToAction("Login", "Account");

            var roles = await _userManager.GetRolesAsync(currentUser);
            IQueryable<Alumni> alumniQuery = _context.Alumni.Include(a => a.User);

            if (roles.Contains(Constants.AlumniRole))
                alumniQuery = alumniQuery.Where(a => a.Privacy == false);

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                // Split on comma, trim each token, remove empties
                // e.g. "john,2026" → ["john", "2026"]
                // e.g. "john@email.com" → ["john@email.com"]
                var tokens = searchString
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim())
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .ToList();

                // Pull the full dataset into memory once so we can do
                // ToString() comparisons (EF cannot translate those to SQL)
                var allAlumni = await alumniQuery.ToListAsync();

                // Each token must match at least one field on the same record.
                // All tokens must match (AND logic across tokens) so that
                // "john,2026" only returns Johns who graduated in 2026.
                var filtered = allAlumni.Where(a =>
                    tokens.All(token =>
                        MatchesToken(a, token)
                    )
                ).ToList();

                ViewData["CurrentFilter"] = searchString;
                ViewData["UserRole"] = roles.FirstOrDefault();

                var currentAlumni = await _context.Alumni
                    .FirstOrDefaultAsync(a => a.UserId == currentUser.Id || a.JagId == currentUser.JagId);
                ViewData["CurrentAlumniId"] = currentAlumni?.AlumniId;

                return View(filtered);
            }

            ViewData["CurrentFilter"] = searchString;
            ViewData["UserRole"] = roles.FirstOrDefault();

            var currentAlumniDefault = await _context.Alumni
                .FirstOrDefaultAsync(a => a.UserId == currentUser.Id || a.JagId == currentUser.JagId);
            ViewData["CurrentAlumniId"] = currentAlumniDefault?.AlumniId;

            return View(await alumniQuery.ToListAsync());
        }

        /// <summary>
        /// Returns true if the given token matches ANY searchable field on the alumni record.
        /// All string comparisons are case-insensitive.
        /// </summary>
        private static bool MatchesToken(Alumni a, string token)
        {
            var t = token.ToLower();

            return
                // Identity
                (!string.IsNullOrEmpty(a.JagId) && a.JagId.ToLower().Contains(t)) ||
                (!string.IsNullOrEmpty(a.Prefix) && a.Prefix.ToLower().Contains(t)) ||
                (!string.IsNullOrEmpty(a.FirstName) && a.FirstName.ToLower().Contains(t)) ||
                (!string.IsNullOrEmpty(a.PreferredFirstName) && a.PreferredFirstName.ToLower().Contains(t)) ||
                (!string.IsNullOrEmpty(a.LastName) && a.LastName.ToLower().Contains(t)) ||
                (!string.IsNullOrEmpty(a.Gender) && a.Gender.ToLower().Contains(t)) ||

                // Contact
                (!string.IsNullOrEmpty(a.StudentEmail) && a.StudentEmail.ToLower().Contains(t)) ||
                (!string.IsNullOrEmpty(a.PermanentEmail) && a.PermanentEmail.ToLower().Contains(t)) ||
                (!string.IsNullOrEmpty(a.Phone) && a.Phone.ToLower().Contains(t)) ||
                (!string.IsNullOrEmpty(a.SocialMediaAccount) && a.SocialMediaAccount.ToLower().Contains(t)) ||

                // Address
                (!string.IsNullOrEmpty(a.Address) && a.Address.ToLower().Contains(t)) ||
                (!string.IsNullOrEmpty(a.City) && a.City.ToLower().Contains(t)) ||
                (!string.IsNullOrEmpty(a.State) && a.State.ToLower().Contains(t)) ||
                (!string.IsNullOrEmpty(a.Postcode) && a.Postcode.ToLower().Contains(t)) ||
                (!string.IsNullOrEmpty(a.Country) && a.Country.ToLower().Contains(t)) ||

              // Academic
                (a.GraduationYear.ToString().Contains(t)) ||
                (a.AgeAtGraduation.ToString().Contains(t)) ||

                // Flags (searchable as "true"/"false" or "active"/"inactive"/"private"/"public")
                (t == "active" && a.IsActive == true) ||
                (t == "inactive" && a.IsActive == false) ||
                (t == "private" && a.Privacy == true) ||
                (t == "public" && a.Privacy == false);
        }

        public async Task<IActionResult> MyProfile()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return RedirectToAction("Login", "Account");

            var alumni = await _context.Alumni
                .FirstOrDefaultAsync(a => a.UserId == currentUser.Id || a.JagId == currentUser.JagId);
            if (alumni == null)
            {
                TempData["ErrorMessage"] = "Alumni profile not found.";
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("Edit", new { id = alumni.AlumniId });
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var alumni = await _context.Alumni
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.AlumniId == id);
            if (alumni == null) return NotFound();
            return View(alumni);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("AlumniId,JagId,Prefix,FirstName,PreferredFirstName,LastName,Gender,AgeAtGraduation,StudentEmail,PermanentEmail,Phone,Address,City,State,Postcode,Country,GraduationYear,SolicitationCode,SocialMediaAccount,Privacy,IsActive,LastUpdated")] Alumni alumni)
        {
            if (ModelState.IsValid)
            {
                alumni.LastUpdated = DateTime.Now;
                alumni.IsActive = false;
                _context.Add(alumni);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Alumni created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(alumni);
        }

        [Authorize(Roles = "Admin,Alumni")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var alumni = await _context.Alumni.Include(a => a.User).FirstOrDefaultAsync(a => a.AlumniId == id);
            if (alumni == null) return NotFound();

            if (!string.IsNullOrEmpty(alumni.JagId) && string.IsNullOrEmpty(alumni.UserId))
            {
                var linkedUser = await _userManager.Users.FirstOrDefaultAsync(u => u.JagId == alumni.JagId);
                if (linkedUser != null)
                {
                    alumni.UserId = linkedUser.Id;
                    _context.Update(alumni);
                    await _context.SaveChangesAsync();
                }
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return RedirectToAction("Login", "Account");

            var roles = await _userManager.GetRolesAsync(currentUser);
            if (roles.Contains(Constants.AlumniRole) && alumni.JagId != currentUser.JagId)
            {
                TempData["ErrorMessage"] = "You can only edit your own profile.";
                return RedirectToAction(nameof(Index));
            }

            return View(alumni);
        }

        [Authorize(Roles = "Admin,Alumni")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AlumniId,JagId,Prefix,FirstName,PreferredFirstName,LastName,Gender,AgeAtGraduation,StudentEmail,PermanentEmail,Phone,Address,City,State,Postcode,Country,GraduationYear,SolicitationCode,SocialMediaAccount,Privacy,IsActive,LastUpdated")] Alumni alumni)
        {
            if (id != alumni.AlumniId) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return RedirectToAction("Login", "Account");

            var roles = await _userManager.GetRolesAsync(currentUser);
            if (roles.Contains(Constants.AlumniRole) && alumni.JagId != currentUser.JagId)
            {
                TempData["ErrorMessage"] = "You can only edit your own profile.";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    alumni.LastUpdated = DateTime.Now;
                    _context.Update(alumni);

                    if (roles.Contains(Constants.AlumniRole) && currentUser.IsFirstLogin)
                    {
                        currentUser.IsFirstLogin = false;
                        await _userManager.UpdateAsync(currentUser);
                        TempData["SuccessMessage"] = "Profile updated! Welcome to the Alumni Management System.";
                    }
                    else
                    {
                        TempData["SuccessMessage"] = "Profile updated successfully!";
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AlumniExists(alumni.AlumniId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            return View(alumni);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var alumni = await _context.Alumni
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.AlumniId == id);
            if (alumni == null) return NotFound();
            return View(alumni);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var alumni = await _context.Alumni
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.AlumniId == id);

            if (alumni == null) return NotFound();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = alumni.User;

                var degrees = _context.AlumniDegrees.Where(d => d.AlumniId == id);
                _context.AlumniDegrees.RemoveRange(degrees);

                var employments = _context.AlumniEmployments.Where(e => e.AlumniId == id);
                _context.AlumniEmployments.RemoveRange(employments);

                var internships = _context.AlumniInternships.Where(i => i.AlumniId == id);
                _context.AlumniInternships.RemoveRange(internships);

                var organizations = _context.AlumniOrganizations.Where(o => o.AlumniId == id);
                _context.AlumniOrganizations.RemoveRange(organizations);

                var messages = _context.AlumniMessages.Where(m => m.AlumniId == id);
                _context.AlumniMessages.RemoveRange(messages);

                _context.Alumni.Remove(alumni);

                if (user != null)
                {
                    var result = await _userManager.DeleteAsync(user);
                    if (!result.Succeeded)
                        throw new Exception("Failed to delete associated user account.");
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                TempData["SuccessMessage"] = $"Alumni '{alumni.FirstName} {alumni.LastName}' and all related records deleted successfully. Registry entry kept for re-import.";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = "Error during deletion: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public IActionResult BulkImport() => View();

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkImport(Microsoft.AspNetCore.Http.IFormFile excelFile)
        {
            if (excelFile == null || excelFile.Length == 0)
            {
                TempData["ErrorMessages"] = "Please select an Excel file to upload.";
                return View();
            }

            var importErrors = new List<string>();
            var successCount = 0;
            var updatedCount = 0;

            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using var stream = new MemoryStream();
                await excelFile.CopyToAsync(stream);
                stream.Position = 0;

                using var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();

                if (worksheet == null)
                {
                    TempData["ErrorMessages"] = "Excel file has no worksheets.";
                    return View();
                }

                var rowCount = worksheet.Dimension?.End.Row ?? 0;
                var colCount = worksheet.Dimension?.End.Column ?? 0;

                // Build header map from row 1
                var headerMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                for (int col = 1; col <= colCount; col++)
                {
                    var header = worksheet.Cells[1, col].Value?.ToString()?.Trim();
                    if (!string.IsNullOrWhiteSpace(header) && !headerMap.ContainsKey(header))
                        headerMap[header] = col;
                }

                string GetCell(int row, string colName)
                {
                    if (!headerMap.TryGetValue(colName, out var colIndex))
                        return string.Empty;
                    var cellValue = worksheet.Cells[row, colIndex].Value;
                    if (cellValue == null) return string.Empty;
                    if (cellValue is double d) return ((long)d).ToString();
                    return cellValue.ToString()?.Trim() ?? string.Empty;
                }

                decimal? GetGpa(int row, string colName)
                {
                    if (!headerMap.TryGetValue(colName, out var colIndex)) return null;
                    var cellValue = worksheet.Cells[row, colIndex].Value;
                    if (cellValue == null) return null;
                    if (cellValue is double d) return (decimal)d;
                    if (decimal.TryParse(cellValue.ToString(), out var parsed)) return parsed;
                    return null;
                }

                for (var row = 2; row <= rowCount; row++)
                {
                    try
                    {
                        var jagId = GetCell(row, "ID");
                        var prefix = GetCell(row, "Name Prefix");
                        var firstName = GetCell(row, "First Name");
                        var lastName = GetCell(row, "Last Name");
                        var gender = GetCell(row, "Sex");
                        var ageText = GetCell(row, "Age");
                        var studentEmail = GetCell(row, "UNIV Email");
                        var permEmail = GetCell(row, "OTH Email");
                        var major = GetCell(row, "Major");
                        var degreeCode = GetCell(row, "Deg");
                        var gradText = GetCell(row, "Grad");
                        var street1 = GetCell(row, "Street Line 1");
                        var street2 = GetCell(row, "Street Line 2");
                        var city = GetCell(row, "City");
                        var state = GetCell(row, "State");
                        var zip = GetCell(row, "Zip");
                        var gpa = GetGpa(row, "Inst GPA");

                        // Skip blank or legend rows
                        if (string.IsNullOrWhiteSpace(jagId) && string.IsNullOrWhiteSpace(firstName))
                            continue;
                        if (jagId == "X" || firstName == "X")
                            continue;

                        // Validate JagId
                        if (string.IsNullOrEmpty(jagId))
                        {
                            importErrors.Add($"Row {row}: ID is required — skipped.");
                            continue;
                        }

                        if (!System.Text.RegularExpressions.Regex.IsMatch(jagId, @"^J\d+$",
                            System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                        {
                            importErrors.Add($"Row {row}: ID '{jagId}' invalid (must be J + digits) — skipped.");
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(firstName))
                        {
                            importErrors.Add($"Row {row}: First Name required — skipped.");
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(lastName))
                        {
                            importErrors.Add($"Row {row}: Last Name required — skipped.");
                            continue;
                        }

                        // Parse graduation year for this row — used in both paths below
                        int graduationYear = 0;
                        if (!string.IsNullOrEmpty(gradText) && gradText.Length >= 4)
                            int.TryParse(gradText.Substring(0, 4), out graduationYear);

                        var conferredDate = graduationYear > 0
                            ? DateOnly.FromDateTime(new DateTime(graduationYear, 1, 1))
                            : DateOnly.FromDateTime(DateTime.UtcNow);

                        // Parse remaining common fields
                        int age = 0;
                        if (!string.IsNullOrEmpty(ageText))
                            int.TryParse(ageText, out age);

                        var finalEmail = !string.IsNullOrEmpty(permEmail)
                            ? permEmail
                            : (!string.IsNullOrEmpty(studentEmail)
                                ? studentEmail
                                : $"{jagId.ToLower()}@placeholder.com");

                        var fullAddress = string.IsNullOrEmpty(street2)
                            ? street1
                            : $"{street1}, {street2}";

                        // Check whether alumni and registry records already exist
                        var alumniExists = await _context.Alumni.AnyAsync(a => a.JagId == jagId);
                        var registryEntry = await _context.AlumniRegistries.FirstOrDefaultAsync(r => r.JagId == jagId);

                        // ── PATH A: ALUMNI ALREADY EXISTS ─────────────────────────────────────
                        // Do NOT re-insert the alumni record. Instead, check whether the degree
                        // from this import row is a new one and add it if so.
                        //
                        // Degree identity is determined solely by (DegreeType + MajorFieldOfStudy),
                        // which maps to a single DegreeProgram row and therefore a single DegreeId.
                        //
                        // Duplicate rules applied here:
                        //   same degree + same year   → skip  (exact duplicate import)
                        //   same degree + diff year   → skip  (same qualification, year doesn't make it new)
                        //   diff degree + same year   → add   (genuinely different qualification)
                        //   diff degree + diff year   → add   (genuinely different qualification)
                        //
                        // Because DegreeId already encodes (DegreeType + Major), checking whether
                        // that DegreeId is already linked to the alumni covers all four cases:
                        //   - same degree → same DegreeId → already linked → skip
                        //   - diff degree → different DegreeId → not linked → add
                        // The graduation year plays no role in the duplicate decision.

                        if (alumniExists)
                        {
                            var existingAlumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == jagId);

                            if (existingAlumni != null && !string.IsNullOrEmpty(major))
                            {
                                // Find or create the DegreeProgram for this import row
                                var degreeProgram = await _context.DegreePrograms.FirstOrDefaultAsync(dp =>
                                    dp.DegreeType.ToLower() == (degreeCode ?? "").ToLower() &&
                                    dp.MajorFieldOfStudy.ToLower() == major.ToLower());

                                if (degreeProgram == null)
                                {
                                    degreeProgram = new DegreeProgram
                                    {
                                        Institution = "University of South Alabama",
                                        DegreeType = string.IsNullOrEmpty(degreeCode) ? "Unknown" : degreeCode,
                                        MajorFieldOfStudy = major,
                                        Department = major
                                    };
                                    _context.DegreePrograms.Add(degreeProgram);
                                    await _context.SaveChangesAsync();
                                }

                                // Is this exact degree (by DegreeId) already on the alumni's record?
                                bool degreeAlreadyLinked = await _context.AlumniDegrees.AnyAsync(ad =>
                                    ad.AlumniId == existingAlumni.AlumniId &&
                                    ad.DegreeId == degreeProgram.DegreeId);

                                if (!degreeAlreadyLinked)
                                {
                                    // Different degree → add it regardless of year
                                    _context.AlumniDegrees.Add(new AlumniDegree
                                    {
                                        AlumniId = existingAlumni.AlumniId,
                                        DegreeId = degreeProgram.DegreeId,
                                        DateConferred = conferredDate,
                                        Gpa = gpa,
                                        YearsToCompleteDegree = null,
                                        EmploymentWhileStudying = null,
                                        DegreeSpecificJob = null,
                                        ParticipatedInResearch = null,
                                        JobSecuredUponGraduation = null,
                                        AttendedOrPlansGradSchool = null
                                    });
                                    await _context.SaveChangesAsync();

                                    // Tag as a degree-added notice — will appear in success banner
                                    importErrors.Add($"Row {row}: '{jagId}' already exists — new degree '{degreeCode} in {major}' (conferred {conferredDate.Year}) added successfully.");
                                }
                                // else: same degree already linked → skipped without adding
                            }

                            // Keep registry names current regardless
                            if (registryEntry != null)
                            {
                                registryEntry.FirstName = firstName;
                                registryEntry.LastName = lastName;
                                _context.AlumniRegistries.Update(registryEntry);
                                await _context.SaveChangesAsync();
                            }

                            continue;
                        }

                        // ── PATH B: NEW ALUMNI ────────────────────────────────────────────────
                        var alumniEntity = new Alumni
                        {
                            JagId = jagId,
                            Prefix = string.IsNullOrEmpty(prefix) ? null : prefix,
                            FirstName = firstName,
                            LastName = lastName,
                            Gender = string.IsNullOrEmpty(gender) ? null : gender,
                            AgeAtGraduation = age > 0 ? age : null,
                            StudentEmail = string.IsNullOrEmpty(studentEmail) ? null : studentEmail,
                            PermanentEmail = finalEmail,
                            Address = string.IsNullOrEmpty(fullAddress) ? null : fullAddress,
                            City = string.IsNullOrEmpty(city) ? null : city,
                            State = string.IsNullOrEmpty(state) ? null : state,
                            Postcode = string.IsNullOrEmpty(zip) ? null : zip,
                            Country = "USA",
                            GraduationYear = graduationYear,
                            SolicitationCode = true,
                            Privacy = false,
                            IsActive = true,
                            LastUpdated = DateTime.Now
                        };

                        _context.Alumni.Add(alumniEntity);

                        // Registry: update name if entry exists, otherwise create it
                        if (registryEntry != null)
                        {
                            registryEntry.FirstName = firstName;
                            registryEntry.LastName = lastName;
                            _context.AlumniRegistries.Update(registryEntry);
                        }
                        else
                        {
                            _context.AlumniRegistries.Add(new AlumniRegistry
                            {
                                JagId = jagId,
                                FirstName = firstName,
                                LastName = lastName,
                                AccountCreated = false
                            });
                        }

                        // Save now so AlumniId is generated before linking degrees
                        await _context.SaveChangesAsync();

                        // Find or create DegreeProgram and link to the new alumni
                        if (!string.IsNullOrEmpty(major))
                        {
                            var degreeProgram = await _context.DegreePrograms.FirstOrDefaultAsync(dp =>
                                dp.DegreeType.ToLower() == (degreeCode ?? "").ToLower() &&
                                dp.MajorFieldOfStudy.ToLower() == major.ToLower());

                            if (degreeProgram == null)
                            {
                                degreeProgram = new DegreeProgram
                                {
                                    Institution = "University of South Alabama",
                                    DegreeType = string.IsNullOrEmpty(degreeCode) ? "Unknown" : degreeCode,
                                    MajorFieldOfStudy = major,
                                    Department = major
                                };
                                _context.DegreePrograms.Add(degreeProgram);
                                await _context.SaveChangesAsync();
                            }

                            bool degreeExists = await _context.AlumniDegrees.AnyAsync(ad =>
                                ad.AlumniId == alumniEntity.AlumniId &&
                                ad.DegreeId == degreeProgram.DegreeId);

                            if (!degreeExists)
                            {
                                _context.AlumniDegrees.Add(new AlumniDegree
                                {
                                    AlumniId = alumniEntity.AlumniId,
                                    DegreeId = degreeProgram.DegreeId,
                                    DateConferred = conferredDate,
                                    Gpa = gpa,
                                    YearsToCompleteDegree = null,
                                    EmploymentWhileStudying = null,
                                    DegreeSpecificJob = null,
                                    ParticipatedInResearch = null,
                                    JobSecuredUponGraduation = null,
                                    AttendedOrPlansGradSchool = null
                                });

                                await _context.SaveChangesAsync();
                            }
                        }

                        if (registryEntry != null)
                            updatedCount++; // re-imported after a previous delete
                        else
                            successCount++; // brand new alumni
                    }
                    catch (Exception rowEx)
                    {
                        var msg = rowEx.InnerException?.Message ?? rowEx.Message;
                        importErrors.Add($"Row {row}: Error — {msg}");

                        foreach (var entry in _context.ChangeTracker.Entries()
                            .Where(e => e.State == Microsoft.EntityFrameworkCore.EntityState.Added)
                            .ToList())
                        {
                            entry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessages"] = $"Import failed: {ex.InnerException?.Message ?? ex.Message}";
                return View();
            }

            // Separate degree-added notices from real errors so they appear in green, not red
            var degreeAddedNotices = importErrors
                .Where(e => e.Contains("added successfully"))
                .ToList();
            var actualErrors = importErrors
                .Where(e => !e.Contains("added successfully"))
                .ToList();

            if (actualErrors.Any())
                TempData["ErrorMessages"] = string.Join("<br/>", actualErrors);

            var summary = new List<string>();
            if (successCount > 0) summary.Add($"{successCount} new alumni imported");
            if (updatedCount > 0) summary.Add($"{updatedCount} previously deleted alumni re-imported");
            if (degreeAddedNotices.Any()) summary.Add($"{degreeAddedNotices.Count} new degree(s) added to existing alumni");

            var successMsg = summary.Any()
                ? $"Bulk import completed. {string.Join(", ", summary)}."
                : "No new alumni were imported.";

            if (degreeAddedNotices.Any())
                successMsg += "<br/>" + string.Join("<br/>", degreeAddedNotices);

            TempData["SuccessMessage"] = successMsg;

            return RedirectToAction(nameof(Index));
        }

        private bool AlumniExists(int id) => _context.Alumni.Any(e => e.AlumniId == id);
    }
}