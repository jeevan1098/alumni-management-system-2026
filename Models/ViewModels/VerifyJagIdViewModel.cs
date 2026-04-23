using System.ComponentModel.DataAnnotations;

namespace Alumni_Management_System.Models.ViewModels
{
    public class VerifyJagIdViewModel
    {
        [Required(ErrorMessage = "JAG ID is required")]
        [RegularExpression(@"^[Jj]\d+$", ErrorMessage = "JAG ID must start with 'J' followed by numbers only (e.g. J0012345).")]
        [Display(Name = "JAG ID")]
        public string JagId { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
    }
}