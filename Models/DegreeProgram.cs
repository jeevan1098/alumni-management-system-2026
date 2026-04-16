using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Alumni_Management_System.Data;
using Microsoft.AspNetCore.Identity;

namespace Alumni_Management_System.Models;

[Table("Degree_Programs")]
public partial class DegreeProgram
{
    [Key]
    [Column("degree_id")]
    [Display(Name = "Degree ID")]
    public int DegreeId { get; set; }

    [Required(ErrorMessage = "Institution is required.")]
    [Column("institution")]
    [StringLength(150, ErrorMessage = "Institution name cannot exceed 150 characters.")]
    public string Institution { get; set; }

    [Required(ErrorMessage = "Degree type is required.")]
    [Column("degree_type")]
    [StringLength(20, ErrorMessage = "Degree type cannot exceed 20 characters.")]
    [Display(Name = "Degree Type")]
    public string DegreeType { get; set; }

    [Required(ErrorMessage = "Major field of study is required.")]
    [Column("major_field_of_study")]
    [StringLength(100, ErrorMessage = "Major field of study cannot exceed 100 characters.")]
    [Display(Name = "Major Field of Study")]
    public string MajorFieldOfStudy { get; set; }

    [Required(ErrorMessage = "Department is required.")]
    [Column("department")]
    [StringLength(100, ErrorMessage = "Department cannot exceed 100 characters.")]
    [Display(Name = "Department")]
    public string Department { get; set; }

    [InverseProperty("Degree")]
    public virtual ICollection<AlumniDegree> AlumniDegrees { get; set; } = new List<AlumniDegree>();
}
