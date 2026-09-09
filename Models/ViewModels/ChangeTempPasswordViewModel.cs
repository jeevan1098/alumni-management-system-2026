using System.ComponentModel.DataAnnotations;

namespace Alumni_Management_System.Models.ViewModels;

// Forced next step after signing in with a temp password an admin issued
// via UsersController.ResetPassword (a forgot-password request).
public class ChangeTempPasswordViewModel
{
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Temporary Password")]
    public string TempPassword { get; set; }

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
