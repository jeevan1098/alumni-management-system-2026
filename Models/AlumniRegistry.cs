using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Alumni_Management_System.Models;

[Table("Alumni_Registry")]
[Index("JagId", Name = "UQ__Alumni_R__FBF400ED39BA228D", IsUnique = true)]
public partial class AlumniRegistry
{
    [Key]
    [Column("registry_id")]
    public int RegistryId { get; set; }

    [Required(ErrorMessage = "JAG ID is required.")]
    [Column("jag_id")]
    [StringLength(20, ErrorMessage = "JAG ID cannot exceed 20 characters.")]
    [RegularExpression(@"^J\d+$", ErrorMessage = "JAG ID must start with 'J' followed by numbers only (e.g. J0012345).")]
    [Display(Name = "JAG ID")]
    public string JagId { get; set; }

    [Required(ErrorMessage = "First name is required.")]
    [Column("first_name")]
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
    [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "First name can only contain letters, spaces, hyphens, and apostrophes.")]
    [Display(Name = "First Name")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Last name is required.")]
    [Column("last_name")]
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
    [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "Last name can only contain letters, spaces, hyphens, and apostrophes.")]
    [Display(Name = "Last Name")]
    public string LastName { get; set; }

    [Column("account_created")]
    [Display(Name = "Account Created")]
    public bool AccountCreated { get; set; }
}
