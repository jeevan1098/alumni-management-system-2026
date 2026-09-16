using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Alumni_Management_System.Data;

namespace Alumni_Management_System.Services;

// Keeps Alumni.CollegeId in step with the college of the alumnus's most
// recently conferred degree. An alumnus's degrees (and their colleges) can
// be added/edited/removed independently of the Alumni record, so this needs
// to be re-run any time an AlumniDegree changes - otherwise CollegeId is
// left stuck at whatever it was set to when the record was first created
// (e.g. by bulk import) even as newer degrees come in.
public static class AlumniCollegeSyncService
{
    public static async Task SyncToMostRecentDegreeAsync(ApplicationDbContext context, int alumniId)
    {
        var mostRecentDegree = await context.AlumniDegrees
            .Where(ad => ad.AlumniId == alumniId)
            .Include(ad => ad.Degree).ThenInclude(d => d.Department)
            .OrderByDescending(ad => ad.DateConferred)
            .FirstOrDefaultAsync();

        // No degrees left (e.g. the last one was just deleted) - leave
        // whatever College value is already there rather than clearing it.
        if (mostRecentDegree == null)
        {
            return;
        }

        var alumni = await context.Alumni.FindAsync(alumniId);
        var newCollegeId = mostRecentDegree.Degree.Department.CollegeId;
        if (alumni != null && alumni.CollegeId != newCollegeId)
        {
            alumni.CollegeId = newCollegeId;
            await context.SaveChangesAsync();
        }
    }
}
