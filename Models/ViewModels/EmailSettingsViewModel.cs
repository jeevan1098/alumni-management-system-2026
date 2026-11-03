using System.ComponentModel.DataAnnotations;

namespace Alumni_Management_System.Models.ViewModels;

public class EmailSettingsViewModel
{
    [Required(ErrorMessage = "Please fill in this field.")]
    [StringLength(200)]
    [Display(Name = "SMTP server")]
    public string Host { get; set; }

    [Range(1, 65535, ErrorMessage = "Port must be between 1 and 65535.")]
    [Display(Name = "Port")]
    public int Port { get; set; } = 587;

    [Display(Name = "Use SSL/TLS")]
    public bool EnableSsl { get; set; } = true;

    [Required(ErrorMessage = "Please fill in this field.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address, e.g. name@example.com.")]
    [StringLength(200)]
    [Display(Name = "From email address")]
    public string FromAddress { get; set; }

    [StringLength(100)]
    [Display(Name = "From name")]
    public string FromName { get; set; }

    [StringLength(200)]
    [Display(Name = "SMTP username")]
    public string Username { get; set; }

    // Never sent back to the browser - left blank keeps the current password.
    [DataType(DataType.Password)]
    [StringLength(200)]
    [Display(Name = "SMTP password")]
    public string NewPassword { get; set; }

    [EmailAddress(ErrorMessage = "Please enter a valid email address, e.g. name@example.com.")]
    [Display(Name = "Send a test email to")]
    public string TestEmailTo { get; set; }

    // Display only - filled by the controller.
    public bool HasPassword { get; set; }
    public bool IsCustomized { get; set; }
    public string UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
