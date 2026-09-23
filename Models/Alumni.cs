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
    [RegularExpression(@"^J\d+$", ErrorMessage = "JAG ID must start with 'J' followed by numbers only.")]
    [Display(Name = "JAG ID")]
    public string JagId { get; set; }

    // Logical link to AppUser via JagId, not a DB foreign key - this Alumni
    // row can exist (bulk-imported) before any login account is created for
    // it, and JagId doubles as the identifier for non-Alumni accounts too.
    // Set manually where needed; never eager-loaded.
    [NotMapped]
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

    [Column("middle_name")]
    [StringLength(50)]
    [Display(Name = "Middle Name")]
    public string MiddleName { get; set; }

    [Required]
    [Column("last_name")]
    [StringLength(50)]
    [Display(Name = "Last Name")]
    public string LastName { get; set; }

    [Column("suffix")]
    [StringLength(10)]
    [Display(Name = "Suffix")]
    public string Suffix { get; set; }

    [Column("gender")]
    [StringLength(20)]
    public string Gender { get; set; }

    [Column("date_of_birth")]
    [Display(Name = "Date of Birth")]
    public DateOnly? DateOfBirth { get; set; }

    [Column("college_id")]
    [Display(Name = "College")]
    public int? CollegeId { get; set; }

    [Column("student_email")]
    [StringLength(150)]
    [Display(Name = "Student Email")]
    public string StudentEmail { get; set; }

    [Required]
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
    [Display(Name = "Most Recent Graduation Year")]
    public int GraduationYear { get; set; }

    // Messages only - see SolicitationHelp. Does NOT affect who can see the
    // profile in the directory (that's everyone; Privacy only hides contact details).
    [Column("solicitation_code")]
    [Display(Name = "Allow Contact (Solicitation)")]
    public bool SolicitationCode { get; set; }

    [Column("social_media_account")]
    [StringLength(255)]
    [Display(Name = "Social Media Account")]
    public string SocialMediaAccount { get; set; }

    // Ticked = contact details are hidden from other alumni - see PrivacyHelp
    // and HideContactDetails(). Admin/Staff and the alumnus themself still see them.
    [Column("privacy")]
    [Display(Name = "Keep Contact Details Private")]
    public bool Privacy { get; set; }

    // Explanations shown next to these two settings on every page that
    // displays or edits them, so the wording stays the same everywhere.
    public const string PrivacyHelp =
        "When ticked, other alumni can still see the name, graduation year, college, city/state and degrees in the alumni directory, " +
        "but NOT the contact details - email addresses, phone number, street address, date of birth and social media account. " +
        "Administrators and staff can always see everything.";

    public const string SolicitationHelp =
        "When ticked, the alumni office may send messages to this alumnus (newsletters, events, announcements and fundraising). " +
        "This only controls messages - it doesn't change who can see the profile or contact details.";

    // Set by HideContactDetails() so views can show "Private" instead of a blank.
    [NotMapped]
    public bool ContactHidden { get; private set; }

    // Clears the contact fields on this in-memory copy for a viewer who isn't
    // allowed to see them. Only call on entities that won't be saved
    // (e.g. loaded with AsNoTracking) - it would otherwise wipe real data.
    public void HideContactDetails()
    {
        PermanentEmail = null;
        StudentEmail = null;
        Phone = null;
        Address = null;
        Postcode = null;
        DateOfBirth = null;
        SocialMediaAccount = null;
        ContactHidden = true;
    }

    [Column("is_active")]
    [Display(Name = "Is Active?")]
    public bool IsActive { get; set; }

    [Column("last_updated", TypeName = "datetime")]
    [Display(Name = "Last Updated")]
    public DateTime LastUpdated { get; set; }

    [ForeignKey("CollegeId")]
    [InverseProperty("Alumni")]
    public virtual College College { get; set; }

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
