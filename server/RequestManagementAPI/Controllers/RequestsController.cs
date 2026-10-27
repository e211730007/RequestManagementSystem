using Microsoft.AspNetCore.Mvc;
using RequestManagement.Application.BL;
using RequestManagement.Application.Models;
using RequestManagement.Application.DTO;

namespace RequestManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestsController : ControllerBase
    {
        private readonly IRequestBL _requestBL;

        public RequestsController(IRequestBL requestBL)
        {
            _requestBL = requestBL;
        }

        [HttpGet]
        public async Task<IActionResult> GetRequests(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] RequestStatus? status = null,
            [FromQuery] RequestPriority? priority = null,
            [FromQuery] string? organizationName = null,
            [FromQuery] string? assignedTo = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] string sortBy = "CreatedAt",
            [FromQuery] bool ascending = false,
            CancellationToken cancellationToken = default)
        {
            var (items, totalRecords, aggregations) = await _requestBL.GetPagedRequestsAsync(
                pageNumber, pageSize, search, status, priority,
                organizationName, assignedTo, fromDate, toDate,
                sortBy, ascending, cancellationToken);

            return Ok(new
            {
                TotalRecords = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize),
                Items = items,

                Aggregations = aggregations
            });
        }

            // GET: api/requests/{id}
            [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var request = await _requestBL.GetByIdAsync(id, cancellationToken);
            if (request == null) return NotFound(new { message = $"Request with ID {id} not found." });
            return Ok(request);
        }

        // POST: api/requests
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRequestDTO dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var created = await _requestBL.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // PUT: api/requests/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateRequestDTO dto, [FromQuery] string changedBy = "SystemUser", CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                await _requestBL.UpdateAsync(id, dto, changedBy, cancellationToken);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = $"Request with ID {id} not found." });
            }
            catch (InvalidOperationException ex) when (ex.Message == "ConcurrencyConflict")
            {
                return StatusCode(StatusCodes.Status409Conflict, new { message = "The request has been modified by another user. Please reload and try again." });
            }
        }
        // PUT: api/requests/{id}/status 
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDto dto, [FromQuery] string changedBy = "SystemUser", CancellationToken cancellationToken = default)
        {
            if (dto == null)
            {
                return BadRequest(new { message = "Invalid request body." });
            }

            try
            {
                await _requestBL.UpdateStatusWithConcurrencyAsync(id, dto.Status, dto.RowVersion, changedBy, cancellationToken);

                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = $"Request with ID {id} not found." });
            }
            catch (InvalidOperationException ex) when (ex.Message == "ConcurrencyConflict")
            {
                return StatusCode(StatusCodes.Status409Conflict, new { message = "The request has been modified by another user. Please reload and try again." });
            }
        }

        // DELETE: api/requests/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            try
            {
                await _requestBL.DeleteAsync(id, cancellationToken);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = $"Request with ID {id} not found." });
            }
        }

        // GET: api/requests/aggregations/status
        [HttpGet("aggregations/status")]
        public async Task<IActionResult> GetCountByStatus(CancellationToken cancellationToken)
        {
            var stats = await _requestBL.GetCountByStatusAsync(cancellationToken);
            return Ok(stats);
        }

        // GET: api/requests/aggregations/priority
        [HttpGet("aggregations/priority")]
        public async Task<IActionResult> GetCountByPriority(CancellationToken cancellationToken)
        {
            var stats = await _requestBL.GetCountByPriorityAsync(cancellationToken);
            return Ok(stats);
        }

        // GET: api/requests/{id}/audit
        [HttpGet("{id}/audit")]
        public async Task<IActionResult> GetAuditHistory(int id, CancellationToken cancellationToken)
        {
            var history = await _requestBL.GetAuditHistoryAsync(id, cancellationToken);
            return Ok(history);
        }

        // POST: api/requests/bulk
        [HttpPost("bulk")]
        public async Task<IActionResult> BulkUpdate([FromBody] BulkUpdateDTO bulkDto, [FromQuery] string changedBy = "SystemUser", CancellationToken cancellationToken = default)
        {
            if (bulkDto == null || bulkDto.Requests == null || bulkDto.Requests.Count == 0 || bulkDto.Requests.Count > 100)
            {
                return BadRequest(new { message = "Bulk update requires between 1 and 100 items." });
            }

            // העברת הרשימה המלאה (כולל ה-RowVersion של כל פנייה) ל-BL
            var result = await _requestBL.BulkUpdateStatusAsync(bulkDto.Requests, bulkDto.NewStatus, changedBy, cancellationToken);

            return Ok(result);
        }
    }

    // DTOs מקומיים לקונטרולר
    public class BulkUpdateDto
    {
        public List<int> RequestIds { get; set; } = new();
        public RequestStatus NewStatus { get; set; }
    }

    public class UpdateStatusDto
    {
        public RequestStatus Status { get; set; }
        public byte[]? RowVersion { get; set; }
    }


}