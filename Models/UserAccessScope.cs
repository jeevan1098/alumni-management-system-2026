using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Alumni_Management_System.Models;

// Grants a user's role scoped access to one college (and optionally one
// department within it), or system-wide access when CollegeId is null and
// the paired AppUserRole.ScopeMode is "System". Reuses AspNetUsers/AspNetRoles
// (via the UserId/RoleId pair on AppUserRole) instead of duplicating them,
// per team decision to build on top of ASP.NET Identity rather than a fully
// custom role system.
[Table("User_Access_Scopes")]
public partial class UserAccessScope
{
    [Key]
    [Column("user_access_scope_id")]
    [Display(Name = "User Access Scope ID")]
    public int UserAccessScopeId { get; set; }

    [Required]
    [Column("user_id")]
    [Display(Name = "User")]
    public string UserId { get; set; }

    [Required]
    [Column("role_id")]
    [Display(Name = "Role")]
    public string RoleId { get; set; }

    [Column("college_id")]
    [Display(Name = "College")]
    public int? CollegeId { get; set; }

    [Column("department_id")]
    [Display(Name = "Department")]
    public int? DepartmentId { get; set; }

    [Required]
    [Column("access_level")]
    [StringLength(20)]
    [Display(Name = "Access Level")]
    public string AccessLevel { get; set; } = "Full";

    [Column("is_active")]
    [Display(Name = "Is Active?")]
    public bool IsActive { get; set; } = true;

    [ForeignKey("UserId")]
    public virtual AppUser User { get; set; }

    [ForeignKey("RoleId")]
    public virtual IdentityRole Role { get; set; }

    [ForeignKey("CollegeId")]
    public virtual College College { get; set; }

    [ForeignKey("DepartmentId")]
    public virtual Department Department { get; set; }
}
