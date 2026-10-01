using System.ComponentModel.DataAnnotations;

namespace Alumni_Management_System.Models.ViewModels
{
    public class VerifyJagIdViewModel
    {
        [Required(ErrorMessage = "JAG ID is required")]
        [RegularExpression(JagIdFormat.Pattern, ErrorMessage = JagIdFormat.ErrorMessage)]
        [Display(Name = "JAG ID")]
        public string JagId { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
    }
}

