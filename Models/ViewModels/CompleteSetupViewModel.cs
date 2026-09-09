using System.ComponentModel.DataAnnotations;

namespace Alumni_Management_System.Models.ViewModels;

// First-login flow for Admin/Staff accounts created by an admin with a
// placeholder username + temporary password: they prove they know the temp
// password, then pick their own permanent username and password.
public class CompleteSetupViewModel
{
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Temporary Password")]
    public string TempPassword { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 3)]
    [RegularExpression(@"^[a-zA-Z0-9._]+$", ErrorMessage = "Username can only contain letters, numbers, dots, and underscores.")]
    [Display(Name = "New Username")]
    public string NewUsername { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "New Password")]
    public string NewPassword { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "The password and confirmation password do not match.")]
    [Display(Name = "Confirm New Password")]
    public string ConfirmPassword { get; set; }
}
