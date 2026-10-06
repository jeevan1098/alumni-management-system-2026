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
    [RegularExpression(JagIdFormat.Pattern, ErrorMessage = JagIdFormat.ErrorMessage)]
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
    [EmailAddress(ErrorMessage = "Please enter a valid email address, e.g. name@example.com.")]
    [Display(Name = "Student Email")]
    public string StudentEmail { get; set; }

    [Required]
    [Column("permanent_email")]
    [StringLength(150)]
    [EmailAddress(ErrorMessage = "Please enter a valid email address, e.g. name@example.com.")]
    [Display(Name = "Permanent Email")]
    public string PermanentEmail { get; set; }

    [Column("phone")]
    [StringLength(20)]
    [RegularExpression(@"^[0-9+()\-. ]{7,20}$", ErrorMessage = "Phone can only contain digits, spaces and + - ( ) . (7 to 20 characters).")]
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

    // Non-nullable, so a blank box fails model binding before [Required] ever
    // runs - the message here is what the browser shows; AlumniController
    // swaps the server-side "The value '' is invalid." for the same text.
    [Column("graduation_year")]
    [Required(ErrorMessage = GraduationYearRequiredMessage)]
    [GraduationYear]
    [Display(Name = "Most Recent Graduation Year")]
    public int GraduationYear { get; set; }

    public const string GraduationYearRequiredMessage = "Please enter the graduation year (for example, 2024).";

    // Messages only - see SolicitationHelp. Does NOT affect who can see the
    // profile in the directory (that's everyone; Privacy only hides contact details).
    [Column("solicitation_code")]
    [Display(Name = "Allow Contact (Solicitation)")]
    public bool SolicitationCode { get; set; }

    // Shown as a clickable link on the profile page, so only real web
    // addresses are allowed - a "javascript:" value would run as a script
    // for whoever clicks it.
    [Column("social_media_account")]
    [StringLength(255)]
    [RegularExpression(@"^https?://\S+$", ErrorMessage = "Please enter a full web address starting with http:// or https://, e.g. https://www.linkedin.com/in/yourname.")]
    [Display(Name = "Social Media Account")]
    public string SocialMediaAccount { get; set; }

    // True when the social link is safe to render as a link (see above);
    // older or imported values that aren't are shown as plain text instead.
    [NotMapped]
    public bool HasSafeSocialMediaLink =>
        Uri.TryCreate(SocialMediaAccount, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

    // Ticked = contact details are hidden from other alumni - see PrivacyHelp
    // and HideContactDetails(). Admin/Staff and the alumnus themself still see them.
    [Column("privacy")]
    [Display(Name = "Keep Contact Details Private")]
    public bool Privacy { get; set; }

    // Explanations shown next to these two settings on every page that
    // displays or edits them, so the wording stays the same everywhere.
    public const string PrivacyHelp =
        "When checked, other alumni can still see the name, graduation year, college, city/state and degrees in the alumni directory, " +
        "but NOT the contact details - email addresses, phone number, street address, date of birth and social media account. " +
        "Administrators and staff can always see everything.";

    public const string SolicitationHelp =
        "When checked, the alumni office may send messages to this alumnus (newsletters, events, announcements and fundraising). " +
        "This only controls messages - it doesn't change who can see the profile or contact details.";

    public const string IsActiveHelp =
        "Checked means the alumnus is living. Every alumnus is active by default - " +
        "uncheck this only after confirming that the alumnus has passed away. " +
        "Only administrators can see or change this.";

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
