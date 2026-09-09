using System;

namespace Alumni_Management_System.Models.ViewModels;

public class AuditLogEntryViewModel
{
    public int AuditId { get; set; }
    public int RecordId { get; set; }
    public string ActionType { get; set; }
    public string ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; }
    public string OldValues { get; set; }
    public string NewValues { get; set; }
}
