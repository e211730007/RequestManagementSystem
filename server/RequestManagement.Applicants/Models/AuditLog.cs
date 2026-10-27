namespace RequestManagement.Application.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public int RequestId { get; set; }
        public string PreviousStatus { get; set; } = string.Empty;
        public string NewStatus { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
        public string ChangedBy { get; set; } = string.Empty; // מזהה המשתמש המבצע
    }
}