using System.ComponentModel.DataAnnotations;

namespace RequestManagement.Application.Models
{
    public class Request
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string OrganizationName { get; set; } = string.Empty;
        public RequestStatus Status { get; set; }
        public RequestPriority Priority { get; set; }
        public string AssignedTo { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Timestamp]
        public byte[]? RowVersion { get; set; }
    }

    public enum RequestStatus
    {
        Pending = 0,     
        InProgress = 1,  
        Approved = 2,    
        Rejected = 3     
    }

    public enum RequestPriority
    {
        Low = 0,        
        Medium = 1,     
        High = 2,       
        Urgent = 3      
    }
}