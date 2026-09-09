using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Alumni_Management_System.Models;

public class AppUserRole : IdentityUserRole<string>
{
    [Required]
    [Column("scope_mode")]
    [StringLength(20)]
    [Display(Name = "Scope Mode")]
    public string ScopeMode { get; set; } = "System";

    [Column("assigned_at", TypeName = "datetime")]
    [Display(Name = "Assigned At")]
    public DateTime AssignedAt { get; set; } = DateTime.Now;

    [Column("assigned_by")]
    [Display(Name = "Assigned By")]
    public string AssignedBy { get; set; }

    [Column("is_active")]
    [Display(Name = "Is Active?")]
    public bool IsActive { get; set; } = true;
}
