using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alumni_Management_System.Models
{
    [Table("Audit_Logs")]
    public class AuditLog
    {
        [Key]
        [Column("audit_id")]
        public int AuditId { get; set; }

        [Required]
        [Column("user_id")]
        public string UserId { get; set; }

        [Required]
        [Column("user_name")]
        [StringLength(256)]
        public string UserName { get; set; }

        [Required]
        [Column("action")]
        [StringLength(50)]
        public string Action { get; set; } // Create, Read, Update, Delete

        [Required]
        [Column("entity_name")]
        [StringLength(100)]
        public string EntityName { get; set; } // Alumni, Message, etc.

        [Column("entity_id")]
        [StringLength(50)]
        public string EntityId { get; set; }

        [Column("details")]
        public string Details { get; set; }

        [Column("ip_address")]
        [StringLength(45)]
        public string IpAddress { get; set; }

        [Column("timestamp", TypeName = "datetime")]
        public DateTime Timestamp { get; set; }

        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; }
    }
}

