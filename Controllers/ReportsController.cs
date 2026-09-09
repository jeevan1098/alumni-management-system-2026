using Alumni_Management_System.Data;
using Alumni_Management_System.Models;
using Alumni_Management_System.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Alumni_Management_System.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public ReportsController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Home()
        {
            return View();
        }

        // Alumni this user is allowed to see - a Staff user scoped to a
        // college (see AccessScopeService) only ever sees that college's
        // alumni in any of these reports, same as the Dashboard.
        private async Task<IQueryable<Alumni>> GetScopedAlumniAsync()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);
            var allowedCollegeIds = await Services.AccessScopeService.GetAllowedCollegeIdsAsync(_context, currentUser, roles);

            IQueryable<Alumni> scopedAlumni = _context.Alumni;
            if (allowedCollegeIds != null)
            {
                scopedAlumni = scopedAlumni.Where(a => a.CollegeId != null && allowedCollegeIds.Contains(a.CollegeId.Value));
            }

            return scopedAlumni;
        }

        // GET: Reports/MailingLabels
        public async Task<IActionResult> MailingLabels(string major, string degreeType, int? year, bool? download)
        {
            var scopedAlumni = await GetScopedAlumniAsync();

            ViewBag.Majors = await _context.DegreePrograms
                .Select(d => d.MajorFieldOfStudy)
                .Distinct()
                .OrderBy(m => m)
                .ToListAsync();

            IQueryable<Alumni> query = scopedAlumni.Include(a => a.AlumniDegrees).ThenInclude(d => d.Degree);

            if (year.HasValue)
            {
                query = query.Where(a => a.GraduationYear == year.Value);
            }

            var hasDegreeTypeFilter = !string.IsNullOrWhiteSpace(degreeType) && degreeType != "ALL";
            if (!string.IsNullOrWhiteSpace(major) || hasDegreeTypeFilter)
            {
                query = query.Where(a => a.AlumniDegrees.Any(d =>
                    (string.IsNullOrWhiteSpace(major) || d.Degree.MajorFieldOfStudy == major) &&
                    (!hasDegreeTypeFilter || d.Degree.DegreeType == degreeType)));
            }

            var alumniList = await query.OrderBy(a => a.LastName).ThenBy(a => a.FirstName).ToListAsync();

            var labels = alumniList.Select(a => new MailingLabelViewModel
            {
                Prefix = a.Prefix,
                FirstName = a.FirstName,
                LastName = a.LastName,
                Address = a.Address,
                City = a.City,
                State = a.State,
                Postcode = a.Postcode
            }).ToList();

            if (download == true)
            {
                return ExportToExcel(labels,
                    new[] { "Prefix", "First Name", "Last Name", "Address", "City", "State", "Postcode" },
                    l => new object[] { l.Prefix, l.FirstName, l.LastName, l.Address, l.City, l.State, l.Postcode },
                    "Mailing Labels", "MailingLabels.xlsx");
            }

            return View(labels);
        }

        // GET: Reports/AlumniCount
        public async Task<IActionResult> AlumniCount(int? fromYear, int? toYear, string groupBy, bool? download)
        {
            var scopedAlumni = await GetScopedAlumniAsync();

            var query = scopedAlumni.Include(a => a.AlumniDegrees).ThenInclude(d => d.Degree).ThenInclude(deg => deg.Department);

            IQueryable<Alumni> yearFiltered = query;
            if (fromYear.HasValue)
            {
                yearFiltered = yearFiltered.Where(a => a.GraduationYear >= fromYear.Value);
            }
            if (toYear.HasValue)
            {
                yearFiltered = yearFiltered.Where(a => a.GraduationYear <= toYear.Value);
            }

            var alumniList = await yearFiltered.ToListAsync();

            var rows = alumniList
                .SelectMany(a => a.AlumniDegrees.Select(d => new { a.GraduationYear, d.Degree }))
                .Where(x => x.Degree != null)
                .GroupBy(x => new { x.Degree.Department.DepartmentName, x.Degree.DegreeType, x.Degree.MajorFieldOfStudy, x.GraduationYear })
                .Select(g => new AlumniCountReportRowViewModel
                {
                    Department = g.Key.DepartmentName,
                    DegreeType = g.Key.DegreeType,
                    Major = g.Key.MajorFieldOfStudy,
                    Year = g.Key.GraduationYear,
                    Count = g.Count()
                })
                .ToList();

            ViewBag.GroupBy = string.IsNullOrWhiteSpace(groupBy) ? "department" : groupBy;
            ViewData["FromYear"] = fromYear;
            ViewData["ToYear"] = toYear;

            if (download == true)
            {
                return ExportToExcel(rows,
                    new[] { "Department", "Degree Type", "Major", "Year", "Count" },
                    r => new object[] { r.Department, r.DegreeType, r.Major, r.Year, r.Count },
                    "Alumni Count", "AlumniCount.xlsx");
            }

            return View(rows);
        }

        // GET: Reports/AverageGPA
        public async Task<IActionResult> AverageGPA(int? fromYear, int? toYear, bool? download)
        {
            var scopedAlumni = await GetScopedAlumniAsync();

            var query = scopedAlumni.Include(a => a.AlumniDegrees).ThenInclude(d => d.Degree);

            IQueryable<Alumni> yearFiltered = query;
            if (fromYear.HasValue)
            {
                yearFiltered = yearFiltered.Where(a => a.GraduationYear >= fromYear.Value);
            }
            if (toYear.HasValue)
            {
                yearFiltered = yearFiltered.Where(a => a.GraduationYear <= toYear.Value);
            }

            var alumniList = await yearFiltered.ToListAsync();

            var rows = alumniList
                .SelectMany(a => a.AlumniDegrees.Select(d => new { a.GraduationYear, AlumniDegree = d }))
                .Where(x => x.AlumniDegree.Degree != null && x.AlumniDegree.Gpa.HasValue)
                .GroupBy(x => new { x.GraduationYear, x.AlumniDegree.Degree.DegreeType, x.AlumniDegree.Degree.MajorFieldOfStudy })
                .Select(g => new AverageGpaReportRowViewModel
                {
                    Year = g.Key.GraduationYear,
                    DegreeType = g.Key.DegreeType,
                    Major = g.Key.MajorFieldOfStudy,
                    DegreeCount = g.Count(),
                    AverageGpa = Math.Round(g.Average(x => x.AlumniDegree.Gpa.Value), 2)
                })
                .ToList();

            ViewData["FromYear"] = fromYear;
            ViewData["ToYear"] = toYear;

            if (download == true)
            {
                return ExportToExcel(rows,
                    new[] { "Year", "Degree Type", "Major", "Degree Count", "Average GPA" },
                    r => new object[] { r.Year, r.DegreeType, r.Major, r.DegreeCount, r.AverageGpa },
                    "Average GPA", "AverageGPA.xlsx");
            }

            return View(rows);
        }

        private FileContentResult ExportToExcel<T>(IEnumerable<T> rows, string[] headers, Func<T, object[]> rowSelector, string sheetName, string fileName)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add(sheetName);

            for (int col = 0; col < headers.Length; col++)
            {
                worksheet.Cells[1, col + 1].Value = headers[col];
                worksheet.Cells[1, col + 1].Style.Font.Bold = true;
            }

            int row = 2;
            foreach (var item in rows)
            {
                var values = rowSelector(item);
                for (int col = 0; col < values.Length; col++)
                {
                    worksheet.Cells[row, col + 1].Value = values[col];
                }
                row++;
            }

            worksheet.Cells[worksheet.Dimension?.Address ?? "A1"].AutoFitColumns();

            var bytes = package.GetAsByteArray();
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}
