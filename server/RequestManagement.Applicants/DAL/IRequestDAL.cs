using RequestManagement.Application.DTO;
using RequestManagement.Application.Models;

namespace RequestManagement.Application.DAL
{
    public interface IRequestDAL
    {
        Task<(IEnumerable<Request> Items, int TotalRecords)> GetPagedAsync(
            int pageNumber, int pageSize, string? search,
            RequestStatus? status, RequestPriority? priority,
            string? organizationName, string? assignedTo,
            DateTime? fromDate, DateTime? toDate,
            string sortBy, bool ascending, CancellationToken cancellationToken);

        Task<Request?> GetByIdAsync(int id, CancellationToken cancellationToken);
        Task<Request> AddAsync(Request request, CancellationToken cancellationToken);
        Task UpdateAsync(Request request, byte[]? clientRowVersion, string changedBy, CancellationToken cancellationToken);
        Task DeleteAsync(Request request, CancellationToken cancellationToken);
        Task<Dictionary<string, int>> GetCountByStatusAsync(CancellationToken cancellationToken);
        Task<Dictionary<string, int>> GetCountByPriorityAsync(CancellationToken cancellationToken);
        Task<IEnumerable<AuditLog>> GetAuditHistoryAsync(int requestId, CancellationToken cancellationToken);
        Task<BulkUpdateResultDTO> BulkUpdateStatusAsync(List<BulkUpdateItemDTO> items, RequestStatus newStatus, string changedBy, CancellationToken cancellationToken);
        Task UpdateSingleStatusDirectAsync(int id, RequestStatus newStatus, string changedBy, CancellationToken cancellationToken);
        Task<RequestAggregationsDTO> GetAggregationsAsync(CancellationToken cancellationToken);
    }
}