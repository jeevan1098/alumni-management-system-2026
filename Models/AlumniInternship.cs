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
    public int AlumniInternshipId { get; set; }

    [Column("alumni_id")]
    public int AlumniId { get; set; }

    [Column("employer_id")]
    public int EmployerId { get; set; }

    [Required]
    [Column("internship_type")]
    [StringLength(50)]
    public string InternshipType { get; set; }

    [Required]
    [Column("title")]
    [StringLength(100)]
    public string Title { get; set; }

    [Column("start_date")]
    public DateOnly StartDate { get; set; }

    [Column("end_date")]
    public DateOnly EndDate { get; set; }

    [ForeignKey("AlumniId")]
    [InverseProperty("AlumniInternships")]
    public virtual Alumni Alumni { get; set; }

    [ForeignKey("EmployerId")]
    [InverseProperty("AlumniInternships")]
    public virtual Employer Employer { get; set; }
}
