using System.ComponentModel.DataAnnotations;

namespace Alumni_Management_System.Models.ViewModels;

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "Enter your username or email so we can identify your account")]
    [Display(Name = "Username or Email")]
    public string UsernameOrEmail { get; set; }
}
