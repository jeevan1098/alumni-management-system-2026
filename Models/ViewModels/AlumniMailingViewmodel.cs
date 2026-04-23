using System.ComponentModel.DataAnnotations;

namespace Alumni_Management_System.Models.ViewModels
{
    public class AlumniMailingViewmodel
    {
        [Required(ErrorMessage = "Alumni ID is required.")]
        public int AlumniId { get; set; }   // IMPORTANT for selecting users

        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
        public string LastName { get; set; }

        [EmailAddress(ErrorMessage = "Permanent email must be a valid email address.")]
        [StringLength(150, ErrorMessage = "Permanent email cannot exceed 150 characters.")]
        public string PermanentEmail { get; set; }

        [Range(1900, 2100, ErrorMessage = "Graduation year must be between 1900 and 2100.")]
        public int GraduationYear { get; set; }

        [StringLength(100, ErrorMessage = "Degree cannot exceed 100 characters.")]
        public string Degree { get; set; }

        [StringLength(20, ErrorMessage = "Degree type cannot exceed 20 characters.")]
        public string DegreeType { get; set; }

        public bool IsSelected { get; set; } // for checkbox selection
        public bool IsAlreadyMapped { get; set; }
    }
}
