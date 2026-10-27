namespace RequestManagement.Application.DTO
{
    public class BulkUpdateResultDTO
    {
        public List<int> SuccessfulIds { get; set; } = new();
        public Dictionary<int, string> FailedIdsWithReasons { get; set; } = new();
    }
}