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

    [Required]
    [Column("jag_id")]
    [StringLength(20)]
    [RegularExpression(@"^J\d+$", ErrorMessage = "JAG ID must start with 'J' followed by numbers only (e.g. J09999999).")]
    [Display(Name = "JAG ID")]
    public string JagId { get; set; }

    [Column("user_id")]
    [StringLength(450)]
    [Display(Name = "User ID")]
    public string? UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual AppUser User { get; set; }

    [Column("prefix")]
    [StringLength(10)]
    public string Prefix { get; set; }

    [Required]
    [Column("first_name")]
    [StringLength(50)]
    [Display(Name = "First Name")]
    public string FirstName { get; set; }

    [Column("preferred_first_name")]
    [StringLength(50)]
    [Display(Name = "Preferred First Name")]
    public string PreferredFirstName { get; set; }

    [Required]
    [Column("last_name")]
    [StringLength(50)]
    [Display(Name = "Last Name")]
    public string LastName { get; set; }

    [Column("gender")]
    [StringLength(20)]
    public string Gender { get; set; }

    [Column("age_at_graduation")]
    [Range(15, 150, ErrorMessage = "Age at graduation must be between 15 and 150.")]
    [Display(Name = "Age at Graduation")]
    public int? AgeAtGraduation { get; set; }

    [Column("student_email")]
    [StringLength(150)]
    [Display(Name = "Student Email")]
    public string StudentEmail { get; set; }

    // Not Required — OTH Email may be empty in Excel
    [Column("permanent_email")]
    [StringLength(150)]
    [Display(Name = "Permanent Email")]
    public string PermanentEmail { get; set; }

    [Column("phone")]
    [StringLength(20)]
    public string Phone { get; set; }

    [Column("address")]
    [StringLength(255)]
    public string Address { get; set; }

    [Column("city")]
    [StringLength(100)]
    public string City { get; set; }

    [Column("state")]
    [StringLength(50)]
    public string State { get; set; }

    [Column("postcode")]
    [StringLength(20)]
    public string Postcode { get; set; }

    [Column("country")]
    [StringLength(50)]
    public string Country { get; set; }

    [Column("graduation_year")]
    [Display(Name = "Graduation Year")]
    public int GraduationYear { get; set; }

    [Column("solicitation_code")]
    [Display(Name = "Allow Contact (Solicitation)")]
    public bool SolicitationCode { get; set; }

    [Column("social_media_account")]
    [StringLength(255)]
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
