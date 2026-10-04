using System;
using System.ComponentModel.DataAnnotations;

namespace Alumni_Management_System.Models;

// Graduation year must be 1950 through next year (students graduating next
// spring). A fixed [Range] can't follow the calendar, hence this attribute.
// A blank value is left to [Required]; 0 ("not known") only comes from bulk
// import and is checked here like any other value when a form saves it.
public class GraduationYearAttribute : ValidationAttribute
{
    public const int Earliest = 1950;

    public static int Latest => DateTime.Now.Year + 1;

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is int year && (year < Earliest || year > Latest))
        {
            return new ValidationResult($"Graduation year must be between {Earliest} and {Latest}.");
        }

        return ValidationResult.Success;
    }
}
