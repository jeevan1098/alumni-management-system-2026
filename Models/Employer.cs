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

    [Required]
    [Column("employer_name")]
    [StringLength(150)]
    [Display(Name = "Employer Name")]
    public string EmployerName { get; set; }

    [Column("location")]
    [StringLength(150)]
    public string Location { get; set; }

    [Column("industry")]
    [StringLength(100)]
    public string Industry { get; set; }

    [InverseProperty("Employer")]
    public virtual ICollection<AlumniEmployment> AlumniEmployments { get; set; } = new List<AlumniEmployment>();

    [InverseProperty("Employer")]
    public virtual ICollection<AlumniInternship> AlumniInternships { get; set; } = new List<AlumniInternship>();
}
