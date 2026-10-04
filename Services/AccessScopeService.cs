using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Alumni_Management_System.Data;
using Alumni_Management_System.Models;

namespace Alumni_Management_System.Services;

// What a limited Staff/Admin user may see: whole colleges and/or single
// departments. A null AccessScope everywhere means unrestricted.
public class AccessScope
{
    public List<int> CollegeIds { get; } = new();
    public List<int> DepartmentIds { get; } = new();
}

// Resolves what a Staff/Admin user is allowed to see based on UserAccessScope
// rows an admin has granted them (see UsersController). A user with no scope
// rows for their role is unrestricted - scoping only kicks in once an admin
// explicitly grants one, so existing accounts are unaffected.
//
// A row with a College but no Department grants the whole college; a row with
// a Department grants just that department. A row with neither is system-wide.
public static class AccessScopeService
{
    // Returns null for unrestricted (system-wide) access.
    public static async Task<AccessScope> GetScopeAsync(ApplicationDbContext context, AppUser user, IList<string> roles)
    {
        var scopes = await context.UserAccessScopes
            .Include(s => s.Role)
            .Where(s => s.UserId == user.Id && s.IsActive)
            .ToListAsync();

        return Resolve(scopes, roles);
    }

    // Same rule as above for scope rows that are already loaded (active rows
    // of one user, with Role included).
    public static AccessScope Resolve(IEnumerable<UserAccessScope> scopes, IList<string> roles)
    {
        var relevantScopes = scopes.Where(s => roles.Contains(s.Role.Name)).ToList();

        if (relevantScopes.Count == 0 || relevantScopes.Any(s => s.CollegeId == null))
        {
            return null;
        }

        var scope = new AccessScope();
        foreach (var s in relevantScopes)
        {
            if (s.DepartmentId == null)
            {
                scope.CollegeIds.Add(s.CollegeId!.Value);
            }
            else
            {
                scope.DepartmentIds.Add(s.DepartmentId.Value);
            }
        }
        scope.CollegeIds.Sort();
        scope.DepartmentIds.Sort();
        return scope;
    }

    // Narrows an Alumni query to the people a scoped user may see. An alumnus
    // is visible when ANY of these hold:
    //   1. their profile's College is one of the user's whole colleges;
    //   2. any of their degrees is in a scoped department, or in a department
    //      of a scoped college;
    //   3. any of their degrees is from an external (non-university) college -
    //      External is visible to everyone.
    // Alumni with no degree on file are matched by rule 1 only.
    public static IQueryable<Alumni> ApplyTo(IQueryable<Alumni> alumni, AccessScope scope)
    {
        if (scope == null)
        {
            return alumni;
        }

        var collegeIds = scope.CollegeIds;
        var departmentIds = scope.DepartmentIds;

        return alumni.Where(a =>
            (a.CollegeId != null && collegeIds.Contains(a.CollegeId.Value))
            || a.AlumniDegrees.Any(d =>
                departmentIds.Contains(d.Degree.DepartmentId)
                || collegeIds.Contains(d.Degree.Department.CollegeId)
                || !d.Degree.Department.College.IsInternal));
    }

    // Whether a degree belongs to the scope itself (its department, or a
    // department of a scoped college). Degree-based reports count only these,
    // so a Cybersecurity coordinator's figures aren't mixed with the same
    // person's CS degree. Needs Degree.Department loaded.
    public static bool IsDegreeInScope(DegreeProgram degree, AccessScope scope) =>
        scope == null
        || scope.DepartmentIds.Contains(degree.DepartmentId)
        || scope.CollegeIds.Contains(degree.Department.CollegeId);

    // Alumni a Staff/Admin user may work with - all of them when unrestricted,
    // otherwise those inside their scope plus their own profile (an Admin who
    // is also an alumnus can always manage their own records).
    public static async Task<IQueryable<Alumni>> GetVisibleAlumniAsync(ApplicationDbContext context, AppUser user, IList<string> roles)
    {
        var scope = await GetScopeAsync(context, user, roles);
        if (scope == null)
        {
            return context.Alumni;
        }

        var scopedIds = ApplyTo(context.Alumni, scope).Select(a => a.AlumniId);
        return context.Alumni.Where(a => a.JagId == user.JagId || scopedIds.Contains(a.AlumniId));
    }

    // True when every given alumnus is visible to the user (see
    // GetVisibleAlumniAsync). Used to stop a scoped admin opening or changing
    // another alumnus's degrees, jobs, etc. by typing a URL or editing a form.
    public static async Task<bool> AreAlumniVisibleAsync(ApplicationDbContext context, AppUser user, IList<string> roles, params int[] alumniIds)
    {
        var ids = alumniIds.Distinct().ToList();
        var visible = await GetVisibleAlumniAsync(context, user, roles);
        return await visible.CountAsync(a => ids.Contains(a.AlumniId)) == ids.Count;
    }

    // "School of Computing, Mitchell College of Business → Business" - shown
    // on banners so a scoped user knows why they see what they see.
    public static async Task<string> DescribeAsync(ApplicationDbContext context, AccessScope scope)
    {
        if (scope == null)
        {
            return null;
        }

        var collegeNames = await context.Colleges
            .Where(c => scope.CollegeIds.Contains(c.CollegeId))
            .OrderBy(c => c.CollegeName)
            .Select(c => c.CollegeName)
            .ToListAsync();
        var departmentNames = await context.Departments
            .Where(d => scope.DepartmentIds.Contains(d.DepartmentId))
            .OrderBy(d => d.College.CollegeName).ThenBy(d => d.DepartmentName)
            .Select(d => d.College.CollegeName + " → " + d.DepartmentName)
            .ToListAsync();

        return string.Join(", ", collegeNames.Concat(departmentNames));
    }

    // True when every college/department given is covered by `outer` (a
    // department is covered by itself or by its whole college).
    // `collegeOfDepartment` maps DepartmentId -> CollegeId.
    // Used to stop a limited admin granting or managing beyond their own scope.
    public static bool Covers(AccessScope outer, IEnumerable<int> collegeIds, IEnumerable<int> departmentIds, IReadOnlyDictionary<int, int> collegeOfDepartment)
    {
        if (outer == null)
        {
            return true;
        }

        return collegeIds.All(outer.CollegeIds.Contains)
            && departmentIds.All(d => outer.DepartmentIds.Contains(d)
                || (collegeOfDepartment.TryGetValue(d, out var c) && outer.CollegeIds.Contains(c)));
    }
}
