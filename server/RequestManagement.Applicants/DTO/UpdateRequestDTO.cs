using RequestManagement.Application.Models;

namespace RequestManagement.Application.DTO
{
    public class UpdateRequestDTO
    {
        public string Title { get; set; } = string.Empty;
        public string OrganizationName { get; set; } = string.Empty;
        public RequestStatus Status { get; set; }
        public RequestPriority Priority { get; set; }
        public string AssignedTo { get; set; } = string.Empty;
        public byte[]? RowVersion { get; set; }
    }
}