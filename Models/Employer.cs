using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Alumni_Management_System.Models;

public partial class Employer
{
    [Key]
    [Column("employer_id")]
    [Display(Name = "Employer ID")]
    public int EmployerId { get; set; }

    [Required(ErrorMessage = "Employer name is required.")]
    [Column("employer_name")]
    [StringLength(150, ErrorMessage = "Employer name cannot exceed 150 characters.")]
    [Display(Name = "Employer Name")]
    public string EmployerName { get; set; }

    [Column("location")]
    [StringLength(150, ErrorMessage = "Location cannot exceed 150 characters.")]
    public string Location { get; set; }

    [Column("industry")]
    [StringLength(100, ErrorMessage = "Industry cannot exceed 100 characters.")]
    public string Industry { get; set; }

    [InverseProperty("Employer")]
    public virtual ICollection<AlumniEmployment> AlumniEmployments { get; set; } = new List<AlumniEmployment>();

    [InverseProperty("Employer")]
    public virtual ICollection<AlumniInternship> AlumniInternships { get; set; } = new List<AlumniInternship>();
}
