using System.ComponentModel.DataAnnotations;

namespace Alumni_Management_System.Models.ViewModels;

public class CreateUserViewModel
{
    [Required]
    [StringLength(20)]
    [RegularExpression(@"^J\d+$", ErrorMessage = "JAG ID must start with 'J' followed by numbers only.")]
    [Display(Name = "JAG ID")]
    public string JagId { get; set; }

    // Not [Required] here - only enforced server-side, and only when the
    // JagId doesn't already belong to an account (see UsersController.Create).
    // If it does, we add the selected Role to that account instead of
    // creating a new one, and these fields don't apply.
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Temporary Password")]
    public string Password { get; set; }

    [Required]
    [Display(Name = "Role")]
    public string Role { get; set; } = Constants.StaffRole;
}
