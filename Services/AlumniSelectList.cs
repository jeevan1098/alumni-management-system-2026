using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Alumni_Management_System.Models;

namespace Alumni_Management_System.Services;

// Alumni dropdowns show "First Last (JAG ID)" so two people with the same
// name can't be mixed up. Use this for every Alumni <select> instead of a
// bare SelectList on FirstName.
public static class AlumniSelectList
{
    public static SelectList Build(IEnumerable<Alumni> alumni, object selectedValue = null)
    {
        var items = alumni
            .Select(a => new { a.AlumniId, DisplayText = $"{a.FirstName} {a.LastName} ({a.JagId})" })
            .OrderBy(a => a.DisplayText)
            .ToList();
        return new SelectList(items, "AlumniId", "DisplayText", selectedValue);
    }
}
