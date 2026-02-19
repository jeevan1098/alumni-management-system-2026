using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Alumni_Management_System.Models;

[Table("Alumni_Internships")]
public partial class AlumniInternship
{
    [Key]
    [Column("alumni_internship_id")]
    [Display(Name = "Alumni Internship ID")]
    public int AlumniInternshipId { get; set; }

    [Column("alumni_id")]
    [Display(Name = "Alumni ID")]
    public int AlumniId { get; set; }

    [Column("employer_id")]
    [Display(Name = "Employer ID")]
    public int EmployerId { get; set; }

    [Required]
    [Column("internship_type")]
    [StringLength(50)]
    [Display(Name = "Internship Type")]
    public string InternshipType { get; set; }

    [Required]
    [Column("title")]
    [StringLength(100)]
    public string Title { get; set; }

    [Column("start_date")]
    [Display(Name = "Start Date")]
    public DateOnly StartDate { get; set; }

    [Column("end_date")]
    [Display(Name = "End Date")]
    public DateOnly EndDate { get; set; }

    [ForeignKey("AlumniId")]
    [InverseProperty("AlumniInternships")]
    public virtual Alumni Alumni { get; set; }

    [ForeignKey("EmployerId")]
    [InverseProperty("AlumniInternships")]
    public virtual Employer Employer { get; set; }
}
