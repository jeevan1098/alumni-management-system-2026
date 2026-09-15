// File: AppUser.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alumni_Management_System.Models
{
    // One JagId = one account: enforced at the DB level in addition to the
    // application-level check in UsersController.Create/AccountController.
    [Index(nameof(JagId), Name = "UQ_AspNetUsers_JagId", IsUnique = true)]
    public class AppUser : IdentityUser
    {
        [Required]
        [StringLength(20)]
        [RegularExpression(@"^J\d+$", ErrorMessage = "JAG ID must start with 'J' followed by numbers only.")]
        [Display(Name = "JAG ID")]
        public string JagId { get; set; }

        [Column("created_at", TypeName = "datetime")]
        [Display(Name = "Created At?")]
        public DateTime CreatedAt { get; set; }

        [Column("is_first_login")]
        [Display(Name = "Is First Login?")]
        public bool IsFirstLogin { get; set; } = true;

        // Set when an admin resets this account's password after a forgot-
        // password request (see UsersController.ResetPassword) - forces the
        // user to set their own new password right after they next sign in
        // with the emailed temporary one.
        [Column("must_change_password")]
        [Display(Name = "Must Change Password?")]
        public bool MustChangePassword { get; set; } = false;

        // Logical link to Alumni via JagId, not a DB foreign key - JagId is
        // shared by every account type (Admin/Staff/Alumni), so only some
        // AppUsers have a matching Alumni row. Set manually where needed
        // (see AlumniController.Delete/DeleteConfirmed); never eager-loaded.
        [NotMapped]
        public virtual Alumni Alumni { get; set; }
        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}