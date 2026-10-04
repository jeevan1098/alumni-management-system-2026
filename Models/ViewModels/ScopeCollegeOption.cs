using System.Collections.Generic;

namespace Alumni_Management_System.Models.ViewModels;

// One college in the Manage Scope / Create User scope picker, with the
// departments under it the signed-in admin is allowed to grant.
public class ScopeCollegeOption
{
    public int CollegeId { get; set; }
    public string CollegeName { get; set; }

    // False when the admin only holds some departments of this college - they
    // can grant those departments but not the whole college.
    public bool CanGrantWholeCollege { get; set; }

    public List<ScopeDepartmentOption> Departments { get; set; } = new();
}

// Model for the shared _ScopePicker partial.
public class ScopePickerModel
{
    public List<ScopeCollegeOption> Options { get; set; } = new();
    public List<int> SelectedCollegeIds { get; set; } = new();
    public List<int> SelectedDepartmentIds { get; set; } = new();

    // Keeps checkbox ids unique when the picker appears on a page with others.
    public string IdPrefix { get; set; } = "scope";
}

public class ScopeDepartmentOption
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; }
}
