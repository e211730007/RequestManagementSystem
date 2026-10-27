using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using RequestManagement.Application.BL;
using RequestManagement.Application.DAL;
using RequestManagement.Application.Data;
using RequestManagement.Application.DTO;
using RequestManagement.Application.Models;
using Xunit;

namespace RequestManagementAPI.Tests.BL
{
    public class RequestServiceTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task CreateAsync_ValidRequest_ReturnsCreatedDto()
        {
            var context = GetInMemoryDbContext();
            var dal = new RequestDAL(context);
            var cache = new MemoryCache(new MemoryCacheOptions());
            var logger = NullLogger<RequestBL>.Instance;
            var service = new RequestBL(dal, cache, logger);

            var dto = new CreateRequestDTO
            {
                Title = "Test Request",
                OrganizationName = "Test Org",
                Status = RequestStatus.Pending,
                Priority = RequestPriority.High,
                AssignedTo = "Developer"
            };

            var result = await service.CreateAsync(dto, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal("Test Request", result.Title);
            Assert.Equal("Pending", result.Status); // התאמה ל-Enum האמיתי
        }

        [Fact]
        public async Task UpdateAsync_NonExistingId_ThrowsKeyNotFoundException()
        {
            var context = GetInMemoryDbContext();
            var dal = new RequestDAL(context);
            var cache = new MemoryCache(new MemoryCacheOptions());
            var logger = NullLogger<RequestBL>.Instance;
            var service = new RequestBL(dal, cache, logger);

            var updateDto = new UpdateRequestDTO
            {
                Title = "Updated",
                OrganizationName = "Org",
                Status = RequestStatus.InProgress,
                Priority = RequestPriority.Medium,
                AssignedTo = "Dev"
            };

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                service.UpdateAsync(999, updateDto, "Tester", CancellationToken.None));
        }

        [Fact]
        public async Task GetCountByStatusAsync_ReturnsCorrectAggregations()
        {
            var context = GetInMemoryDbContext();
            context.Requests.Add(new Request { Title = "R1", Status = RequestStatus.Pending, Priority = RequestPriority.Low, OrganizationName = "A", AssignedTo = "X" });
            context.Requests.Add(new Request { Title = "R2", Status = RequestStatus.Pending, Priority = RequestPriority.High, OrganizationName = "B", AssignedTo = "Y" });
            context.Requests.Add(new Request { Title = "R3", Status = RequestStatus.Approved, Priority = RequestPriority.Medium, OrganizationName = "C", AssignedTo = "Z" });
            await context.SaveChangesAsync();

            var dal = new RequestDAL(context);
            var cache = new MemoryCache(new MemoryCacheOptions());
            var logger = NullLogger<RequestBL>.Instance;
            var service = new RequestBL(dal, cache, logger);
            var stats = await service.GetCountByStatusAsync(CancellationToken.None);

            Assert.NotNull(stats);
            Assert.Equal(2, stats["Pending"]);
            Assert.Equal(1, stats["Approved"]);
        }

        [Fact]
        public async Task UpdateStatus_InvalidStatusValue_ThrowsException()
        {
            var context = GetInMemoryDbContext();
            var request = new Request
            {
                Id = 1,
                Title = "Test Request",
                Status = RequestStatus.Pending,
                Priority = RequestPriority.Low,
                OrganizationName = "Org",
                AssignedTo = "Dev"
            };
            context.Requests.Add(request);
            await context.SaveChangesAsync();

            var dal = new RequestDAL(context);
            var cache = new MemoryCache(new MemoryCacheOptions());
            var logger = NullLogger<RequestBL>.Instance;
            var service = new RequestBL(dal, cache, logger);

            var updateDto = new UpdateRequestDTO
            {
                Title = "Updated Title",
                OrganizationName = "Org",
                Status = (RequestStatus)99, 
                Priority = RequestPriority.Low,
                AssignedTo = "Dev"
            };

            var exception = await Record.ExceptionAsync(() =>
                service.UpdateAsync(1, updateDto, "SystemUser", CancellationToken.None));

            Assert.NotNull(exception);
        }
    }
}