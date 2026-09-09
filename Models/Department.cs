using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alumni_Management_System.Models;

[Table("Departments")]
public partial class Department
{
    [Key]
    [Column("department_id")]
    [Display(Name = "Department ID")]
    public int DepartmentId { get; set; }

    [Column("college_id")]
    [Display(Name = "College")]
    public int CollegeId { get; set; }

    [Required]
    [Column("department_name")]
    [StringLength(150)]
    [Display(Name = "Department Name")]
    public string DepartmentName { get; set; }

    [Column("is_active")]
    [Display(Name = "Is Active?")]
    public bool IsActive { get; set; } = true;

    [ForeignKey("CollegeId")]
    [InverseProperty("Departments")]
    public virtual College College { get; set; }

    [InverseProperty("Department")]
    public virtual ICollection<DegreeProgram> DegreePrograms { get; set; } = new List<DegreeProgram>();

    [InverseProperty("Department")]
    public virtual ICollection<StudentOrganization> StudentOrganizations { get; set; } = new List<StudentOrganization>();
}
