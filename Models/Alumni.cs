using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alumni_Management_System.Models;

[Index("JagId", Name = "UQ__Alumni__FBF400ED6AB71E0A", IsUnique = true)]
public partial class Alumni
{
    [Key]
    [Column("alumni_id")]
    [Display(Name = "Alumni")]
    public int AlumniId { get; set; }

    [Required(ErrorMessage = "JAG ID is required.")]
    [Column("jag_id")]
    [StringLength(20, ErrorMessage = "JAG ID cannot exceed 20 characters.")]
    [RegularExpression(@"^J\d+$", ErrorMessage = "JAG ID must start with 'J' followed by numbers only (e.g. J0012345).")]
    [Display(Name = "JAG ID")]
    public string JagId { get; set; }

    [Column("user_id")]
    [StringLength(450)]
    [Display(Name = "User ID")]
    public string? UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual AppUser User { get; set; }

    [Column("prefix")]
    [StringLength(10, ErrorMessage = "Prefix cannot exceed 10 characters.")]
    public string Prefix { get; set; }

    [Required(ErrorMessage = "First name is required.")]
    [Column("first_name")]
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
    [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "First name can only contain letters, spaces, hyphens, and apostrophes.")]
    [Display(Name = "First Name")]
    public string FirstName { get; set; }

    [Column("preferred_first_name")]
    [StringLength(50, ErrorMessage = "Preferred first name cannot exceed 50 characters.")]
    [RegularExpression(@"^[a-zA-Z\s\-']*$", ErrorMessage = "Preferred first name can only contain letters, spaces, hyphens, and apostrophes.")]
    [Display(Name = "Preferred First Name")]
    public string PreferredFirstName { get; set; }

    [Required(ErrorMessage = "Last name is required.")]
    [Column("last_name")]
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
    [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "Last name can only contain letters, spaces, hyphens, and apostrophes.")]
    [Display(Name = "Last Name")]
    public string LastName { get; set; }

    [Column("gender")]
    [StringLength(20, ErrorMessage = "Gender cannot exceed 20 characters.")]
    public string Gender { get; set; }

    [Column("age_at_graduation")]
    [Range(15, 30, ErrorMessage = "Age at graduation must be between 15 and 30.")]
    [Display(Name = "Age at Graduation")]
    public int? AgeAtGraduation { get; set; }

    [Column("student_email")]
    [StringLength(150, ErrorMessage = "Student email cannot exceed 150 characters.")]
    [EmailAddress(ErrorMessage = "Student email must be a valid email address.")]
    [Display(Name = "Student Email")]
    public string StudentEmail { get; set; }

    // Not Required — OTH Email may be empty in Excel
    [Column("permanent_email")]
    [StringLength(150, ErrorMessage = "Permanent email cannot exceed 150 characters.")]
    [EmailAddress(ErrorMessage = "Permanent email must be a valid email address.")]
    [Display(Name = "Permanent Email")]
    public string PermanentEmail { get; set; }

    [Column("phone")]
    [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters.")]
    [Phone(ErrorMessage = "Phone number is not valid.")]
    public string Phone { get; set; }

    [Column("address")]
    [StringLength(255, ErrorMessage = "Address cannot exceed 255 characters.")]
    public string Address { get; set; }

    [Column("city")]
    [StringLength(100, ErrorMessage = "City cannot exceed 100 characters.")]
    [RegularExpression(@"^[a-zA-Z\s\-'.]+$", ErrorMessage = "City can only contain letters, spaces, hyphens, apostrophes, and periods.")]
    public string City { get; set; }

    [Column("state")]
    [StringLength(50, ErrorMessage = "State cannot exceed 50 characters.")]
    [RegularExpression(@"^[a-zA-Z\s\-'.]+$", ErrorMessage = "State can only contain letters, spaces, hyphens, apostrophes, and periods.")]
    public string State { get; set; }

    [Column("postcode")]
    [StringLength(20, ErrorMessage = "Postcode cannot exceed 20 characters.")]
    [RegularExpression(@"^\d{5}(-\d{4})?$", ErrorMessage = "Postcode must be a valid format (e.g. 12345 or 12345-6789).")]
    public string Postcode { get; set; }

    [Column("country")]
    [StringLength(50, ErrorMessage = "Country cannot exceed 50 characters.")]
    [RegularExpression(@"^[a-zA-Z\s\-'.]+$", ErrorMessage = "Country can only contain letters, spaces, hyphens, apostrophes, and periods.")]
    public string Country { get; set; }

    [Required(ErrorMessage = "Graduation year is required.")]
    [Column("graduation_year")]
    [Range(1900, 2100, ErrorMessage = "Graduation year must be between 1900 and 2100.")]
    [Display(Name = "Graduation Year")]
    public int GraduationYear { get; set; }

    [Column("solicitation_code")]
    [Display(Name = "Allow Contact (Solicitation)")]
    public bool SolicitationCode { get; set; }

    [Column("social_media_account")]
    [StringLength(255, ErrorMessage = "Social media account cannot exceed 255 characters.")]
    [Url(ErrorMessage = "Social media account must be a valid URL.")]
    [Display(Name = "Social Media Account")]
    public string SocialMediaAccount { get; set; }

    [Column("privacy")]
    public bool Privacy { get; set; }

    [Column("is_active")]
    [Display(Name = "Is Active?")]
    public bool IsActive { get; set; }

    [Column("last_updated", TypeName = "datetime")]
    [Display(Name = "Last Updated")]
    public DateTime LastUpdated { get; set; }

    [InverseProperty("Alumni")]
    public virtual ICollection<AlumniDegree> AlumniDegrees { get; set; } = new List<AlumniDegree>();

    [InverseProperty("Alumni")]
    public virtual ICollection<AlumniEmployment> AlumniEmployments { get; set; } = new List<AlumniEmployment>();

    [InverseProperty("Alumni")]
    public virtual ICollection<AlumniInternship> AlumniInternships { get; set; } = new List<AlumniInternship>();

    [InverseProperty("Alumni")]
    public virtual ICollection<AlumniMessage> AlumniMessages { get; set; } = new List<AlumniMessage>();

    [InverseProperty("Alumni")]
    public virtual ICollection<AlumniOrganization> AlumniOrganizations { get; set; } = new List<AlumniOrganization>();
}
