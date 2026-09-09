using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alumni_Management_System.Models.Audit;

[Table("Employers_Audit")]
public partial class EmployerAudit
{
    [Key]
    [Column("audit_id")]
    public int AuditId { get; set; }

    [Column("employer_id")]
    public int EmployerId { get; set; }

    [Required]
    [Column("action_type")]
    [StringLength(20)]
    public string ActionType { get; set; }

    [Column("changed_by")]
    [StringLength(256)]
    public string ChangedBy { get; set; }

    [Column("changed_at", TypeName = "datetime")]
    public DateTime ChangedAt { get; set; }

    [Column("old_values")]
    public string OldValues { get; set; }

    [Column("new_values")]
    public string NewValues { get; set; }
}
