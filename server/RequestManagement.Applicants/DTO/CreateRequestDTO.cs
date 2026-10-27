using System.ComponentModel.DataAnnotations;
using RequestManagement.Application.Models;

namespace RequestManagement.Application.DTO
{
    public class CreateRequestDTO
    {
        [Required(ErrorMessage = "כותרת היא שדה חובה")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "כותרת חייבת להיות בין 2 ל-200 תווים")]
        public string Title { get; set; } = string.Empty;
        [Required(ErrorMessage = "שם הארגון הוא שדה חובה")]
        public string OrganizationName { get; set; } = string.Empty;
        public RequestStatus Status { get; set; } = RequestStatus.Pending;
        public RequestPriority Priority { get; set; } = RequestPriority.Medium;
        [Required(ErrorMessage = "שם המטפל הוא שדה חובה")]
        public string AssignedTo { get; set; } = string.Empty;
    }
}