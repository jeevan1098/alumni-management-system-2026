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

    [Column("alumni_id")]
    [Display(Name = "Alumni ID")]
    public int AlumniId { get; set; }

    [Column("degree_id")]
    [Display(Name = "Degree ID")]
    public int DegreeId { get; set; }

    [Column("date_conferred")]
    [Display(Name = "Date Conferred")]
    public DateOnly DateConferred { get; set; }

    [Column("years_to_complete_degree")]
    [Display(Name = "Years To Complete Degree")]
    public int? YearsToCompleteDegree { get; set; }

    [Column("gpa", TypeName = "decimal(3, 2)")]
    [Display(Name = "Grade")]
    public decimal? Gpa { get; set; }

    [Column("employment_while_studying")]
    [StringLength(50)]
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
