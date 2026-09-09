using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alumni_Management_System.Models;

[Table("Student_Organizations")]
public partial class StudentOrganization
{
    [Key]
    [Column("organization_id")]
    [Display(Name = "Organization ID")]
    public int OrganizationId { get; set; }

    [Required]
    [Column("organization_name")]
    [StringLength(150)]
    [Display(Name = "Organization Name")]
    public string OrganizationName { get; set; }

    [Column("college_id")]
    [Display(Name = "College")]
    public int CollegeId { get; set; }

    [Column("department_id")]
    [Display(Name = "Department")]
    public int? DepartmentId { get; set; }

    [Column("is_active")]
    [Display(Name = "Is Active?")]
    public bool IsActive { get; set; } = true;

    [ForeignKey("CollegeId")]
    [InverseProperty("StudentOrganizations")]
    public virtual College College { get; set; }

    [ForeignKey("DepartmentId")]
    [InverseProperty("StudentOrganizations")]
    public virtual Department Department { get; set; }

    [InverseProperty("Organization")]
    public virtual ICollection<AlumniOrganization> AlumniOrganizations { get; set; } = new List<AlumniOrganization>();
}
