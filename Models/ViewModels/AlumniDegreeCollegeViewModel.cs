namespace Alumni_Management_System.Models.ViewModels;

// Read-only summary row for the Alumni profile page's "Colleges &
// Departments" section - one per degree, since a person can hold several
// degrees across different departments/colleges (see AlumniController.Edit).
public class AlumniDegreeCollegeViewModel
{
    public string DegreeType { get; set; }
    public string MajorFieldOfStudy { get; set; }
    public string DepartmentName { get; set; }
    public string CollegeName { get; set; }
}
