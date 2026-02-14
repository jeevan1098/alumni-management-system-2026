using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Alumni_Management_System.Models;

[Table("Alumni_Employment")]
public partial class AlumniEmployment
{
    [Key]
    [Column("alumni_employment_id")]
    public int AlumniEmploymentId { get; set; }

    [Column("alumni_id")]
    public int AlumniId { get; set; }

    [Column("employer_id")]
    public int EmployerId { get; set; }

    [Required]
    [Column("job_title")]
    [StringLength(100)]
    public string JobTitle { get; set; }

    [Column("start_date")]
    public DateOnly StartDate { get; set; }

    [Column("end_date")]
    public DateOnly? EndDate { get; set; }

    [Column("salary_range")]
    [StringLength(50)]
    public string SalaryRange { get; set; }

    [ForeignKey("AlumniId")]
    [InverseProperty("AlumniEmployments")]
    public virtual Alumni Alumni { get; set; }

    [ForeignKey("EmployerId")]
    [InverseProperty("AlumniEmployments")]
    public virtual Employer Employer { get; set; }
}
