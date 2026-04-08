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

            if (!string.IsNullOrEmpty(searchString))
            {
                alumniQuery = alumniQuery.Where(a =>
                    a.FirstName.Contains(searchString) ||
                    a.LastName.Contains(searchString) ||
                    a.JagId.Contains(searchString) ||
                    a.PermanentEmail.Contains(searchString));
            }

            ViewData["CurrentFilter"] = searchString;
            ViewData["UserRole"] = roles.FirstOrDefault();

            var currentAlumni = await _context.Alumni
                .FirstOrDefaultAsync(a => a.UserId == currentUser.Id || a.JagId == currentUser.JagId);
            ViewData["CurrentAlumniId"] = currentAlumni?.AlumniId;

            return View(await alumniQuery.ToListAsync());
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

                // ?? Delete all related alumni records first ??
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

                // ?? Delete Alumni (NOT registry — kept for re-import) ??
                _context.Alumni.Remove(alumni);

                // ?? Delete associated user account if exists ??
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
                        var degreeCode = GetCell(row, "Degree");
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

                        if (string.IsNullOrWhiteSpace(lastName))
                        {
                            importErrors.Add($"Row {row}: Last Name required — skipped.");
                            continue;
                        }

                        // Parse common fields
                        int age = 0;
                        if (!string.IsNullOrEmpty(ageText))
                            int.TryParse(ageText, out age);

                        int graduationYear = 0;
                        if (!string.IsNullOrEmpty(gradText) && gradText.Length >= 4)
                            int.TryParse(gradText.Substring(0, 4), out graduationYear);

                        var finalEmail = !string.IsNullOrEmpty(permEmail)
                            ? permEmail
                            : (!string.IsNullOrEmpty(studentEmail)
                                ? studentEmail
                                : $"{jagId.ToLower()}@placeholder.com");

                        var fullAddress = string.IsNullOrEmpty(street2)
                            ? street1
                            : $"{street1}, {street2}";

                        // ?? Check if Alumni already exists ??
                        var alumniExists = await _context.Alumni.AnyAsync(a => a.JagId == jagId);

                        // ?? Check if Registry entry exists ??
                        var registryEntry = await _context.AlumniRegistries.FirstOrDefaultAsync(r => r.JagId == jagId);

                        if (alumniExists)
                        {
                            // Alumni already in DB — skip alumni insert but update registry if needed
                            if (registryEntry != null)
                            {
                                registryEntry.FirstName = firstName;
                                registryEntry.LastName = lastName;
                                _context.AlumniRegistries.Update(registryEntry);
                                await _context.SaveChangesAsync();
                                importErrors.Add($"Row {row}: '{jagId}' already exists in Alumni — skipped. Registry updated.");
                            }
                            continue;
                        }

                        // ?? Alumni does NOT exist — create it (re-import after delete) ??
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
                            SolicitationCode = false,
                            Privacy = false,
                            IsActive = true,
                            LastUpdated = DateTime.Now
                        };

                        _context.Alumni.Add(alumniEntity);

                        // ?? Registry: update if exists, create if not ??
                        if (registryEntry != null)
                        {
                            // Update existing registry entry with latest name
                            registryEntry.FirstName = firstName;
                            registryEntry.LastName = lastName;
                            _context.AlumniRegistries.Update(registryEntry);
                        }
                        else
                        {
                            // Create new registry entry
                            _context.AlumniRegistries.Add(new AlumniRegistry
                            {
                                JagId = jagId,
                                FirstName = firstName,
                                LastName = lastName,
                                AccountCreated = false
                            });
                        }

                        // Save to get AlumniId
                        await _context.SaveChangesAsync();

                        // ?? Find or create DegreeProgram ??
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
                                    DateConferred = graduationYear > 0
                                        ? DateOnly.FromDateTime(new DateTime(graduationYear, 1, 1))
                                        : DateOnly.FromDateTime(DateTime.UtcNow),
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
                            updatedCount++; // was re-imported after delete
                        else
                            successCount++; // brand new import
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

            if (importErrors.Any())
                TempData["ErrorMessages"] = string.Join("<br/>", importErrors);

            var summary = new List<string>();
            if (successCount > 0) summary.Add($"{successCount} new alumni imported");
            if (updatedCount > 0) summary.Add($"{updatedCount} previously deleted alumni re-imported");

            TempData["SuccessMessage"] = summary.Any()
                ? $"Bulk import completed. {string.Join(", ", summary)}."
                : "No new alumni were imported.";

            return RedirectToAction(nameof(Index));
        }

        private bool AlumniExists(int id) => _context.Alumni.Any(e => e.AlumniId == id);
    }
}