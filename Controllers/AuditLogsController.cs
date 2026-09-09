using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Alumni_Management_System.Data;
using Alumni_Management_System.Models.ViewModels;

namespace Alumni_Management_System.Controllers
{
    // Read-only viewer over the per-entity *_Audit tables (who changed what,
    // and when) written automatically by ApplicationDbContext.SaveChanges.
    [Authorize(Roles = "Admin")]
    public class AuditLogsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuditLogsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private static readonly Dictionary<string, Func<ApplicationDbContext, IQueryable<AuditLogEntryViewModel>>> AuditQueries = new()
        {
            ["Alumni"] = ctx => ctx.AlumniAudits.Select(a => new AuditLogEntryViewModel { AuditId = a.AuditId, RecordId = a.AlumniId, ActionType = a.ActionType, ChangedBy = a.ChangedBy, ChangedAt = a.ChangedAt, OldValues = a.OldValues, NewValues = a.NewValues }),
            ["Alumni Degrees"] = ctx => ctx.AlumniDegreeAudits.Select(a => new AuditLogEntryViewModel { AuditId = a.AuditId, RecordId = a.AlumniDegreeId, ActionType = a.ActionType, ChangedBy = a.ChangedBy, ChangedAt = a.ChangedAt, OldValues = a.OldValues, NewValues = a.NewValues }),
            ["Alumni Employment"] = ctx => ctx.AlumniEmploymentAudits.Select(a => new AuditLogEntryViewModel { AuditId = a.AuditId, RecordId = a.AlumniEmploymentId, ActionType = a.ActionType, ChangedBy = a.ChangedBy, ChangedAt = a.ChangedAt, OldValues = a.OldValues, NewValues = a.NewValues }),
            ["Alumni Internships"] = ctx => ctx.AlumniInternshipAudits.Select(a => new AuditLogEntryViewModel { AuditId = a.AuditId, RecordId = a.AlumniInternshipId, ActionType = a.ActionType, ChangedBy = a.ChangedBy, ChangedAt = a.ChangedAt, OldValues = a.OldValues, NewValues = a.NewValues }),
            ["Alumni Messages"] = ctx => ctx.AlumniMessageAudits.Select(a => new AuditLogEntryViewModel { AuditId = a.AuditId, RecordId = a.AlumniMessageId, ActionType = a.ActionType, ChangedBy = a.ChangedBy, ChangedAt = a.ChangedAt, OldValues = a.OldValues, NewValues = a.NewValues }),
            ["Alumni Organizations"] = ctx => ctx.AlumniOrganizationAudits.Select(a => new AuditLogEntryViewModel { AuditId = a.AuditId, RecordId = a.AlumniOrganizationId, ActionType = a.ActionType, ChangedBy = a.ChangedBy, ChangedAt = a.ChangedAt, OldValues = a.OldValues, NewValues = a.NewValues }),
            ["Alumni Registry"] = ctx => ctx.AlumniRegistryAudits.Select(a => new AuditLogEntryViewModel { AuditId = a.AuditId, RecordId = a.RegistryId, ActionType = a.ActionType, ChangedBy = a.ChangedBy, ChangedAt = a.ChangedAt, OldValues = a.OldValues, NewValues = a.NewValues }),
            ["Colleges"] = ctx => ctx.CollegeAudits.Select(a => new AuditLogEntryViewModel { AuditId = a.AuditId, RecordId = a.CollegeId, ActionType = a.ActionType, ChangedBy = a.ChangedBy, ChangedAt = a.ChangedAt, OldValues = a.OldValues, NewValues = a.NewValues }),
            ["Departments"] = ctx => ctx.DepartmentAudits.Select(a => new AuditLogEntryViewModel { AuditId = a.AuditId, RecordId = a.DepartmentId, ActionType = a.ActionType, ChangedBy = a.ChangedBy, ChangedAt = a.ChangedAt, OldValues = a.OldValues, NewValues = a.NewValues }),
            ["Student Organizations"] = ctx => ctx.StudentOrganizationAudits.Select(a => new AuditLogEntryViewModel { AuditId = a.AuditId, RecordId = a.OrganizationId, ActionType = a.ActionType, ChangedBy = a.ChangedBy, ChangedAt = a.ChangedAt, OldValues = a.OldValues, NewValues = a.NewValues }),
            ["Degree Programs"] = ctx => ctx.DegreeProgramAudits.Select(a => new AuditLogEntryViewModel { AuditId = a.AuditId, RecordId = a.DegreeId, ActionType = a.ActionType, ChangedBy = a.ChangedBy, ChangedAt = a.ChangedAt, OldValues = a.OldValues, NewValues = a.NewValues }),
            ["Employers"] = ctx => ctx.EmployerAudits.Select(a => new AuditLogEntryViewModel { AuditId = a.AuditId, RecordId = a.EmployerId, ActionType = a.ActionType, ChangedBy = a.ChangedBy, ChangedAt = a.ChangedAt, OldValues = a.OldValues, NewValues = a.NewValues }),
            ["Messages"] = ctx => ctx.MessageAudits.Select(a => new AuditLogEntryViewModel { AuditId = a.AuditId, RecordId = a.MessageId, ActionType = a.ActionType, ChangedBy = a.ChangedBy, ChangedAt = a.ChangedAt, OldValues = a.OldValues, NewValues = a.NewValues }),
            ["User Access Scopes"] = ctx => ctx.UserAccessScopeAudits.Select(a => new AuditLogEntryViewModel { AuditId = a.AuditId, RecordId = a.UserAccessScopeId, ActionType = a.ActionType, ChangedBy = a.ChangedBy, ChangedAt = a.ChangedAt, OldValues = a.OldValues, NewValues = a.NewValues }),
        };

        // GET: AuditLogs
        public async Task<IActionResult> Index(string entity = "Alumni", string search = null)
        {
            if (!AuditQueries.ContainsKey(entity))
            {
                entity = "Alumni";
            }

            var query = AuditQueries[entity](_context);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(e =>
                    (e.ChangedBy != null && e.ChangedBy.Contains(search)) ||
                    (e.OldValues != null && e.OldValues.Contains(search)) ||
                    (e.NewValues != null && e.NewValues.Contains(search)));
            }

            var entries = await query
                .OrderByDescending(e => e.ChangedAt)
                .Take(300)
                .ToListAsync();

            ViewData["Entities"] = AuditQueries.Keys.ToList();
            ViewData["SelectedEntity"] = entity;
            ViewData["Search"] = search;
            return View(entries);
        }
    }
}
