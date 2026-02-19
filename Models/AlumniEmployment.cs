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
    [Display(Name = "Alumni Employment ID")]
    public int AlumniEmploymentId { get; set; }

    [Column("alumni_id")]
    [Display(Name = "Alumni ID")]
    public int AlumniId { get; set; }

    [Column("employer_id")]
    [Display(Name = "Employer ID")]
    public int EmployerId { get; set; }

    [Required]
    [Column("job_title")]
    [StringLength(100)]
    [Display(Name = "Job Title")]
    public string JobTitle { get; set; }

    [Column("start_date")]
    [Display(Name = "Start Date")]
    public DateOnly StartDate { get; set; }

    [Column("end_date")]
    [Display(Name = "End Date")]
    public DateOnly? EndDate { get; set; }

    [Column("salary_range")]
    [StringLength(50)]
    [Display(Name = "Salary Range")]
    public string SalaryRange { get; set; }

    [ForeignKey("AlumniId")]
    [InverseProperty("AlumniEmployments")]
    public virtual Alumni Alumni { get; set; }

    [ForeignKey("EmployerId")]
    [InverseProperty("AlumniEmployments")]
    public virtual Employer Employer { get; set; }
}
