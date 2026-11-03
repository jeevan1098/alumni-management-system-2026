using Alumni_Management_System.Services;

namespace Alumni_Management_System.Models.ViewModels;

public class SiteContentEditViewModel
{
    public string Key { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public string ReturnUrl { get; set; }

    // Display only - filled by the controller, never bound from the form.
    public SiteContentDefinition Definition { get; set; }
    public SiteContentEntry Saved { get; set; }
    public string PreviewSubject { get; set; }
    public string PreviewHtml { get; set; }
}
