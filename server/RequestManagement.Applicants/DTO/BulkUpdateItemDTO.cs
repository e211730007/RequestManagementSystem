using RequestManagement.Application.Models;

namespace RequestManagement.Application.DTO
{
    public class BulkUpdateDTO
    {
        public List<BulkUpdateItemDTO> Requests { get; set; } = new();
        public RequestStatus NewStatus { get; set; }
    }

    public class BulkUpdateItemDTO
    {
        public int Id { get; set; }
        public byte[]? RowVersion { get; set; }
    }
}