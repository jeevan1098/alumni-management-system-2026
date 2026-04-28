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

        public async Task<IActionResult> Index(string searchString, string sortOrder)
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
                var tokens = searchString
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim())
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .ToList();

                var allAlumni = await alumniQuery.ToListAsync();

                var filtered = allAlumni.Where(a =>
                    tokens.All(token => MatchesToken(a, token))
                ).ToList();

                ViewData["CurrentFilter"] = searchString;
                ViewData["UserRole"] = roles.FirstOrDefault();
                ViewData["SortOrder"] = sortOrder;

                var currentAlumni = await _context.Alumni
                    .FirstOrDefaultAsync(a => a.UserId == currentUser.Id || a.JagId == currentUser.JagId);
                ViewData["CurrentAlumniId"] = currentAlumni?.AlumniId;

                return View(ApplySort(filtered.AsQueryable(), sortOrder).ToList());
            }

            ViewData["CurrentFilter"] = searchString;
            ViewData["UserRole"] = roles.FirstOrDefault();
            ViewData["SortOrder"] = sortOrder;

            var currentAlumniDefault = await _context.Alumni
                .FirstOrDefaultAsync(a => a.UserId == currentUser.Id || a.JagId == currentUser.JagId);
            ViewData["CurrentAlumniId"] = currentAlumniDefault?.AlumniId;

            return View(ApplySort(alumniQuery, sortOrder).ToList());
        }

        private IQueryable<Alumni> ApplySort(IQueryable<Alumni> query, string sortOrder)
        {
            return sortOrder switch
            {
                "jagid" => query.OrderBy(a => a.JagId),
                "jagid_desc" => query.OrderByDescending(a => a.JagId),
                "name" => query.OrderBy(a => a.LastName).ThenBy(a => a.FirstName),
                "name_desc" => query.OrderByDescending(a => a.LastName).ThenByDescending(a => a.FirstName),
                "email" => query.OrderBy(a => a.PermanentEmail),
                "email_desc" => query.OrderByDescending(a => a.PermanentEmail),
                "year" => query.OrderBy(a => a.GraduationYear),
                "year_desc" => query.OrderByDescending(a => a.GraduationYear),
                "city" => query.OrderBy(a => a.City),
                "city_desc" => query.OrderByDescending(a => a.City),
                "state" => query.OrderBy(a => a.State),
                "state_desc" => query.OrderByDescending(a => a.State),
                _ => query.OrderByDescending(a => a.GraduationYear),
            };
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

        //public async Task<IActionResult> Details(int? id)
        //{
        //    if (id == null) return NotFound();
        //    var alumni = await _context.Alumni
        //        .Include(a => a.User)
        //        .FirstOrDefaultAsync(m => m.AlumniId == id);
        //    if (alumni == null) return NotFound();
        //    return View(alumni);
        //}
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var alumni = await _context.Alumni
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.AlumniId == id);

            if (alumni == null) return NotFound();

            bool isAdminOrStaff = User.IsInRole("Admin") || User.IsInRole("Staff");
            bool isAlumni = User.IsInRole("Alumni");

            // Find the logged-in alumni's own record
            bool isOwnProfile = false;
            if (isAlumni)
            {
                var currentAlumni = await _context.Alumni
                    .FirstOrDefaultAsync(a => a.UserId == _userManager.GetUserId(User));
                isOwnProfile = currentAlumni?.AlumniId == id;
            }

            // Alumni can view:
            //   - their own profile (always)
            //   - other alumni profiles only if Privacy == false
            // Admin/Staff can view everyone
            if (isAlumni && !isOwnProfile && alumni.Privacy == true)
                return Forbid();

            // canViewDetails: show related records when...
            //   - Admin/Staff (always)
            //   - Own profile (always)
            //   - Another alumni viewing a public profile (Privacy == false)
            bool canViewDetails = isAdminOrStaff || isOwnProfile || (!isOwnProfile && alumni.Privacy == false);

            if (canViewDetails)
            {
                ViewBag.Employments = await _context.AlumniEmployments
                    .Include(e => e.Employer)
                    .Where(e => e.AlumniId == id)
                    .ToListAsync();

                ViewBag.Internships = await _context.AlumniInternships
                    .Include(i => i.Employer)
                    .Where(i => i.AlumniId == id)
                    .ToListAsync();

                ViewBag.Organizations = await _context.AlumniOrganizations
                    .Include(o => o.OrganizationType)
                    .Where(o => o.AlumniId == id)
                    .ToListAsync();

                // Degrees only for Admin/Staff
                if (isAdminOrStaff)
                {
                    ViewBag.Degrees = await _context.AlumniDegrees
                        .Include(d => d.Degree)
                        .Where(d => d.AlumniId == id)
                        .ToListAsync();
                }
            }

            ViewBag.CanViewDetails = canViewDetails;
            ViewBag.IsAdminOrStaff = isAdminOrStaff;

            return View(alumni);
        }


        [Authorize(Roles = "Admin")]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("AlumniId,JagId,Prefix,FirstName,PreferredFirstName,LastName,Gender,AgeAtGraduation,StudentEmail,PermanentEmail,Phone,Address,City,State,Postcode,Country,GraduationYear,SolicitationCode,SocialMediaAccount,Privacy,IsActive,LastUpdated")] Alumni alumni)
        {
            // Check for duplicate JAG ID in Alumni table
            bool jagIdExistsInAlumni = await _context.Alumni
                .AnyAsync(a => a.JagId == alumni.JagId);

            if (jagIdExistsInAlumni)
            {
                ModelState.AddModelError("JagId", $"JAG ID '{alumni.JagId}' already exists in the Alumni records.");
            }

            await ValidateEmails(alumni);


            if (ModelState.IsValid)
            {
                alumni.LastUpdated = DateTime.Now;
                alumni.IsActive = false;
                var registryEntry = new AlumniRegistry
                {
                    JagId = alumni.JagId,
                    FirstName = alumni.FirstName,
                    LastName = alumni.LastName
                };
                _context.Add(alumni);
                _context.AlumniRegistries.Add(registryEntry);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Alumni created and added to Registry successfully!";
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

            // Check for duplicate JAG ID, excluding the current record
            bool jagIdTaken = await _context.Alumni
                .AnyAsync(a => a.JagId == alumni.JagId && a.AlumniId != alumni.AlumniId);

            if (jagIdTaken)
            {
                ModelState.AddModelError("JagId", $"JAG ID '{alumni.JagId}' is already assigned to another alumni.");
            }

            await ValidateEmails(alumni);

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

        private async Task ValidateEmails(Alumni alumni)
        {
            if (await _context.Alumni.AnyAsync(a => a.StudentEmail == alumni.StudentEmail && a.AlumniId != alumni.AlumniId))
            {
                ModelState.AddModelError("StudentEmail", "Student Email already exists.");
            }

            if (await _context.Alumni.AnyAsync(a => a.PermanentEmail == alumni.PermanentEmail && a.AlumniId != alumni.AlumniId))
            {
                ModelState.AddModelError("PermanentEmail", "Permanent Email already exists.");
            }
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

            // Validate file extension
            var allowedExtensions = new[] { ".xlsx", ".xls" };
            var fileExtension = Path.GetExtension(excelFile.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                TempData["ErrorMessages"] = "Invalid file type. Please upload an Excel file (.xlsx or .xls).";
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

                if (rowCount < 2)
                {
                    TempData["ErrorMessages"] = "Excel file contains no data rows.";
                    return View();
                }

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
                            importErrors.Add($"Row {row}: ID '{jagId}' is invalid — must start with 'J' followed by numbers only (e.g. J0012345) — skipped.");
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(firstName))
                        {
                            importErrors.Add($"Row {row}: First Name is required — skipped.");
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(lastName))
                        {
                            importErrors.Add($"Row {row}: Last Name is required — skipped.");
                            continue;
                        }

                        // Parse graduation year for this row — used in both paths below
                        int graduationYear = 0;
                        if (!string.IsNullOrEmpty(gradText) && gradText.Length >= 4)
                            int.TryParse(gradText.Substring(0, 4), out graduationYear);

                        if (graduationYear > 0 && (graduationYear < 1900 || graduationYear > 2100))
                        {
                            importErrors.Add($"Row {row}: Graduation year '{graduationYear}' is invalid — must be between 1900 and 2100 — skipped.");
                            continue;
                        }

                        var conferredDate = graduationYear > 0
                            ? DateOnly.FromDateTime(new DateTime(graduationYear, 1, 1))
                            : DateOnly.FromDateTime(DateTime.UtcNow);

                        // Parse age
                        int age = 0;
                        if (!string.IsNullOrEmpty(ageText))
                        {
                            int.TryParse(ageText, out age);
                            if (age > 0 && (age < 15 || age > 150))
                            {
                                importErrors.Add($"Row {row}: Age '{age}' is invalid — must be between 15 and 150 — skipped.");
                                continue;
                            }
                        }

                        // Validate GPA if present
                        if (gpa.HasValue && (gpa < 0.0m || gpa > 4.0m))
                        {
                            importErrors.Add($"Row {row}: GPA '{gpa}' is invalid — must be between 0.0 and 4.0 — skipped.");
                            continue;
                        }

                        // Validate emails if present
                        if (!string.IsNullOrWhiteSpace(studentEmail) && !IsValidEmail(studentEmail))
                        {
                            importErrors.Add($"Row {row}: Student email '{studentEmail}' is not a valid email address — skipped.");
                            continue;
                        }

                        if (!string.IsNullOrWhiteSpace(permEmail) && !IsValidEmail(permEmail))
                        {
                            importErrors.Add($"Row {row}: Permanent email '{permEmail}' is not a valid email address — skipped.");
                            continue;
                        }

                        var finalEmail = !string.IsNullOrEmpty(permEmail)
                            ? permEmail
                            : (!string.IsNullOrEmpty(studentEmail)
                                ? studentEmail
                                : $"{jagId.ToLower()}@placeholder.com");

                        var fullAddress = string.Join(", ", new[] { street1, street2 }
                            .Where(s => !string.IsNullOrWhiteSpace(s)));

                        // Check whether alumni and registry records already exist
                        var alumniExists = await _context.Alumni.AnyAsync(a => a.JagId == jagId);
                        var registryEntry = await _context.AlumniRegistries.FirstOrDefaultAsync(r => r.JagId == jagId);

                        // ── PATH A: ALUMNI ALREADY EXISTS ────────────────────────────────────
                        if (alumniExists)
                        {
                            var existingAlumni = await _context.Alumni.FirstOrDefaultAsync(a => a.JagId == jagId);

                            if (existingAlumni != null && !string.IsNullOrEmpty(major))
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

                                bool degreeAlreadyLinked = await _context.AlumniDegrees.AnyAsync(ad =>
                                    ad.AlumniId == existingAlumni.AlumniId &&
                                    ad.DegreeId == degreeProgram.DegreeId);

                                if (!degreeAlreadyLinked)
                                {
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

                                    importErrors.Add($"Row {row}: '{jagId}' already exists — new degree '{degreeCode} in {major}' (conferred {conferredDate.Year}) added successfully.");
                                }
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
                            Prefix = NormalizePrefix(prefix),
                            FirstName = firstName,
                            LastName = lastName,
                            Gender = NormalizeGender(gender),
                            AgeAtGraduation = age > 0 ? age : null,
                            StudentEmail = string.IsNullOrWhiteSpace(studentEmail) ? null : studentEmail,
                            PermanentEmail = finalEmail,
                            Address = string.IsNullOrWhiteSpace(fullAddress) ? null : fullAddress,
                            City = string.IsNullOrWhiteSpace(city) ? null : city,
                            State = string.IsNullOrWhiteSpace(state) ? null : state,
                            Postcode = string.IsNullOrWhiteSpace(zip) ? null : zip,
                            Country = "USA",
                            GraduationYear = graduationYear,
                            SolicitationCode = true,
                            Privacy = false,
                            IsActive = true,
                            LastUpdated = DateTime.Now
                        };

                        _context.Alumni.Add(alumniEntity);

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

                        await _context.SaveChangesAsync();

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
                            updatedCount++;
                        else
                            successCount++;
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

            // Separate degree-added notices from real errors
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ResetAlumniPassword(int alumniId)
        {
            var alumni = await _context.Alumni
                .FirstOrDefaultAsync(a => a.AlumniId == alumniId);
            if (alumni == null)
            {
                TempData["ErrorMessage"] = "Alumni not found.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByNameAsync(alumni.JagId)
                       ?? await _userManager.Users.FirstOrDefaultAsync(u => u.JagId == alumni.JagId);

            if (user == null)
            {
                TempData["ErrorMessage"] = $"{alumni.FirstName} {alumni.LastName} does not have an account yet.";
                return RedirectToAction(nameof(Index));
            }

            // Generate random temp password
            var random = new Random();
            var digits = random.Next(1000, 9999).ToString();
            var letters = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            var letter = letters[random.Next(letters.Length)];
            var tempPassword = $"Jag@{digits}{letter}";

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, tempPassword);

            if (result.Succeeded)
            {
                user.IsFirstLogin = true;
                await _userManager.UpdateAsync(user);

                TempData["ResetPasswordSuccess"] = $"Password reset for {alumni.FirstName} {alumni.LastName} ({alumni.JagId})";
                TempData["TempPassword"] = tempPassword;
            }
            else
            {
                TempData["ErrorMessage"] = "Reset failed: " + string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Simple email format check used during bulk import validation.
        /// </summary>
        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private string NormalizePrefix(string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix))
                return null;

            return prefix.Trim().ToUpper().Replace(".", "") switch
            {
                "MR" => "Mr.",
                "MRS" => "Mrs.",
                "MS" => "Ms.",
                "DR" => "Dr.",
                "PROF" => "Prof.",
                _ => null
            };
        }

        private string NormalizeGender(string gender)
        {
            if (string.IsNullOrWhiteSpace(gender)) return null;
            return gender.Trim().ToUpper() switch
            {
                "M" => "Male",
                "F" => "Female",
                "O" => "Other",
                "P" => "Prefer not to say",
                "MALE" => "Male",
                "FEMALE" => "Female",
                _ => null
            };
        }
    }
}