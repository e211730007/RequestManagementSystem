using Microsoft.EntityFrameworkCore;
using RequestManagement.Application.Data;
using RequestManagement.Application.DTO;
using RequestManagement.Application.Models;

namespace RequestManagement.Application.DAL
{
    public class RequestDAL : IRequestDAL
    {
        private readonly AppDbContext _context;

        public RequestDAL(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<Request> Items, int TotalRecords)> GetPagedAsync(
            int pageNumber, int pageSize, string? search,
            RequestStatus? status, RequestPriority? priority,
            string? organizationName, string? assignedTo,
            DateTime? fromDate, DateTime? toDate,
            string sortBy, bool ascending, CancellationToken cancellationToken)
        {
            var query = _context.Requests
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(r => r.Title.Contains(search) || r.OrganizationName.Contains(search));
            }

            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
            }

            if (priority.HasValue)
            {
                query = query.Where(r => r.Priority == priority.Value);
            }

            if (!string.IsNullOrWhiteSpace(organizationName))
            {
                query = query.Where(r => r.OrganizationName.Contains(organizationName));
            }

            if (!string.IsNullOrWhiteSpace(assignedTo))
            {
                query = query.Where(r => r.AssignedTo.Contains(assignedTo));
            }

            if (fromDate.HasValue) query = query.Where(r => r.CreatedAt >= fromDate.Value);
            if (toDate.HasValue) query = query.Where(r => r.CreatedAt <= toDate.Value);

            query = sortBy?.ToLower() switch
            {
                "priority" => ascending ? query.OrderBy(r => r.Priority) : query.OrderByDescending(r => r.Priority),
                "title" => ascending ? query.OrderBy(r => r.Title) : query.OrderByDescending(r => r.Title),
                "organizationname" => ascending ? query.OrderBy(r => r.OrganizationName) : query.OrderByDescending(r => r.OrganizationName),
                "status" => ascending ? query.OrderBy(r => r.Status) : query.OrderByDescending(r => r.Status),
                _ => ascending ? query.OrderBy(r => r.CreatedAt) : query.OrderByDescending(r => r.CreatedAt)
            };

            var totalRecords = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalRecords);
        }

        public async Task<Request?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return await _context.Requests
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        }

        public async Task<Request> AddAsync(Request request, CancellationToken cancellationToken)
        {
            request.CreatedAt = DateTime.UtcNow;
            request.UpdatedAt = DateTime.UtcNow;
            _context.Requests.Add(request);
            await _context.SaveChangesAsync(cancellationToken);
            return request;
        }

        public async Task UpdateAsync(Request request, byte[]? clientRowVersion, string changedBy, CancellationToken cancellationToken)
        {
            var originalStatus = await _context.Requests
                .Where(r => r.Id == request.Id)
                .Select(r => r.Status)
                .FirstOrDefaultAsync(cancellationToken);

            request.UpdatedAt = DateTime.UtcNow;
            _context.Requests.Update(request);

            if (clientRowVersion != null && clientRowVersion.Length > 0)
            {
                _context.Entry(request).Property(r => r.RowVersion).OriginalValue = clientRowVersion;
            }

            try
            {
                if (originalStatus != request.Status)
                {
                    var audit = new AuditLog
                    {
                        RequestId = request.Id,
                        PreviousStatus = originalStatus.ToString(),
                        NewStatus = request.Status.ToString(),
                        ChangedAt = DateTime.UtcNow,
                        ChangedBy = changedBy
                    };
                    _context.AuditLogs.Add(audit);
                }

                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new InvalidOperationException("ConcurrencyConflict");
            }
        }

        public async Task DeleteAsync(Request request, CancellationToken cancellationToken)
        {
            _context.Requests.Remove(request);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Dictionary<string, int>> GetCountByStatusAsync(CancellationToken cancellationToken)
        {
            var rawData = await _context.Requests
                .AsNoTracking()
                .GroupBy(r => r.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            return rawData.ToDictionary(x => x.Status.ToString(), x => x.Count);
        }

        public async Task<Dictionary<string, int>> GetCountByPriorityAsync(CancellationToken cancellationToken)
        {
            var rawData = await _context.Requests
                .AsNoTracking()
                .GroupBy(r => r.Priority)
                .Select(g => new { Priority = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            return rawData.ToDictionary(x => x.Priority.ToString(), x => x.Count);
        }

        public async Task<IEnumerable<AuditLog>> GetAuditHistoryAsync(int requestId, CancellationToken cancellationToken)
        {
            return await _context.AuditLogs
                .AsNoTracking()
                .Where(a => a.RequestId == requestId)
                .OrderByDescending(a => a.ChangedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<BulkUpdateResultDTO> BulkUpdateStatusAsync(List<BulkUpdateItemDTO> items, RequestStatus newStatus, string changedBy, CancellationToken cancellationToken)
        {
            if (!Enum.IsDefined(typeof(RequestStatus), newStatus))
            {
                throw new ArgumentException("Invalid status value.");
            }
            var result = new BulkUpdateResultDTO();

            if (items == null || items.Count == 0 || items.Count > 100)
            {
                throw new ArgumentException("Bulk operation requires between 1 and 100 items.");
            }

            var requestIds = items.Select(i => i.Id).ToList();

            var requests = await _context.Requests
                .Where(r => requestIds.Contains(r.Id))
                .ToListAsync(cancellationToken);

            var foundIds = requests.Select(r => r.Id).ToList();
            var missingIds = requestIds.Except(foundIds).ToList();

            foreach (var missingId in missingIds)
            {
                result.FailedIdsWithReasons[missingId] = "Request not found.";
            }

            foreach (var item in items.Where(i => foundIds.Contains(i.Id)))
            {
                var req = requests.First(r => r.Id == item.Id);

                if (item.RowVersion != null)
                {
                    _context.Entry(req).Property(r => r.RowVersion).OriginalValue = item.RowVersion;
                }

                var originalStatus = req.Status;
                req.Status = newStatus;
                req.UpdatedAt = DateTime.UtcNow;

                try
                {
                    if (originalStatus != newStatus)
                    {
                        var audit = new AuditLog
                        {
                            RequestId = req.Id,
                            PreviousStatus = originalStatus.ToString(),
                            NewStatus = newStatus.ToString(),
                            ChangedAt = DateTime.UtcNow,
                            ChangedBy = changedBy
                        };
                        _context.AuditLogs.Add(audit);
                    }

                    await _context.SaveChangesAsync(cancellationToken);
                    result.SuccessfulIds.Add(req.Id);
                }
                catch (DbUpdateConcurrencyException)
                {
                    result.FailedIdsWithReasons[req.Id] = "Concurrency conflict detected.";
                    _context.Entry(req).State = EntityState.Detached;
                }
                catch (Exception ex)
                {
                    result.FailedIdsWithReasons[req.Id] = ex.Message;
                    _context.Entry(req).State = EntityState.Detached;
                }
            }

            return result;
        }

        public async Task UpdateSingleStatusDirectAsync(int id, RequestStatus newStatus, string changedBy, CancellationToken cancellationToken)
        {
            if (!Enum.IsDefined(typeof(RequestStatus), newStatus))
            {
                throw new ArgumentException("Invalid status value.");
            }
            var req = await _context.Requests.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
            if (req == null)
            {
                throw new KeyNotFoundException($"Request with ID {id} was not found.");
            }

            var originalStatus = req.Status;
            if (originalStatus != newStatus)
            {
                req.Status = newStatus;
                req.UpdatedAt = DateTime.UtcNow;

                var audit = new AuditLog
                {
                    RequestId = req.Id,
                    PreviousStatus = originalStatus.ToString(),
                    NewStatus = newStatus.ToString(),
                    ChangedAt = DateTime.UtcNow,
                    ChangedBy = changedBy
                };
                _context.AuditLogs.Add(audit);

                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<RequestAggregationsDTO> GetAggregationsAsync(CancellationToken cancellationToken)
        {
            return new RequestAggregationsDTO
            {
                PendingCount = await _context.Requests.CountAsync(r => r.Status == RequestStatus.Pending, cancellationToken),
                InProgressCount = await _context.Requests.CountAsync(r => r.Status == RequestStatus.InProgress, cancellationToken),
                HighPriorityCount = await _context.Requests.CountAsync(r => r.Priority == RequestPriority.High, cancellationToken)
            };
        }
    }
}