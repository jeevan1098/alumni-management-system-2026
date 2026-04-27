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

    [Required(ErrorMessage = "Alumni ID is required.")]
    [Column("alumni_id")]
    [Display(Name = "Alumni ID")]
    public int AlumniId { get; set; }

    [Required(ErrorMessage = "Employer ID is required.")]
    [Column("employer_id")]
    [Display(Name = "Employer ID")]
    public int EmployerId { get; set; }

    [Required(ErrorMessage = "Job title is required.")]
    [Column("job_title")]
    [StringLength(100, ErrorMessage = "Job title cannot exceed 100 characters.")]
    [Display(Name = "Job Title")]
    public string JobTitle { get; set; }

    [Required(ErrorMessage = "Start date is required.")]
    [Column("start_date")]
    [Display(Name = "Start Date")]
    public DateOnly StartDate { get; set; }

    [Column("end_date")]
    [Display(Name = "End Date")]
    public DateOnly? EndDate { get; set; }

    [Column("salary_range")]
    [StringLength(50, ErrorMessage = "Salary range cannot exceed 50 characters.")]
    [RegularExpression(@"^[\$\d,\s\-kKmMbBa-zA-Z\+]+$", ErrorMessage = "Salary range contains invalid characters.")]
    [Display(Name = "Salary Range")]
    public string SalaryRange { get; set; }

    [ForeignKey("AlumniId")]
    [InverseProperty("AlumniEmployments")]
    public virtual Alumni Alumni { get; set; }

    [ForeignKey("EmployerId")]
    [InverseProperty("AlumniEmployments")]
    public virtual Employer Employer { get; set; }
}