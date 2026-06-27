using System;
using System.ComponentModel.DataAnnotations;

namespace ECommerce.Models
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? UserEmail { get; set; }
        public string Action { get; set; } = string.Empty; // Create, Update, Delete
        public string TargetTable { get; set; } = string.Empty;
        public string TargetId { get; set; } = string.Empty;
        public string? OldValues { get; set; } // JSON representation
        public string? NewValues { get; set; } // JSON representation
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
