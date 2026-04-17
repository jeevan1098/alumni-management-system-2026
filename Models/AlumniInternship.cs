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

    [Required(ErrorMessage = "Alumni ID is required.")]
    [Column("alumni_id")]
    [Display(Name = "Alumni ID")]
    public int AlumniId { get; set; }

    [Required(ErrorMessage = "Employer ID is required.")]
    [Column("employer_id")]
    [Display(Name = "Employer ID")]
    public int EmployerId { get; set; }

    [Required(ErrorMessage = "Internship type is required.")]
    [Column("internship_type")]
    [StringLength(50, ErrorMessage = "Internship type cannot exceed 50 characters.")]
    [Display(Name = "Internship Type")]
    public string InternshipType { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [Column("title")]
    [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
    public string Title { get; set; }

    [Required(ErrorMessage = "Start date is required.")]
    [Column("start_date")]
    [Display(Name = "Start Date")]
    public DateOnly StartDate { get; set; }

    [Required(ErrorMessage = "End date is required.")]
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
