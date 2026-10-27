using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Xunit;

namespace RequestManagementAPI.Tests.Integration
{
    public class RequestsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public RequestsControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetRequests_Endpoint_ReturnsSuccessStatusCode()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/Requests");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrEmpty(content));
        }
      
        [Fact]
        public async Task GetRequestAudit_NonExistingId_ReturnsExpectedStatus()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/Requests/999999/audit");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        }
    }
}