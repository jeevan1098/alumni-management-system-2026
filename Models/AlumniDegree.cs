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
    public int AlumniDegreeId { get; set; }

    [Column("alumni_id")]
    public int AlumniId { get; set; }

    [Column("degree_id")]
    public int DegreeId { get; set; }

    [Column("date_conferred")]
    public DateOnly DateConferred { get; set; }

    [Column("years_to_complete_degree")]
    public int? YearsToCompleteDegree { get; set; }

    [Column("gpa", TypeName = "decimal(3, 2)")]
    public decimal? Gpa { get; set; }

    [Column("employment_while_studying")]
    [StringLength(50)]
    public string EmploymentWhileStudying { get; set; }

    [Column("degree_specific_job")]
    public bool? DegreeSpecificJob { get; set; }

    [Column("participated_in_research")]
    public bool? ParticipatedInResearch { get; set; }

    [Column("job_secured_upon_graduation")]
    public bool? JobSecuredUponGraduation { get; set; }

    [Column("attended_or_plans_grad_school")]
    public bool? AttendedOrPlansGradSchool { get; set; }

    [ForeignKey("AlumniId")]
    [InverseProperty("AlumniDegrees")]
    public virtual Alumni Alumni { get; set; }

    [ForeignKey("DegreeId")]
    [InverseProperty("AlumniDegrees")]
    public virtual DegreeProgram Degree { get; set; }
}
