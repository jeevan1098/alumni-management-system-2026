using System.Collections.Generic;
using System.Linq;

namespace Alumni_Management_System.Services;

// Builds "3 departments, 21 alumni and 1 access scope" for the messages shown
// when a delete is refused because other records still point to the item.
public static class InUseMessage
{
    // Returns null when every count is zero (nothing is in the way).
    public static string Describe(params (int Count, string Singular, string Plural)[] items)
    {
        var parts = items
            .Where(i => i.Count > 0)
            .Select(i => $"{i.Count} {(i.Count == 1 ? i.Singular : i.Plural)}")
            .ToList();

        return parts.Count switch
        {
            0 => null,
            1 => parts[0],
            _ => string.Join(", ", parts.Take(parts.Count - 1)) + " and " + parts.Last()
        };
    }
}
