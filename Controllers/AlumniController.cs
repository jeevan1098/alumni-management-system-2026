using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
            {
                return RedirectToAction("Login", "Account");
            }

            var roles = await _userManager.GetRolesAsync(currentUser);

            IQueryable<Alumni> alumniQuery = _context.Alumni.Include(a => a.User);

            if (roles.Contains(Constants.AlumniRole))
            {

                alumniQuery = alumniQuery.Where(a => a.Privacy == false);
            }

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

            var currentAlumni = await _context.Alumni.FirstOrDefaultAsync(a => a.UserId == currentUser.Id || a.JagId == currentUser.JagId);
            ViewData["CurrentAlumniId"] = currentAlumni?.AlumniId;

            return View(await alumniQuery.ToListAsync());
        }

        public async Task<IActionResult> MyProfile()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var alumni = await _context.Alumni.FirstOrDefaultAsync(a => a.UserId == currentUser.Id || a.JagId == currentUser.JagId);
            if (alumni == null)
            {
                TempData["ErrorMessage"] = "Alumni profile not found.";
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("Edit", new { id = alumni.AlumniId });
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alumni = await _context.Alumni
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.AlumniId == id);
            if (alumni == null)
            {
                return NotFound();
            }

            return View(alumni);
        }

        [Authorize(Roles = "Admin")] // Only Admin can create alumni manually
        public IActionResult Create()
        {

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // Only Admin can create alumni manually
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
            if (id == null)
            {
                return NotFound();
            }

            var alumni = await _context.Alumni.Include(a => a.User).FirstOrDefaultAsync(a => a.AlumniId == id);
            if (alumni == null)
            {
                return NotFound();
            }

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
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var roles = await _userManager.GetRolesAsync(currentUser);

            if (roles.Contains(Constants.AlumniRole))
            {

                if (alumni.JagId != currentUser.JagId)
                {
                    TempData["ErrorMessage"] = "You can only edit your own profile.";
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(alumni);
        }

        [Authorize(Roles = "Admin,Alumni")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AlumniId,JagId,Prefix,FirstName,PreferredFirstName,LastName,Gender,AgeAtGraduation,StudentEmail,PermanentEmail,Phone,Address,City,State,Postcode,Country,GraduationYear,SolicitationCode,SocialMediaAccount,Privacy,IsActive,LastUpdated")] Alumni alumni)
        {
            if (id != alumni.AlumniId)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var roles = await _userManager.GetRolesAsync(currentUser);

            if (roles.Contains(Constants.AlumniRole))
            {

                if (alumni.JagId != currentUser.JagId)
                {
                    TempData["ErrorMessage"] = "You can only edit your own profile.";
                    return RedirectToAction(nameof(Index));
                }
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
                        TempData["SuccessMessage"] = "Profile updated successfully! Welcome to the Alumni Management System.";
                    }
                    else
                    {
                        TempData["SuccessMessage"] = "Profile updated successfully!";
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AlumniExists(alumni.AlumniId))
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

            return View(alumni);
        }

        [Authorize(Roles = "Admin")] // Only Admin can delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alumni = await _context.Alumni
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.AlumniId == id);
            if (alumni == null)
            {
                return NotFound();
            }

            return View(alumni);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // Only Admin can delete
        public async Task<IActionResult> DeleteConfirmed(int id)
        {

            var alumni = await _context.Alumni
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.AlumniId == id);

            if (alumni == null)
            {
                return NotFound();
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {

                    var user = alumni.User;

                    _context.Alumni.Remove(alumni);

                    if (user != null)
                    {
                        var result = await _userManager.DeleteAsync(user);
                        if (!result.Succeeded)
                        {
                            throw new Exception("Failed to delete associated user account.");
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["SuccessMessage"] = "Alumni and associated user account deleted successfully!";
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    TempData["ErrorMessage"] = "Error during deletion: " + ex.Message;
                }
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")] // Only admin can access bulk import
        public IActionResult BulkImport()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")] // Only admin can access bulk import
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkImport(Microsoft.AspNetCore.Http.IFormFile excelFile)
        {
            if (excelFile == null || excelFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select an Excel file to upload.";
                return View();
            }

            var importErrors = new List<string>();
            var successCount = 0;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var stream = new MemoryStream())
            {
                await excelFile.CopyToAsync(stream);
                stream.Position = 0;

                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null)
                    {
                        TempData["ErrorMessage"] = "Excel file has no worksheets.";
                        return View();
                    }

                    var rowCount = worksheet.Dimension?.End.Row ?? 0;
                    var colCount = worksheet.Dimension?.End.Column ?? 0;

                    // Build header map from first row (case-insensitive, space-insensitive)
                    var headerMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                    for (int col = 1; col <= colCount; col++)
                    {
                        var headerValue = worksheet.Cells[1, col].Value?.ToString()?.Trim();
                        if (string.IsNullOrWhiteSpace(headerValue)) continue;
                        // Normalize header: remove spaces for matching
                        var normalizedHeader = System.Text.RegularExpressions.Regex.Replace(headerValue, @"\s+", "");
                        headerMap[normalizedHeader] = col;
                    }

                    // Helper function to get cell value by header name
                    string GetCellValueByHeader(int row, string headerName)
                    {
                        var normalizedHeader = System.Text.RegularExpressions.Regex.Replace(headerName, @"\s+", "");
                        if (headerMap.TryGetValue(normalizedHeader, out var colIndex))
                        {
                            return worksheet.Cells[row, colIndex].Value?.ToString()?.Trim() ?? string.Empty;
                        }
                        return string.Empty;
                    }

                    for (var row = 2; row <= rowCount; row++)
                    {
                        var jagId = GetCellValueByHeader(row, "JagId")?.Trim();
                        if (string.IsNullOrEmpty(jagId))
                        {
                            importErrors.Add($"Row {row}: JagId is required.");
                            continue;
                        }

                        var prefix = GetCellValueByHeader(row, "Name Prefix");
                        var firstName = GetCellValueByHeader(row, "First Name");
                        var preferredFirstName = GetCellValueByHeader(row, "Preferred First Name");
                        var lastName = GetCellValueByHeader(row, "Last Name");
                        var gender = GetCellValueByHeader(row, "Gender");
                        var ageAtGraduationText = GetCellValueByHeader(row, "Age At Graduation");
                        var studentEmail = GetCellValueByHeader(row, "Student Email");
                        var permanentEmail = GetCellValueByHeader(row, "Permanent Email");
                        var phone = GetCellValueByHeader(row, "Phone");
                        var address = GetCellValueByHeader(row, "Address");
                        var city = GetCellValueByHeader(row, "City");
                        var state = GetCellValueByHeader(row, "State");
                        var postcode = GetCellValueByHeader(row, "Postcode");
                        var country = GetCellValueByHeader(row, "Country");
                        var graduationYearText = GetCellValueByHeader(row, "GraduationYear");
                        var solicitationCodeText = GetCellValueByHeader(row, "SolicitationCode");
                        var socialMediaAccount = GetCellValueByHeader(row, "Social Media Account");
                        var privacyText = GetCellValueByHeader(row, "Privacy");
                        var isActiveText = GetCellValueByHeader(row, "IsActive");
                        var lastUpdatedText = GetCellValueByHeader(row, "LastUpdated");
                        var degreeType = GetCellValueByHeader(row, "DegreeType");
                        var major = GetCellValueByHeader(row, "Major");
                        var degreeProgram = GetCellValueByHeader(row, "DegreeProgram");
                        var educationInstitution = GetCellValueByHeader(row, "Education Institution");
                        var gpaText = GetCellValueByHeader(row, "GPA");

                        // Validate JagId format
                        if (!System.Text.RegularExpressions.Regex.IsMatch(jagId, @"^J00\d+$", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                        {
                            importErrors.Add($"Row {row}: JagId '{jagId}' is invalid. Expected format: J00... (e.g. J0012345).");
                            continue;
                        }

                        // Enforce unique JagId in AlumniRegistry (allow only one per JagId)
                        if (await _context.AlumniRegistries.AnyAsync(r => r.JagId == jagId))
                        {
                            importErrors.Add($"Row {row}: JagId '{jagId}' already exists in AlumniRegistry and cannot be imported again.");
                            continue;
                        }

                        // Enforce unique JagId in Alumni table (duplicate fails import)
                        if (await _context.Alumni.AnyAsync(a => a.JagId == jagId))
                        {
                            importErrors.Add($"Row {row}: JagId '{jagId}' already exists in Alumni and cannot be imported again.");
                            continue;
                        }

                        var graduationYear = 0;
                        if (!string.IsNullOrEmpty(graduationYearText))
                        {
                            // Extract first 4 digits to handle 6-digit timestamps or full dates
                            string yearText = graduationYearText.Trim().Substring(0, Math.Min(4, graduationYearText.Trim().Length));
                            if (!int.TryParse(yearText, out graduationYear))
                            {
                                importErrors.Add($"Row {row}: GraduationYear '{graduationYearText}' is invalid.");
                                continue;
                            }
                        }

                        var ageAtGraduation = 0;
                        if (!string.IsNullOrEmpty(ageAtGraduationText) && !int.TryParse(ageAtGraduationText, out ageAtGraduation))
                        {
                            importErrors.Add($"Row {row}: AgeAtGraduation '{ageAtGraduationText}' is invalid.");
                            continue;
                        }

                        var solicitationCode = false;
                        if (!string.IsNullOrEmpty(solicitationCodeText) && !bool.TryParse(solicitationCodeText, out solicitationCode))
                        {
                            importErrors.Add($"Row {row}: SolicitationCode '{solicitationCodeText}' is invalid (true/false expected).\n");
                            continue;
                        }

                        var privacy = false;
                        if (!string.IsNullOrEmpty(privacyText) && !bool.TryParse(privacyText, out privacy))
                        {
                            importErrors.Add($"Row {row}: Privacy '{privacyText}' is invalid (true/false expected).\n");
                            continue;
                        }

                        var isActive = false;
                        if (!string.IsNullOrEmpty(isActiveText) && !bool.TryParse(isActiveText, out isActive))
                        {
                            importErrors.Add($"Row {row}: IsActive '{isActiveText}' is invalid (true/false expected).\n");
                            continue;
                        }

                        var lastUpdated = DateTime.Now;
                        if (!string.IsNullOrEmpty(lastUpdatedText) && !DateTime.TryParse(lastUpdatedText, out lastUpdated))
                        {
                            importErrors.Add($"Row {row}: LastUpdated '{lastUpdatedText}' is invalid datetime.");
                            continue;
                        }

                        var alumniEntity = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == jagId);

                        if (alumniEntity == null)
                        {
                            alumniEntity = new Alumni
                            {
                                JagId = jagId,
                                Prefix = prefix,
                                FirstName = firstName,
                                PreferredFirstName = preferredFirstName,
                                LastName = lastName,
                                Gender = gender,
                                AgeAtGraduation = ageAtGraduation,
                                StudentEmail = studentEmail,
                                PermanentEmail = permanentEmail,
                                Phone = phone,
                                Address = address,
                                City = city,
                                State = state,
                                Postcode = postcode,
                                Country = country,
                                GraduationYear = graduationYear,
                                SolicitationCode = solicitationCode,
                                SocialMediaAccount = socialMediaAccount,
                                Privacy = privacy,
                                IsActive = isActive,
                                LastUpdated = lastUpdated
                            };

                            _context.Alumni.Add(alumniEntity);
                        }
                        else
                        {
                            importErrors.Add($"Row {row}: JagId '{jagId}' already exists; skipping Alumni insert and continuing with AlumniDegree/AlumniRegistry updates.");
                        }

                        // Ensure alumni registry entry exists for this JagId
                        var alumniRegistryEntity = await _context.AlumniRegistries.FirstOrDefaultAsync(r => r.JagId == jagId);
                        if (alumniRegistryEntity == null)
                        {
                            alumniRegistryEntity = new AlumniRegistry
                            {
                                JagId = jagId,
                                FirstName = firstName,
                                LastName = lastName,
                                AccountCreated = false
                            };
                            _context.AlumniRegistries.Add(alumniRegistryEntity);
                        }

                        // Determine degree program; allow existing or create new stub
                        DegreeProgram degreeProgramEntity = null;
                        var normalizedMajor = major?.Trim().ToLower();
                        var normalizedDegreeType = degreeType?.Trim().ToLower();
                        var normalizedInstitution = educationInstitution?.Trim();

                        if (!string.IsNullOrEmpty(normalizedDegreeType) && !string.IsNullOrEmpty(normalizedMajor))
                        {
                            degreeProgramEntity = await _context.DegreePrograms.FirstOrDefaultAsync(dp =>
                                dp.DegreeType.ToLower() == normalizedDegreeType &&
                                dp.MajorFieldOfStudy.ToLower() == normalizedMajor);
                        }

                        if (degreeProgramEntity == null && !string.IsNullOrEmpty(normalizedMajor))
                        {
                            degreeProgramEntity = await _context.DegreePrograms.FirstOrDefaultAsync(dp =>
                                dp.MajorFieldOfStudy.ToLower() == normalizedMajor);
                        }

                        if (degreeProgramEntity == null && !string.IsNullOrEmpty(normalizedDegreeType))
                        {
                            degreeProgramEntity = await _context.DegreePrograms.FirstOrDefaultAsync(dp =>
                                dp.DegreeType.ToLower() == normalizedDegreeType);
                        }

                        if (degreeProgramEntity == null)
                        {
                            degreeProgramEntity = new DegreeProgram
                            {
                                Institution = string.IsNullOrEmpty(normalizedInstitution) ? "Unknown" : normalizedInstitution,
                                DegreeType = string.IsNullOrEmpty(degreeType) ? "Unknown" : degreeType,
                                MajorFieldOfStudy = string.IsNullOrEmpty(major) ? "Unknown" : major,
                                Department = string.IsNullOrEmpty(major) ? "Unknown" : major
                            };
                            _context.DegreePrograms.Add(degreeProgramEntity);
                        }

                        // Insert alumni degree (one per alumni/degree program combo)
                        int alumniIdToUse = alumniEntity.AlumniId;
                        if (alumniIdToUse == 0)
                        {
                            await _context.SaveChangesAsync();
                            alumniIdToUse = alumniEntity.AlumniId;
                        }

                        // Parse GPA
                        decimal? parsedGpa = null;
                        if (!string.IsNullOrEmpty(gpaText) && decimal.TryParse(gpaText, out var gpaValue))
                        {
                            parsedGpa = gpaValue;
                        }

                        if (!await _context.AlumniDegrees.AnyAsync(ad => ad.AlumniId == alumniIdToUse && ad.DegreeId == degreeProgramEntity.DegreeId))
                        {
                            var alumniDegree = new AlumniDegree
                            {
                                AlumniId = alumniIdToUse,
                                DegreeId = degreeProgramEntity.DegreeId,
                                DateConferred = graduationYear > 0 ? DateOnly.FromDateTime(new DateTime(graduationYear, 1, 1)) : DateOnly.FromDateTime(DateTime.UtcNow),
                                YearsToCompleteDegree = null,
                                Gpa = parsedGpa,
                                EmploymentWhileStudying = null,
                                DegreeSpecificJob = null,
                                ParticipatedInResearch = null,
                                JobSecuredUponGraduation = null,
                                AttendedOrPlansGradSchool = null
                            };

                            _context.AlumniDegrees.Add(alumniDegree);
                        }
                        else
                        {
                            importErrors.Add($"Row {row}: AlumniDegree for JagId '{jagId}' and same degree program already exists; skipping.");
                        }

                        successCount++;
                    }

                    if (importErrors.Any())
                    {
                        await _context.SaveChangesAsync();
                        TempData["ErrorMessage"] = $"Bulk import completed with {importErrors.Count} problem(s). Please review the error log and fix these entries before re-running.";
                        TempData["ImportErrors"] = string.Join("\n", importErrors);
                        return View();
                    }

                    await _context.SaveChangesAsync();
                }
            }

            TempData["SuccessMessage"] = $"Bulk import successful for {successCount} rows.";
            return RedirectToAction(nameof(Index));
        }

        private bool AlumniExists(int id)
        {
            return _context.Alumni.Any(e => e.AlumniId == id);
        }
    }
}
