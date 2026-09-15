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

    [Required]
    [Column("jag_id")]
    [StringLength(20)]
    [RegularExpression(@"^J\d+$", ErrorMessage = "JAG ID must start with 'J' followed by numbers only.")]
    [Display(Name = "JAG ID")]
    public string JagId { get; set; }

    [Required]
    [Column("first_name")]
    [StringLength(50)]
    [Display(Name = "First Name")]
    public string FirstName { get; set; }

    [Required]
    [Column("last_name")]
    [StringLength(50)]
    [Display(Name = "Last Name")]
    public string LastName { get; set; }

    [Column("account_created")]
    [Display(Name = "Account Created")]
    public bool AccountCreated { get; set; }
}
