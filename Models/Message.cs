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

    [Required(ErrorMessage = "Title is required.")]
    [Column("title")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 150 characters.")]
    public string Title { get; set; }

    [Required(ErrorMessage = "Message body is required.")]
    [Column("message_body")]
    [StringLength(5000, MinimumLength = 1, ErrorMessage = "Message body cannot exceed 5000 characters.")]
    [Display(Name = "Message Body")]
    public string MessageBody { get; set; }

    [Required(ErrorMessage = "Message type is required.")]
    [Column("message_type")]
    [StringLength(50, ErrorMessage = "Message type cannot exceed 50 characters.")]
    [Display(Name = "Message Type")]
    public string MessageType { get; set; }

    [Column("created_by")]
    [StringLength(450, ErrorMessage = "Created by cannot exceed 450 characters.")]
    [Display(Name = "Created By?")]
    public string CreatedBy { get; set; }

    [Required(ErrorMessage = "Created at date is required.")]
    [Column("created_at", TypeName = "datetime")]
    [Display(Name = "Created At?")]
    public DateTime CreatedAt { get; set; }

    [InverseProperty("Message")]
    public virtual ICollection<AlumniMessage> AlumniMessages { get; set; } = new List<AlumniMessage>();

    [ForeignKey("CreatedBy")]
    public virtual AppUser CreatedByNavigation { get; set; }
}
