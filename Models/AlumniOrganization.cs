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
    public int AlumniOrganizationId { get; set; }

    [Column("alumni_id")]
    public int AlumniId { get; set; }

    [Column("organization_type_id")]
    public int OrganizationTypeId { get; set; }

    [Column("officer_roles")]
    [StringLength(150)]
    public string OfficerRoles { get; set; }

    [ForeignKey("AlumniId")]
    [InverseProperty("AlumniOrganizations")]
    public virtual Alumni Alumni { get; set; }

    [ForeignKey("OrganizationTypeId")]
    [InverseProperty("AlumniOrganizations")]
    public virtual OrganizationType OrganizationType { get; set; }
}
