using WEBDULICH.Models;

namespace WEBDULICH.Services.CustomerSegmentation
{
    public interface ICustomerSegmentationService
    {
        // Segment Management
        Task<CustomerSegment> CreateSegmentAsync(CustomerSegment segment);
        Task<CustomerSegment> GetSegmentByIdAsync(int id);
        Task<List<CustomerSegment>> GetAllSegmentsAsync();
        Task<CustomerSegment> UpdateSegmentAsync(CustomerSegment segment);
        Task<bool> DeleteSegmentAsync(int id);

        // Segment Analysis
        Task<List<CustomerSegment>> AnalyzeAndCreateSegmentsAsync();
        Task UpdateSegmentMembersAsync(int segmentId);
        Task<List<User>> GetSegmentMembersAsync(int segmentId);
        Task<CustomerSegment> GetUserPrimarySegmentAsync(int userId);

        // Customer Behavior
        Task<CustomerBehavior> GetCustomerBehaviorAsync(int userId);
        Task UpdateCustomerBehaviorAsync(int userId);
        Task<List<CustomerBehavior>> GetHighValueCustomersAsync(int count = 100);
        Task<List<CustomerBehavior>> GetChurnRiskCustomersAsync(decimal minRiskScore = 0.7m);

        // Insights
        Task<Dictionary<string, object>> GetSegmentInsightsAsync(int segmentId);
        Task<Dictionary<string, object>> GetOverallSegmentationInsightsAsync();
        Task<List<string>> GetMarketingRecommendationsAsync(int segmentId);
    }
}
