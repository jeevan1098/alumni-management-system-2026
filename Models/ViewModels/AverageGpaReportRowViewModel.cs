namespace Alumni_Management_System.Models.ViewModels;

public class AverageGpaReportRowViewModel
{
    public int Year { get; set; }
    public string DegreeType { get; set; }
    public string Major { get; set; }
    public int DegreeCount { get; set; }
    public decimal AverageGpa { get; set; }
}
