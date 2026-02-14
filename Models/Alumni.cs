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
    public int AlumniId { get; set; }

    [Column("user_id")]
    public string UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual AppUser User { get; set; }

    [Required]
    [Column("jag_id")]
    [StringLength(20)]
    [RegularExpression(@"^J00\d+$", ErrorMessage = "JAG ID must start with 'J00' followed by numbers only.")]
    [Display(Name = "JAG ID")]
    public string JagId { get; set; }

    [Column("prefix")]
    [StringLength(10)]
    public string Prefix { get; set; }

    [Required]
    [Column("first_name")]
    [StringLength(50)]
    public string FirstName { get; set; }

    [Column("preferred_first_name")]
    [StringLength(50)]
    public string PreferredFirstName { get; set; }

    [Required]
    [Column("last_name")]
    [StringLength(50)]
    public string LastName { get; set; }

    [Column("gender")]
    [StringLength(20)]
    public string Gender { get; set; }

    [Column("date_of_birth")]
    public DateOnly? DateOfBirth { get; set; }

    [Column("student_email")]
    [StringLength(150)]
    public string StudentEmail { get; set; }

    [Required]
    [Column("permanent_email")]
    [StringLength(150)]
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
    public int GraduationYear { get; set; }

    [Column("solicitation_code")]
    [StringLength(20)]
    public string SolicitationCode { get; set; }

    [Column("social_media_account")]
    [StringLength(255)]
    public string SocialMediaAccount { get; set; }

    [Column("privacy")]
    public bool Privacy { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; }

    [Column("last_updated", TypeName = "datetime")]
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

    //[ForeignKey("UserId")]
    //[InverseProperty("Alumni")]
   
    // for one-to-one relationship with user
    //public string IdentityUserId { get; set; }    

    // navigation property to allow user info
    //public virtual IdentityUser IdentityUser { get; set; }
}
