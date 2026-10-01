namespace Alumni_Management_System.Models;

// The one JAG ID format used everywhere (models, view models, bulk import
// and the hints shown beside every JAG ID field) so they can't drift apart.
public static class JagIdFormat
{
    public const int Length = 9;
    public const string Pattern = @"^J\d{8}$";
    public const string Example = "J00123456";
    public const string Hint = "Letter 'J' followed by exactly 8 digits - 9 characters in total (e.g., " + Example + ")";
    public const string ErrorMessage = "JAG ID must be the letter 'J' followed by exactly 8 digits (e.g., " + Example + ").";
}
