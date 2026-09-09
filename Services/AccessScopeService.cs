using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Alumni_Management_System.Data;
using Alumni_Management_System.Models;

namespace Alumni_Management_System.Services;

// Resolves what a Staff/Admin user is allowed to see based on UserAccessScope
// rows an admin has granted them (see UserAccessScopesController). A user
// with no scope rows for their role is unrestricted - scoping only kicks in
// once an admin explicitly grants one, so existing accounts are unaffected.
public static class AccessScopeService
{
    // Returns null for unrestricted (system-wide) access, or the set of
    // College IDs the user is limited to.
    public static async Task<List<int>> GetAllowedCollegeIdsAsync(ApplicationDbContext context, AppUser user, IList<string> roles)
    {
        var scopes = await context.UserAccessScopes
            .Include(s => s.Role)
            .Where(s => s.UserId == user.Id && s.IsActive)
            .ToListAsync();

        var relevantScopes = scopes.Where(s => roles.Contains(s.Role.Name)).ToList();

        if (relevantScopes.Count == 0)
        {
            return null;
        }

        if (relevantScopes.Any(s => s.CollegeId == null))
        {
            return null;
        }

        return relevantScopes.Select(s => s.CollegeId!.Value).Distinct().ToList();
    }
}
