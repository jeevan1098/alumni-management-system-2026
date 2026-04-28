using Alumni_Management_System.Data;
using Alumni_Management_System.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using System.IO;

namespace Alumni_Management_System.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public ReportsController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> Home()
        {
            ViewBag.Majors = await _context.DegreePrograms
                .Where(d => d.MajorFieldOfStudy != null)
                .Select(d => d.MajorFieldOfStudy)
                .Distinct()
                .OrderBy(m => m)
                .ToListAsync();

            return View();
        }

        //Report 1
        public async Task<IActionResult> MailingLabels(string major, string degreeType, int? year, bool download)
        {
            ViewBag.Majors = await _context.DegreePrograms
                .Where(d => d.MajorFieldOfStudy != null)
                .Select(d => d.MajorFieldOfStudy)
                .Distinct()
                .OrderBy(m => m)
                .ToListAsync();

            var query = _context.Alumni
                .AsNoTracking()
                .Where(a => a.IsActive == true &&
                            a.SolicitationCode == true)
                .AsQueryable();

            // year filter
            if (year.HasValue)
            {
                query = query.Where(a => a.GraduationYear == year);
            }

            // major + degree filter
            bool filterMajor = !string.IsNullOrEmpty(major);
            bool filterDegree = !string.IsNullOrEmpty(degreeType) && degreeType != "ALL";

            if (filterMajor)
            {
                query = query.Where(a =>
                    a.AlumniDegrees.Any(ad =>
                        ad.Degree.MajorFieldOfStudy == major
                    ));
            }

            if (filterDegree)
            {
                query = query.Where(a =>
                    a.AlumniDegrees.Any(ad =>
                        degreeType == "BS" ? ad.Degree.DegreeType.StartsWith("BS") :
                        degreeType == "MS" ? ad.Degree.DegreeType.StartsWith("MS") :
                        degreeType == "PhD" ? ad.Degree.DegreeType.StartsWith("PH") :
                        true
                    ));
            }

            // data
            var data = await query
                .Select(a => new
                {
                    a.Prefix,
                    a.FirstName,
                    a.LastName,
                    a.Address,
                    a.City,
                    a.State,
                    a.Postcode
                })
                .ToListAsync();

            // excel download
            if (download)
            {
                using (var workbook = new XLWorkbook())
                {
                    var ws = workbook.Worksheets.Add("Mailing Labels");

                    ws.Cell(1, 1).Value = "Name Prefix";
                    ws.Cell(1, 2).Value = "First Name";
                    ws.Cell(1, 3).Value = "Last Name";
                    ws.Cell(1, 4).Value = "Address";
                    ws.Cell(1, 5).Value = "City";
                    ws.Cell(1, 6).Value = "State";
                    ws.Cell(1, 7).Value = "Zip";

                    int row = 2;

                    foreach (var item in data)
                    {
                        ws.Cell(row, 1).Value = item.Prefix;
                        ws.Cell(row, 2).Value = item.FirstName;
                        ws.Cell(row, 3).Value = item.LastName;
                       
                        ws.Cell(row, 4).Value = item.Address;
                        ws.Cell(row, 5).Value = item.City;
                        ws.Cell(row, 6).Value = item.State;
                        ws.Cell(row, 7).Value = item.Postcode;
                        row++;
                    }

                    ws.Columns().AdjustToContents();

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);

                        return File(
                            stream.ToArray(),
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            "MailingLabels.xlsx"
                        );
                    }
                }
            }

            return View(data);
        }
        //Report 2
        public async Task<IActionResult> AlumniCount(
    int fromYear,
    int toYear,
    string groupBy = "year",
    bool download = false)
        {
            var data = await _context.AlumniDegrees
                .Where(ad =>
                    ad.DateConferred.Year >= fromYear &&
                    ad.DateConferred.Year <= toYear
                )
                .Select(ad => new
                {
                    Year = ad.DateConferred.Year,
                    Department = ad.Degree.Department,
                    Major = ad.Degree.MajorFieldOfStudy,
                    DegreeType =
                        ad.Degree.DegreeType.StartsWith("BS") ? "Bachelors" :
                        ad.Degree.DegreeType.StartsWith("MS") ? "Masters" :
                        ad.Degree.DegreeType.StartsWith("PH") ? "PhD" :
                        "Other"
                })
                .ToListAsync();

            var result = data
                .GroupBy(x => new
                {
                    x.Year,
                    x.Department,
                    x.Major,
                    x.DegreeType
                })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Department,
                    g.Key.Major,
                    g.Key.DegreeType,
                    Count = g.Count()
                })
                .ToList();

            ViewBag.GroupBy = groupBy;

            if (download)
            {
                using (var workbook = new XLWorkbook())
                {
                    var ws = workbook.Worksheets.Add("Alumni Count Report");

                    int row = 1;
                    int grandTotal = 0;

                    // Header
                    ws.Cell(row, 1).Value = "Year";
                    ws.Cell(row, 2).Value = "Department";
                    ws.Cell(row, 3).Value = "Major";
                    ws.Cell(row, 4).Value = "Degree Type";
                    ws.Cell(row, 5).Value = "Count";

                    ws.Range(row, 1, row, 5).Style.Font.Bold = true;
                    ws.Range(row, 1, row, 5).Style.Fill.BackgroundColor = XLColor.LightGray;

                    row++;

                    if (groupBy == "year")
                    {
                        var yearGroups = result
                            .OrderBy(x => x.Year)
                            .ThenBy(x => x.DegreeType)
                            .ThenBy(x => x.Department)
                            .ThenBy(x => x.Major)
                            .GroupBy(x => x.Year);

                        foreach (var yearGroup in yearGroups)
                        {
                            int yearTotal = 0;

                            foreach (var degreeGroup in yearGroup.GroupBy(x => x.DegreeType))
                            {
                                int degreeTotal = 0;

                                foreach (var item in degreeGroup)
                                {
                                    ws.Cell(row, 1).Value = item.Year;
                                    ws.Cell(row, 2).Value = item.Department;
                                    ws.Cell(row, 3).Value = item.Major;
                                    ws.Cell(row, 4).Value = item.DegreeType;
                                    ws.Cell(row, 5).Value = item.Count;

                                    degreeTotal += item.Count;
                                    yearTotal += item.Count;
                                    grandTotal += item.Count;

                                    row++;
                                }

                                // Degree subtotal
                                ws.Cell(row, 4).Value = "Total " + degreeGroup.Key;
                                ws.Cell(row, 5).Value = degreeTotal;

                                ws.Range(row, 1, row, 5).Style.Fill.BackgroundColor = XLColor.LightBlue;
                                ws.Range(row, 1, row, 5).Style.Font.Bold = true;

                                row++;
                            }

                            // Year total
                            ws.Cell(row, 4).Value = "Total Year " + yearGroup.Key;
                            ws.Cell(row, 5).Value = yearTotal;

                            ws.Range(row, 1, row, 5).Style.Fill.BackgroundColor = XLColor.SkyBlue;
                            ws.Range(row, 1, row, 5).Style.Font.Bold = true;

                            row += 2;
                        }
                    }
                    else
                    {
                        var deptGroups = result
                            .OrderBy(x => x.Department)
                            .ThenBy(x => x.DegreeType)
                            .ThenBy(x => x.Major)
                            .ThenBy(x => x.Year)
                            .GroupBy(x => x.Department);

                        foreach (var deptGroup in deptGroups)
                        {
                            int deptTotal = 0;

                            foreach (var degreeGroup in deptGroup.GroupBy(x => x.DegreeType))
                            {
                                int degreeTotal = 0;

                                foreach (var item in degreeGroup)
                                {
                                    ws.Cell(row, 1).Value = item.Year;
                                    ws.Cell(row, 2).Value = item.Department;
                                    ws.Cell(row, 3).Value = item.Major;
                                    ws.Cell(row, 4).Value = item.DegreeType;
                                    ws.Cell(row, 5).Value = item.Count;

                                    degreeTotal += item.Count;
                                    deptTotal += item.Count;
                                    grandTotal += item.Count;

                                    row++;
                                }

                                // Degree subtotal
                                ws.Cell(row, 4).Value = "Total " + degreeGroup.Key;
                                ws.Cell(row, 5).Value = degreeTotal;

                                ws.Range(row, 1, row, 5).Style.Fill.BackgroundColor = XLColor.LightBlue;
                                ws.Range(row, 1, row, 5).Style.Font.Bold = true;

                                row++;
                            }

                            // Department total
                            ws.Cell(row, 3).Value = "Total " + deptGroup.Key;
                            ws.Cell(row, 5).Value = deptTotal;

                            ws.Range(row, 1, row, 5).Style.Fill.BackgroundColor = XLColor.SkyBlue;
                            ws.Range(row, 1, row, 5).Style.Font.Bold = true;

                            row += 2;
                        }
                    }

                    // Grand total
                    ws.Cell(row, 4).Value = "Grand Total";
                    ws.Cell(row, 5).Value = grandTotal;

                    ws.Range(row, 1, row, 5).Style.Fill.BackgroundColor = XLColor.LightGreen;
                    ws.Range(row, 1, row, 5).Style.Font.Bold = true;

                    ws.Columns().AdjustToContents();

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);

                        return File(
                            stream.ToArray(),
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            "AlumniCountReport.xlsx"
                        );
                    }
                }
            }

            return View(result);
        }

        //Report 3

        public async Task<IActionResult> AverageGPA(int fromYear, int toYear, bool download = false)
        {
            var data = await _context.AlumniDegrees
                .Where(ad =>
                    ad.DateConferred.Year >= fromYear &&
                    ad.DateConferred.Year <= toYear
                )
                .Select(ad => new
                {
                    Year = ad.DateConferred.Year,
                    Major = ad.Degree.MajorFieldOfStudy,
                    DegreeType =
                        ad.Degree.DegreeType.StartsWith("BS") ? "Bachelors" :
                        ad.Degree.DegreeType.StartsWith("MS") ? "Masters" :
                        ad.Degree.DegreeType.StartsWith("PH") ? "PhD" :
                        "Other",
                    GPA = ad.Gpa
                })
                .ToListAsync();

            var result = data
                .GroupBy(x => new
                {
                    x.Year,
                    x.DegreeType,
                    x.Major
                })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.DegreeType,
                    Major = g.Key.Major,
                    DegreeCount = g.Count(),
                    AverageGPA = Math.Round((double)g.Average(x => x.GPA), 2)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.DegreeType)
                .ThenBy(x => x.Major)
                .ToList();

            // ================= EXCEL =================
            if (download)
            {
                using (var workbook = new XLWorkbook())
                {
                    var ws = workbook.Worksheets.Add("Average GPA Report");

                    int row = 1;

                    ws.Cell(row, 1).Value = "Year";
                    ws.Cell(row, 2).Value = "Degree Type";
                    ws.Cell(row, 3).Value = "Major Field of Study";
                    ws.Cell(row, 4).Value = "Degree Count";
                    ws.Cell(row, 5).Value = "Average GPA";

                    ws.Range(row, 1, row, 5).Style.Font.Bold = true;
                    ws.Range(row, 1, row, 5).Style.Fill.BackgroundColor = XLColor.LightGray;

                    row++;

                    var yearGroups = result.GroupBy(x => x.Year);

                    foreach (var yearGroup in yearGroups)
                    {
                        int yearCount = 0;
                        decimal yearTotalGpa = 0;

                        foreach (var degreeGroup in yearGroup.GroupBy(x => x.DegreeType))
                        {
                            int degreeCount = 0;
                            decimal degreeTotalGpa = 0;

                            foreach (var item in degreeGroup)
                            {
                                ws.Cell(row, 1).Value = item.Year;
                                ws.Cell(row, 2).Value = item.DegreeType;
                                ws.Cell(row, 3).Value = item.Major;
                                ws.Cell(row, 4).Value = item.DegreeCount;
                                ws.Cell(row, 5).Value = item.AverageGPA;

                                degreeCount += item.DegreeCount;
                                degreeTotalGpa += (decimal)item.AverageGPA * item.DegreeCount;

                                yearCount += item.DegreeCount;
                                yearTotalGpa += (decimal)item.AverageGPA * item.DegreeCount;

                                row++;
                            }

                            // Degree subtotal
                            ws.Cell(row, 3).Value = "Summary " + degreeGroup.Key;
                            ws.Cell(row, 4).Value = degreeCount;
                            ws.Cell(row, 5).Value =
                                degreeCount == 0 ? 0 :
                                Math.Round(degreeTotalGpa / degreeCount, 2);

                            ws.Range(row, 1, row, 5).Style.Fill.BackgroundColor = XLColor.LightBlue;
                            ws.Range(row, 1, row, 5).Style.Font.Bold = true;

                            row++;
                        }

                        // Year subtotal
                        ws.Cell(row, 3).Value = "Year Summary " + yearGroup.Key;
                        ws.Cell(row, 4).Value = yearCount;
                        ws.Cell(row, 5).Value =
                            yearCount == 0 ? 0 :
                            Math.Round(yearTotalGpa / yearCount, 2);

                        ws.Range(row, 1, row, 5).Style.Fill.BackgroundColor = XLColor.SkyBlue;
                        ws.Range(row, 1, row, 5).Style.Font.Bold = true;

                        row += 2;
                    }

                    ws.Columns().AdjustToContents();

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);

                        return File(
                            stream.ToArray(),
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            "AverageGPAReport.xlsx"
                        );
                    }
                }
            }

            return View(result);
        }
    }
}
