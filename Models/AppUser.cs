// File: AppUser.cs
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alumni_Management_System.Models
{
    public class AppUser : IdentityUser
    {
        [StringLength(20, ErrorMessage = "JAG ID cannot exceed 20 characters.")]
        [RegularExpression(@"^J\d+$", ErrorMessage = "JAG ID must start with 'J' followed by numbers only (e.g. J0012345).")]
        [Display(Name = "JAG ID")]
        public string JagId { get; set; }

        [InverseProperty("User")]
        public virtual ICollection<Alumni> Alumni { get; set; } = new List<Alumni>();

        [Required(ErrorMessage = "Created at date is required.")]
        [Column("created_at", TypeName = "datetime")]
        [Display(Name = "Created At?")]
        public DateTime CreatedAt { get; set; }

        [Column("is_first_login")]
        [Display(Name = "Is First Login?")]
        public bool IsFirstLogin { get; set; } = true;

        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
