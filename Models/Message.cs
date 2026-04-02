using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Alumni_Management_System.Models;

public partial class Message
{
    [Key]
    [Column("message_id")]
    [Display(Name = "Message ID")]
    public int MessageId { get; set; }

    [Required]
    [Column("title")]
    [StringLength(150)]
    public string Title { get; set; }

    [Required]
    [Column("message_body")]
    [Display(Name = "Message Body")]
    public string MessageBody { get; set; }

    [Required]
    [Column("message_type")]
    [StringLength(50)]
    [Display(Name = "Message Type")]
    public string MessageType { get; set; }

    [Column("created_by")]
    [Display(Name = "Created By?")]
    public string CreatedBy { get; set; }

    [Column("created_at", TypeName = "datetime")]
    [Display(Name = "Created At?")]
    public DateTime CreatedAt { get; set; }

    [InverseProperty("Message")]
    public virtual ICollection<AlumniMessage> AlumniMessages { get; set; } = new List<AlumniMessage>();

    [ForeignKey("CreatedBy")]

    public virtual AppUser CreatedByNavigation { get; set; }
}
