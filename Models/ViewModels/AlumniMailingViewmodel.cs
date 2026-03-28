namespace Alumni_Management_System.Models.ViewModels
{
    public class AlumniMailingViewmodel
    {
            public int AlumniId { get; set; }   // IMPORTANT for selecting users

            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string PermanentEmail { get; set; }
            public int GraduationYear { get; set; }

            public string Degree { get; set; }
            public string DegreeType { get; set; }
          

            public bool IsSelected { get; set; } // for checkbox selection
        }
    
}
