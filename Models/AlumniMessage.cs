using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Alumni_Management_System.Models;

[Table("Alumni_Messages")]
public partial class AlumniMessage
{
    [Key]
    [Column("alumni_message_id")]
    [Display(Name = "Alumni Message ID")]
    public int AlumniMessageId { get; set; }

    [Column("alumni_id")]
    [Display(Name = "Alumni ID")]
    public int AlumniId { get; set; }

    [Column("message_id")]
    [Display(Name = "Message ID")]
    public int MessageId { get; set; }

    [Column("sent_at", TypeName = "datetime")]
    [Display(Name = "Sent At?")]
    public DateTime SentAt { get; set; }

    [ForeignKey("AlumniId")]
    [InverseProperty("AlumniMessages")]
    public virtual Alumni Alumni { get; set; }

    [ForeignKey("MessageId")]
    [InverseProperty("AlumniMessages")]
    public virtual Message Message { get; set; }
}
