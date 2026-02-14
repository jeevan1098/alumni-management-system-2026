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
    [RegularExpression(@"^J00\d+$", ErrorMessage = "JAG ID must start with 'J00' followed by numbers only.")]
    [Display(Name = "JAG ID")]
    public string JagId { get; set; }

    [Required]
    [Column("first_name")]
    [StringLength(50)]
    public string FirstName { get; set; }

    [Required]
    [Column("last_name")]
    [StringLength(50)]
    public string LastName { get; set; }

    [Column("graduation_year")]
    public int? GraduationYear { get; set; }

    [Column("degree_program")]
    [StringLength(100)]
    public string DegreeProgram { get; set; }

    [Column("email_on_record")]
    [StringLength(150)]
    public string EmailOnRecord { get; set; }

    [Column("account_created")]
    public bool AccountCreated { get; set; }
}
