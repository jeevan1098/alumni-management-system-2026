using Alumni_Management_System.Data;
using Alumni_Management_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Alumni_Management_System.Controllers
{
    [Authorize(Roles = "Admin")] // Only Admin can view audit logs
    public class AuditLogsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuditLogsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: AuditLogs
        public async Task<IActionResult> Index(string searchUser, string searchAction, string searchEntity, DateTime? startDate, DateTime? endDate, int page = 1)
        {
            int pageSize = 50;

            var query = _context.AuditLogs.Include(a => a.User).AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchUser))
            {
                query = query.Where(a => a.UserName.Contains(searchUser) || a.UserId.Contains(searchUser));
                ViewData["SearchUser"] = searchUser;
            }

            if (!string.IsNullOrEmpty(searchAction))
            {
                query = query.Where(a => a.Action == searchAction);
                ViewData["SearchAction"] = searchAction;
            }

            if (!string.IsNullOrEmpty(searchEntity))
            {
                query = query.Where(a => a.EntityName.Contains(searchEntity));
                ViewData["SearchEntity"] = searchEntity;
            }

            if (startDate.HasValue)
            {
                query = query.Where(a => a.Timestamp >= startDate.Value);
                ViewData["StartDate"] = startDate.Value.ToString("yyyy-MM-dd");
            }

            if (endDate.HasValue)
            {
                var endDateTime = endDate.Value.AddDays(1).AddSeconds(-1);
                query = query.Where(a => a.Timestamp <= endDateTime);
                ViewData["EndDate"] = endDate.Value.ToString("yyyy-MM-dd");
            }

            // Order by most recent first
            query = query.OrderByDescending(a => a.Timestamp);

            // Pagination
            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            var auditLogs = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;
            ViewData["TotalRecords"] = totalRecords;

            return View(auditLogs);
        }

        // GET: AuditLogs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var auditLog = await _context.AuditLogs
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.AuditId == id);

            if (auditLog == null)
            {
                return NotFound();
            }

            return View(auditLog);
        }
    }
}

