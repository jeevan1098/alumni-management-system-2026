using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Alumni_Management_System.Models;

[Table("Alumni_Degrees")]
public partial class AlumniDegree
{
    [Key]
    [Column("alumni_degree_id")]
    [Display(Name = "Alumni Degree ID")]
    public int AlumniDegreeId { get; set; }

    [Required(ErrorMessage = "Alumni ID is required.")]
    [Column("alumni_id")]
    [Display(Name = "Alumni ID")]
    public int AlumniId { get; set; }

    [Required(ErrorMessage = "Degree ID is required.")]
    [Column("degree_id")]
    [Display(Name = "Degree ID")]
    public int DegreeId { get; set; }

    [Required(ErrorMessage = "Date conferred is required.")]
    [Column("date_conferred")]
    [Display(Name = "Date Conferred")]
    public DateOnly DateConferred { get; set; }

    [Column("years_to_complete_degree")]
    [Range(1, 20, ErrorMessage = "Years to complete degree must be between 1 and 20.")]
    [Display(Name = "Years To Complete Degree")]
    public int? YearsToCompleteDegree { get; set; }

    [Column("gpa", TypeName = "decimal(3, 2)")]
    [Range(0.0, 4.0, ErrorMessage = "GPA must be between 0.0 and 4.0.")]
    [Display(Name = "Grade")]
    public decimal? Gpa { get; set; }

    [Column("employment_while_studying")]
    [StringLength(50, ErrorMessage = "Employment while studying cannot exceed 50 characters.")]
    [Display(Name = "Employment While Studying")]
    public string EmploymentWhileStudying { get; set; }

    [Column("degree_specific_job")]
    [Display(Name = "Any Degree Specific Job?")]
    public bool? DegreeSpecificJob { get; set; }

    [Column("participated_in_research")]
    [Display(Name = "Participated In Research(s)?")]
    public bool? ParticipatedInResearch { get; set; }

    [Column("job_secured_upon_graduation")]
    [Display(Name = "Job Secured Upon Graduation?")]
    public bool? JobSecuredUponGraduation { get; set; }

    [Column("attended_or_plans_grad_school")]
    [Display(Name = "Attended Or Plans to Grad School?")]
    public bool? AttendedOrPlansGradSchool { get; set; }

    [ForeignKey("AlumniId")]
    [InverseProperty("AlumniDegrees")]
    public virtual Alumni Alumni { get; set; }

    [ForeignKey("DegreeId")]
    [InverseProperty("AlumniDegrees")]
    public virtual DegreeProgram Degree { get; set; }
}
