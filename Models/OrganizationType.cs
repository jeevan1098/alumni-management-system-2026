using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Alumni_Management_System.Models;

[Table("Organization_Types")]
public partial class OrganizationType
{
    [Key]
    [Column("organization_type_id")]
    [Display(Name = "Organization Type ID")]
    public int OrganizationTypeId { get; set; }

    [Required(ErrorMessage = "Organization name is required.")]
    [Column("organization_name")]
    [StringLength(150, ErrorMessage = "Organization name cannot exceed 150 characters.")]
    [Display(Name = "Organization Name")]
    public string OrganizationName { get; set; }

    [InverseProperty("OrganizationType")]
    public virtual ICollection<AlumniOrganization> AlumniOrganizations { get; set; } = new List<AlumniOrganization>();
}
