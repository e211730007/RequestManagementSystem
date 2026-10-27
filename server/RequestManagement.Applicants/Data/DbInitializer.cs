using Microsoft.EntityFrameworkCore;
using RequestManagement.Application;
using RequestManagement.Application.Models;

namespace RequestManagement.Application.Data
{
    public static class DbInitializer
    {
        public static void SeedData(AppDbContext context)
        {
            if (context.Requests.Any())
            {
                return;
            }

            var random = new Random();
            var orgs = new[] { "עיריית ירושלים", "משרד הפנים", "משרד הבינוי והשיכון", "רשות מינהל מקרקעי ישראל", "הוועדה המחוזית" };
            var assignees = new[] { "ישראל ישראלי", "שרה כהן", "דוד לוי", "יוסי אברהמי" };

            int totalRecords = 100000;
            int batchSize = 10000;

            for (int i = 0; i < totalRecords; i += batchSize)
            {
                var requests = new List<Request>(batchSize);
                int currentBatchSize = Math.Min(batchSize, totalRecords - i);

                for (int j = 0; j < currentBatchSize; j++)
                {
                    requests.Add(new Request
                    {
                        Title = $"בקשה מס' {i + j + 1}: טיפול בתיק תכנון ובנייה",
                        OrganizationName = orgs[random.Next(orgs.Length)],
                        Status = (RequestStatus)random.Next(0, 4),
                        Priority = (RequestPriority)random.Next(0, 4),
                        AssignedTo = assignees[random.Next(assignees.Length)],
                        CreatedAt = DateTime.UtcNow.AddDays(-random.Next(365)),
                        UpdatedAt = DateTime.UtcNow
                    });
                }

                context.Requests.AddRange(requests);
                context.SaveChanges();
                context.ChangeTracker.Clear();
            }
        }
    }
}