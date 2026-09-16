using System.ComponentModel.DataAnnotations;

namespace Alumni_Management_System.Models.ViewModels
{
    public class VerifyJagIdViewModel
    {
        [Required(ErrorMessage = "JAG ID is required")]
        [RegularExpression(@"^J\d{8}$", ErrorMessage = "JAG ID must start with 'J' followed by 8 digits (e.g., J00123456).")]
        [Display(Name = "JAG ID")]
        public string JagId { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
    }
}

