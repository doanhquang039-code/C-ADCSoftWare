using WEBDULICH.Models;

namespace WEBDULICH.Services
{
    public interface IEmailMarketingService
    {
        // Campaign management
        Task<List<EmailCampaign>> GetCampaignsAsync();
        Task<EmailCampaign> GetCampaignByIdAsync(int id);
        Task<EmailCampaign> CreateCampaignAsync(EmailCampaign campaign);
        Task<EmailCampaign> UpdateCampaignAsync(EmailCampaign campaign);
        Task DeleteCampaignAsync(int id);
        Task<EmailCampaign> DuplicateCampaignAsync(int id);
        
        // Campaign execution
        Task<bool> SendCampaignAsync(int campaignId);
        Task<bool> ScheduleCampaignAsync(int campaignId, DateTime scheduledDate);
        Task<bool> CancelCampaignAsync(int campaignId);
        Task ProcessScheduledCampaignsAsync();
        
        // Templates
        Task<List<EmailTemplate>> GetTemplatesAsync();
        Task<EmailTemplate> GetTemplateByIdAsync(int id);
        Task<EmailTemplate> CreateTemplateAsync(EmailTemplate template);
        Task<EmailTemplate> UpdateTemplateAsync(EmailTemplate template);
        Task DeleteTemplateAsync(int id);
        
        // Subscribers
        Task<List<EmailSubscriber>> GetSubscribersAsync();
        Task<EmailSubscriber> GetSubscriberByEmailAsync(string email);
        Task<EmailSubscriber> SubscribeAsync(string email, string firstName = null, string lastName = null, string source = null);
        Task<bool> UnsubscribeAsync(string email);
        Task<bool> UpdateSubscriberPreferencesAsync(string email, string preferences);
        
        // Targeting and segmentation
        Task<List<string>> GetRecipientsAsync(CampaignTargetCriteria criteria);
        Task<int> GetRecipientCountAsync(CampaignTargetCriteria criteria);
        
        // Analytics and tracking
        Task<EmailStats> GetEmailStatsAsync();
        Task<CampaignPerformance> GetCampaignPerformanceAsync(int campaignId);
        Task TrackEmailOpenAsync(string trackingToken);
        Task TrackEmailClickAsync(string trackingToken, string url);
        
        // Automation
        Task SendWelcomeEmailAsync(string email, string firstName = null);
        Task SendBookingConfirmationAsync(int bookingId);
        Task SendBookingReminderAsync(int bookingId);
        Task SendPromotionalEmailAsync(string email, string promoCode);
    }
}