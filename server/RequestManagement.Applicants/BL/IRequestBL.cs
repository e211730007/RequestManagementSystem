using RequestManagement.Application.DTO;
using RequestManagement.Application.Models;

namespace RequestManagement.Application.BL
{
    public interface IRequestBL
    {
        Task<(IEnumerable<RequestDTO> Items, int TotalRecords, RequestAggregationsDTO Aggregations)> GetPagedRequestsAsync(
             int pageNumber, int pageSize, string? search,
             RequestStatus? status, RequestPriority? priority,
             string? organizationName, string? assignedTo,
             DateTime? fromDate, DateTime? toDate,
             string sortBy, bool ascending, CancellationToken cancellationToken);
        Task<RequestDTO?> GetByIdAsync(int id, CancellationToken cancellationToken);
        Task<RequestDTO> CreateAsync(CreateRequestDTO dto, CancellationToken cancellationToken);
        Task UpdateAsync(int id, UpdateRequestDTO dto, string changedBy, CancellationToken cancellationToken);
        Task UpdateStatusWithConcurrencyAsync(int id, RequestStatus newStatus, byte[]? rowVersion, string changedBy, CancellationToken cancellationToken);
        Task DeleteAsync(int id, CancellationToken cancellationToken);
        Task<Dictionary<string, int>> GetCountByStatusAsync(CancellationToken cancellationToken);
        Task<Dictionary<string, int>> GetCountByPriorityAsync(CancellationToken cancellationToken);
        Task<IEnumerable<AuditLog>> GetAuditHistoryAsync(int requestId, CancellationToken cancellationToken);
        Task<BulkUpdateResultDTO> BulkUpdateStatusAsync(List<BulkUpdateItemDTO> items, RequestStatus newStatus, string changedBy, CancellationToken cancellationToken);
        Task UpdateStatusDirectAsync(int id, RequestStatus newStatus, string changedBy, CancellationToken cancellationToken);

    }
}