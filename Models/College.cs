using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alumni_Management_System.Models;

[Table("Colleges")]
public partial class College
{
    [Key]
    [Column("college_id")]
    [Display(Name = "College ID")]
    public int CollegeId { get; set; }

    [Required]
    [Column("college_name")]
    [StringLength(150)]
    [Display(Name = "College Name")]
    public string CollegeName { get; set; }

    [Column("is_internal")]
    [Display(Name = "Internal College?")]
    public bool IsInternal { get; set; }

    [Column("is_active")]
    [Display(Name = "Is Active?")]
    public bool IsActive { get; set; } = true;

    [InverseProperty("College")]
    public virtual ICollection<Department> Departments { get; set; } = new List<Department>();

    [InverseProperty("College")]
    public virtual ICollection<Alumni> Alumni { get; set; } = new List<Alumni>();

    [InverseProperty("College")]
    public virtual ICollection<StudentOrganization> StudentOrganizations { get; set; } = new List<StudentOrganization>();
}
