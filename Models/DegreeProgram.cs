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

    [Required]
    [Column("institution")]
    [StringLength(150)]
    public string Institution { get; set; }

    [Required]
    [Column("degree_type")]
    [StringLength(20)]
    [Display(Name = "Degree Type")]
    public string DegreeType { get; set; }

    [Required]
    [Column("major_field_of_study")]
    [StringLength(100)]
    [Display(Name = "Major Field of Study")]
    public string MajorFieldOfStudy { get; set; }

    [Column("department_id")]
    [Display(Name = "Department")]
    public int DepartmentId { get; set; }

    [Column("is_active")]
    [Display(Name = "Is Active?")]
    public bool IsActive { get; set; } = true;

    [ForeignKey("DepartmentId")]
    [InverseProperty("DegreePrograms")]
    public virtual Department Department { get; set; }

    [InverseProperty("Degree")]
    public virtual ICollection<AlumniDegree> AlumniDegrees { get; set; } = new List<AlumniDegree>();
}
