using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using RequestManagement.Application.DAL;
using RequestManagement.Application.DTO;
using RequestManagement.Application.Models;

namespace RequestManagement.Application.BL
{
    public class RequestBL : IRequestBL
    {
        private readonly IRequestDAL _requestDAL;
        private readonly IMemoryCache _memoryCache;
        private const string StatusCacheKey = "Cache_CountByStatus";
        private const string PriorityCacheKey = "Cache_CountByPriority";
        private readonly ILogger<RequestBL> _logger;

        public RequestBL(IRequestDAL requestDAL, IMemoryCache memoryCache, ILogger<RequestBL> logger)
        {
            _requestDAL = requestDAL;
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public async Task<(IEnumerable<RequestDTO> Items, int TotalRecords, RequestAggregationsDTO Aggregations)> GetPagedRequestsAsync(
             int pageNumber, int pageSize, string? search,
             RequestStatus? status, RequestPriority? priority,
             string? organizationName, string? assignedTo,
             DateTime? fromDate, DateTime? toDate,
             string sortBy, bool ascending, CancellationToken cancellationToken)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var (items, totalRecords) = await _requestDAL.GetPagedAsync(
                    pageNumber, pageSize, search, status, priority,
                    organizationName, assignedTo, fromDate, toDate,
                    sortBy, ascending, cancellationToken);

                var dtos = items.Select(r => new RequestDTO
                {
                    Id = r.Id,
                    Title = r.Title,
                    OrganizationName = r.OrganizationName,
                    Status = r.Status.ToString(),
                    Priority = r.Priority.ToString(),
                    AssignedTo = r.AssignedTo,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    RowVersion = r.RowVersion
                }).ToList();

                var aggregations = await _requestDAL.GetAggregationsAsync(cancellationToken);

                return (dtos, totalRecords, aggregations);
            }
            catch (OperationCanceledException)
            {
                return (Enumerable.Empty<RequestDTO>(), 0, new RequestAggregationsDTO());
            }
        }

        public async Task<RequestDTO?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            var r = await _requestDAL.GetByIdAsync(id, cancellationToken);
            if (r == null) return null;

            return new RequestDTO
            {
                Id = r.Id,
                Title = r.Title,
                OrganizationName = r.OrganizationName,
                Status = r.Status.ToString(),
                Priority = r.Priority.ToString(),
                AssignedTo = r.AssignedTo,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                RowVersion = r.RowVersion 
            };
        }

        private static RequestDTO MapToDto(Request r)
        {
            return new RequestDTO
            {
                Id = r.Id,
                Title = r.Title,
                OrganizationName = r.OrganizationName,
                Status = r.Status.ToString(),
                Priority = r.Priority.ToString(),
                AssignedTo = r.AssignedTo,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                RowVersion = r.RowVersion 
            };
        }

        public async Task<RequestDTO> CreateAsync(CreateRequestDTO dto, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating a new request for organization: {Organization}", dto.OrganizationName);

            var request = new Request
            {
                Title = dto.Title,
                OrganizationName = dto.OrganizationName,
                Status = dto.Status,
                Priority = dto.Priority,
                AssignedTo = dto.AssignedTo
            };

            var created = await _requestDAL.AddAsync(request, cancellationToken);
            InvalidateCache();

            _logger.LogInformation("Successfully created request with ID: {RequestId}", created.Id);

            return MapToDto(created);
        }

        public async Task UpdateAsync(int id, UpdateRequestDTO dto, string changedBy, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Attempting to update request ID: {RequestId} by user: {User}", id, changedBy);

            var existing = await _requestDAL.GetByIdAsync(id, cancellationToken);
            if (existing == null)
            {
                _logger.LogWarning("Update failed: Request ID {RequestId} not found.", id);
                throw new KeyNotFoundException($"Request with ID {id} was not found.");
            }

            existing.Title = dto.Title;
            existing.OrganizationName = dto.OrganizationName;
            existing.Status = dto.Status;
            existing.Priority = dto.Priority;
            existing.AssignedTo = dto.AssignedTo;

            if (dto.RowVersion != null)
            {
                existing.RowVersion = dto.RowVersion;
            }

            try
            {
                await _requestDAL.UpdateAsync(existing, existing.RowVersion, changedBy, cancellationToken);
                InvalidateCache();
                _logger.LogInformation("Successfully updated request ID: {RequestId}", id);
            }
            catch (InvalidOperationException ex) when (ex.Message == "ConcurrencyConflict")
            {
                _logger.LogWarning("Concurrency conflict detected while updating request ID: {RequestId}", id);
                throw;
            }
        }

        public async Task UpdateStatusWithConcurrencyAsync(int id, RequestStatus newStatus, byte[]? rowVersion, string changedBy, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Attempting to update status for request ID: {RequestId} to {NewStatus}", id, newStatus);

            var existing = await _requestDAL.GetByIdAsync(id, cancellationToken);
            if (existing == null)
            {
                _logger.LogWarning("Status update failed: Request ID {RequestId} not found.", id);
                throw new KeyNotFoundException($"Request with ID {id} was not found.");
            }

            existing.Status = newStatus;
            if (rowVersion != null)
            {
                existing.RowVersion = rowVersion;
            }

            try
            {
                await _requestDAL.UpdateAsync(existing, existing.RowVersion, changedBy, cancellationToken);
                InvalidateCache();
                _logger.LogInformation("Successfully updated status for request ID: {RequestId}", id);
            }
            catch (InvalidOperationException ex) when (ex.Message == "ConcurrencyConflict")
            {
                _logger.LogWarning("Concurrency conflict detected while updating status for request ID: {RequestId}", id);
                throw;
            }
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken)
        {
            var existing = await _requestDAL.GetByIdAsync(id, cancellationToken);
            if (existing == null)
            {
                throw new KeyNotFoundException("Request not found.");
            }

            await _requestDAL.DeleteAsync(existing, cancellationToken);
            InvalidateCache();
        }

        public async Task<Dictionary<string, int>> GetCountByStatusAsync(CancellationToken cancellationToken)
        {
            return await _memoryCache.GetOrCreateAsync(StatusCacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                return await _requestDAL.GetCountByStatusAsync(cancellationToken);
            }) ?? new Dictionary<string, int>();
        }

        public async Task<Dictionary<string, int>> GetCountByPriorityAsync(CancellationToken cancellationToken)
        {
            return await _memoryCache.GetOrCreateAsync(PriorityCacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                return await _requestDAL.GetCountByPriorityAsync(cancellationToken);
            }) ?? new Dictionary<string, int>();
        }

        public async Task<IEnumerable<AuditLog>> GetAuditHistoryAsync(int requestId, CancellationToken cancellationToken)
        {
            return await _requestDAL.GetAuditHistoryAsync(requestId, cancellationToken);
        }

        public async Task<BulkUpdateResultDTO> BulkUpdateStatusAsync(List<BulkUpdateItemDTO> items, RequestStatus newStatus, string changedBy, CancellationToken cancellationToken)
        {
            var result = await _requestDAL.BulkUpdateStatusAsync(items, newStatus, changedBy, cancellationToken);

            if (result.SuccessfulIds != null && result.SuccessfulIds.Count > 0)
            {
                InvalidateCache();
                _logger.LogInformation("Bulk status update completed successfully for {Count} requests.", result.SuccessfulIds.Count);
            }

            return result;
        }

        public async Task UpdateStatusDirectAsync(int id, RequestStatus newStatus, string changedBy, CancellationToken cancellationToken)
        {
            await _requestDAL.UpdateSingleStatusDirectAsync(id, newStatus, changedBy, cancellationToken);
            InvalidateCache();
        }

        private void InvalidateCache()
        {
            _memoryCache.Remove(StatusCacheKey);
            _memoryCache.Remove(PriorityCacheKey);
        }
    }
}