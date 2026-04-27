using System.ComponentModel.DataAnnotations;

namespace WEBDULICH.Models
{
    public class EmailCampaign
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; }
        
        [Required]
        public string Subject { get; set; }
        
        [Required]
        public string Content { get; set; }
        
        /// <summary>
        /// HTML template content
        /// </summary>
        public string HtmlContent { get; set; }
        
        /// <summary>
        /// "Draft", "Scheduled", "Sending", "Sent", "Cancelled"
        /// </summary>
        public string Status { get; set; } = "Draft";
        
        /// <summary>
        /// "Newsletter", "Promotion", "Welcome", "Booking_Confirmation", "Reminder"
        /// </summary>
        public string CampaignType { get; set; }
        
        public DateTime? ScheduledDate { get; set; }
        public DateTime? SentDate { get; set; }
        
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        
        /// <summary>
        /// Target audience criteria (JSON)
        /// </summary>
        public string TargetCriteria { get; set; }
        
        /// <summary>
        /// Total recipients
        /// </summary>
        public int TotalRecipients { get; set; }
        
        /// <summary>
        /// Successfully sent
        /// </summary>
        public int SentCount { get; set; }
        
        /// <summary>
        /// Failed to send
        /// </summary>
        public int FailedCount { get; set; }
        
        /// <summary>
        /// Email opened
        /// </summary>
        public int OpenedCount { get; set; }
        
        /// <summary>
        /// Links clicked
        /// </summary>
        public int ClickedCount { get; set; }
        
        /// <summary>
        /// Unsubscribed
        /// </summary>
        public int UnsubscribedCount { get; set; }

        public ICollection<EmailLog> EmailLogs { get; set; } = new List<EmailLog>();
    }

    public class EmailLog
    {
        public int Id { get; set; }
        
        public int CampaignId { get; set; }
        public EmailCampaign Campaign { get; set; }
        
        public int UserId { get; set; }
        public User User { get; set; }
        
        [Required]
        public string RecipientEmail { get; set; }
        
        /// <summary>
        /// "Sent", "Failed", "Opened", "Clicked", "Unsubscribed"
        /// </summary>
        public string Status { get; set; }
        
        public DateTime SentAt { get; set; }
        public DateTime? OpenedAt { get; set; }
        public DateTime? ClickedAt { get; set; }
        
        /// <summary>
        /// Error message if failed
        /// </summary>
        public string ErrorMessage { get; set; }
        
        /// <summary>
        /// Tracking token for opens/clicks
        /// </summary>
        public string TrackingToken { get; set; }
    }

    public class EmailTemplate
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        /// <summary>
        /// "Newsletter", "Promotion", "Welcome", "Booking_Confirmation", "Reminder"
        /// </summary>
        public string TemplateType { get; set; }
        
        [Required]
        public string Subject { get; set; }
        
        [Required]
        public string HtmlContent { get; set; }
        
        public string TextContent { get; set; }
        
        /// <summary>
        /// Template variables (JSON)
        /// </summary>
        public string Variables { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }

    public class EmailSubscriber
    {
        public int Id { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        public string FirstName { get; set; }
        public string LastName { get; set; }
        
        /// <summary>
        /// "Active", "Unsubscribed", "Bounced"
        /// </summary>
        public string Status { get; set; } = "Active";
        
        /// <summary>
        /// Subscription preferences (JSON)
        /// </summary>
        public string Preferences { get; set; }
        
        public DateTime SubscribedAt { get; set; } = DateTime.Now;
        public DateTime? UnsubscribedAt { get; set; }
        
        /// <summary>
        /// Source of subscription
        /// </summary>
        public string Source { get; set; }
        
        /// <summary>
        /// Tags for segmentation
        /// </summary>
        public string Tags { get; set; }
        
        public int? UserId { get; set; }
        public User User { get; set; }
    }

    public class CampaignTargetCriteria
    {
        public List<string> UserTypes { get; set; } = new(); // "Customer", "Subscriber"
        public List<string> Tags { get; set; } = new();
        public DateTime? RegisteredAfter { get; set; }
        public DateTime? RegisteredBefore { get; set; }
        public DateTime? LastBookingAfter { get; set; }
        public DateTime? LastBookingBefore { get; set; }
        public bool? HasBookings { get; set; }
        public List<int> DestinationIds { get; set; } = new();
        public List<string> Preferences { get; set; } = new();
    }

    public class EmailStats
    {
        public int TotalCampaigns { get; set; }
        public int ActiveSubscribers { get; set; }
        public int TotalEmailsSent { get; set; }
        public double AverageOpenRate { get; set; }
        public double AverageClickRate { get; set; }
        public double UnsubscribeRate { get; set; }
        public List<CampaignPerformance> RecentCampaigns { get; set; } = new();
    }

    public class CampaignPerformance
    {
        public int CampaignId { get; set; }
        public string CampaignName { get; set; }
        public DateTime SentDate { get; set; }
        public int Recipients { get; set; }
        public int Opens { get; set; }
        public int Clicks { get; set; }
        public double OpenRate { get; set; }
        public double ClickRate { get; set; }
    }
}