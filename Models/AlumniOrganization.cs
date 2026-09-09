using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Alumni_Management_System.Models;

[Table("Alumni_Organizations")]
public partial class AlumniOrganization
{
    [Key]
    [Column("alumni_organization_id")]
    [Display(Name = "Alumni Organization ID")]
    public int AlumniOrganizationId { get; set; }

    [Column("alumni_id")]
    [Display(Name = "Alumni ID")]
    public int AlumniId { get; set; }

    [Column("organization_id")]
    [Display(Name = "Organization ID")]
    public int OrganizationId { get; set; }

    [Column("officer_roles")]
    [StringLength(150)]
    [Display(Name = "Officer Roles")]
    public string OfficerRoles { get; set; }

    [ForeignKey("AlumniId")]
    [InverseProperty("AlumniOrganizations")]
    public virtual Alumni Alumni { get; set; }

    [ForeignKey("OrganizationId")]
    [InverseProperty("AlumniOrganizations")]
    public virtual StudentOrganization Organization { get; set; }
}
