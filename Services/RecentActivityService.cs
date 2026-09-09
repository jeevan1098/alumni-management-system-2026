using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Alumni_Management_System.Data;

namespace Alumni_Management_System.Services;

public class RecentActivityItem
{
    public string EntityType { get; set; }
    public string ActionType { get; set; }
    public string ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; }
    public string Summary { get; set; }

    public string Icon => EntityType switch
    {
        "Alumni" => "bi-person-fill",
        "Alumni Registry" => "bi-journal-text",
        "Message" => "bi-envelope-fill",
        "College" => "bi-bank",
        _ => "bi-clock-history"
    };

    public string BadgeClass => ActionType switch
    {
        "Added" => "bg-success",
        "Deleted" => "bg-danger",
        _ => "bg-primary"
    };
}

// Builds the Dashboard's "Recent Activity" feed from the *_Audit tables that
// ApplicationDbContext.SaveChanges already writes automatically - real
// timestamps and who made the change, instead of guessing from row order.
public static class RecentActivityService
{
    public static async Task<List<RecentActivityItem>> GetRecentAsync(ApplicationDbContext context, int take)
    {
        // A single DbContext can't run concurrent queries, so these run
        // sequentially - each pulls a few extra before the final merge/sort/
        // trim below, so a burst of activity in one area doesn't crowd out another.
        var alumni = await context.AlumniAudits
            .OrderByDescending(a => a.ChangedAt)
            .Take(take)
            .Select(a => new RecentActivityItem
            {
                EntityType = "Alumni",
                ActionType = a.ActionType,
                ChangedBy = a.ChangedBy,
                ChangedAt = a.ChangedAt,
                Summary = $"Alumni record {a.ActionType.ToLower()}"
            })
            .ToListAsync();

        var registry = await context.AlumniRegistryAudits
            .OrderByDescending(a => a.ChangedAt)
            .Take(take)
            .Select(a => new RecentActivityItem
            {
                EntityType = "Alumni Registry",
                ActionType = a.ActionType,
                ChangedBy = a.ChangedBy,
                ChangedAt = a.ChangedAt,
                Summary = $"Registry entry {a.ActionType.ToLower()}"
            })
            .ToListAsync();

        var messages = await context.MessageAudits
            .OrderByDescending(a => a.ChangedAt)
            .Take(take)
            .Select(a => new RecentActivityItem
            {
                EntityType = "Message",
                ActionType = a.ActionType,
                ChangedBy = a.ChangedBy,
                ChangedAt = a.ChangedAt,
                Summary = $"Message {a.ActionType.ToLower()}"
            })
            .ToListAsync();

        var colleges = await context.CollegeAudits
            .OrderByDescending(a => a.ChangedAt)
            .Take(take)
            .Select(a => new RecentActivityItem
            {
                EntityType = "College",
                ActionType = a.ActionType,
                ChangedBy = a.ChangedBy,
                ChangedAt = a.ChangedAt,
                Summary = $"College {a.ActionType.ToLower()}"
            })
            .ToListAsync();

        return alumni
            .Concat(registry)
            .Concat(messages)
            .Concat(colleges)
            .OrderByDescending(i => i.ChangedAt)
            .Take(take)
            .ToList();
    }
}
