using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net;
using System.Net.Mail;
using WEBDULICH.Models;

namespace WEBDULICH.Services
{
    public class EmailMarketingService : IEmailMarketingService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ICurrentUserService _currentUserService;

        public EmailMarketingService(ApplicationDbContext context, IConfiguration configuration, ICurrentUserService currentUserService)
        {
            _context = context;
            _configuration = configuration;
            _currentUserService = currentUserService;
        }

        #region Campaign Management

        public async Task<List<EmailCampaign>> GetCampaignsAsync()
        {
            return await _context.EmailCampaigns
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<EmailCampaign> GetCampaignByIdAsync(int id)
        {
            return await _context.EmailCampaigns
                .Include(c => c.EmailLogs)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<EmailCampaign> CreateCampaignAsync(EmailCampaign campaign)
        {
            var currentUser = _currentUserService.GetCurrentUser();
            campaign.CreatedBy = currentUser?.Id ?? 0;
            campaign.CreatedAt = DateTime.Now;
            
            _context.EmailCampaigns.Add(campaign);
            await _context.SaveChangesAsync();
            return campaign;
        }

        public async Task<EmailCampaign> UpdateCampaignAsync(EmailCampaign campaign)
        {
            campaign.UpdatedAt = DateTime.Now;
            _context.EmailCampaigns.Update(campaign);
            await _context.SaveChangesAsync();
            return campaign;
        }

        public async Task DeleteCampaignAsync(int id)
        {
            var campaign = await _context.EmailCampaigns.FindAsync(id);
            if (campaign != null && campaign.Status == "Draft")
            {
                _context.EmailCampaigns.Remove(campaign);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<EmailCampaign> DuplicateCampaignAsync(int id)
        {
            var original = await GetCampaignByIdAsync(id);
            if (original == null) return null;

            var duplicate = new EmailCampaign
            {
                Name = $"{original.Name} (Copy)",
                Subject = original.Subject,
                Content = original.Content,
                HtmlContent = original.HtmlContent,
                CampaignType = original.CampaignType,
                TargetCriteria = original.TargetCriteria,
                Status = "Draft"
            };

            return await CreateCampaignAsync(duplicate);
        }

        #endregion

        #region Campaign Execution

        public async Task<bool> SendCampaignAsync(int campaignId)
        {
            var campaign = await GetCampaignByIdAsync(campaignId);
            if (campaign == null || campaign.Status != "Draft") return false;

            try
            {
                campaign.Status = "Sending";
                await UpdateCampaignAsync(campaign);

                // Get recipients
                var criteria = string.IsNullOrEmpty(campaign.TargetCriteria) 
                    ? new CampaignTargetCriteria() 
                    : JsonConvert.DeserializeObject<CampaignTargetCriteria>(campaign.TargetCriteria);
                
                var recipients = await GetRecipientsAsync(criteria);
                campaign.TotalRecipients = recipients.Count;

                // Send emails
                var sentCount = 0;
                var failedCount = 0;

                foreach (var email in recipients)
                {
                    try
                    {
                        var trackingToken = Guid.NewGuid().ToString();
                        var personalizedContent = PersonalizeContent(campaign.HtmlContent ?? campaign.Content, email);
                        
                        await SendEmailAsync(email, campaign.Subject, personalizedContent, trackingToken);
                        
                        // Log success
                        var log = new EmailLog
                        {
                            CampaignId = campaignId,
                            RecipientEmail = email,
                            Status = "Sent",
                            SentAt = DateTime.Now,
                            TrackingToken = trackingToken
                        };
                        _context.EmailLogs.Add(log);
                        sentCount++;
                    }
                    catch (Exception ex)
                    {
                        // Log failure
                        var log = new EmailLog
                        {
                            CampaignId = campaignId,
                            RecipientEmail = email,
                            Status = "Failed",
                            SentAt = DateTime.Now,
                            ErrorMessage = ex.Message
                        };
                        _context.EmailLogs.Add(log);
                        failedCount++;
                    }
                }

                // Update campaign
                campaign.Status = "Sent";
                campaign.SentDate = DateTime.Now;
                campaign.SentCount = sentCount;
                campaign.FailedCount = failedCount;
                
                await UpdateCampaignAsync(campaign);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {
                campaign.Status = "Draft";
                await UpdateCampaignAsync(campaign);
                return false;
            }
        }

        public async Task<bool> ScheduleCampaignAsync(int campaignId, DateTime scheduledDate)
        {
            var campaign = await GetCampaignByIdAsync(campaignId);
            if (campaign == null || campaign.Status != "Draft") return false;

            campaign.Status = "Scheduled";
            campaign.ScheduledDate = scheduledDate;
            await UpdateCampaignAsync(campaign);

            return true;
        }

        public async Task<bool> CancelCampaignAsync(int campaignId)
        {
            var campaign = await GetCampaignByIdAsync(campaignId);
            if (campaign == null || campaign.Status != "Scheduled") return false;

            campaign.Status = "Cancelled";
            await UpdateCampaignAsync(campaign);

            return true;
        }

        public async Task ProcessScheduledCampaignsAsync()
        {
            var scheduledCampaigns = await _context.EmailCampaigns
                .Where(c => c.Status == "Scheduled" && c.ScheduledDate <= DateTime.Now)
                .ToListAsync();

            foreach (var campaign in scheduledCampaigns)
            {
                await SendCampaignAsync(campaign.Id);
            }
        }

        #endregion

        #region Templates

        public async Task<List<EmailTemplate>> GetTemplatesAsync()
        {
            return await _context.EmailTemplates
                .Where(t => t.IsActive)
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<EmailTemplate> GetTemplateByIdAsync(int id)
        {
            return await _context.EmailTemplates.FindAsync(id);
        }

        public async Task<EmailTemplate> CreateTemplateAsync(EmailTemplate template)
        {
            template.CreatedAt = DateTime.Now;
            _context.EmailTemplates.Add(template);
            await _context.SaveChangesAsync();
            return template;
        }

        public async Task<EmailTemplate> UpdateTemplateAsync(EmailTemplate template)
        {
            template.UpdatedAt = DateTime.Now;
            _context.EmailTemplates.Update(template);
            await _context.SaveChangesAsync();
            return template;
        }

        public async Task DeleteTemplateAsync(int id)
        {
            var template = await _context.EmailTemplates.FindAsync(id);
            if (template != null)
            {
                template.IsActive = false;
                await _context.SaveChangesAsync();
            }
        }

        #endregion

        #region Subscribers

        public async Task<List<EmailSubscriber>> GetSubscribersAsync()
        {
            return await _context.EmailSubscribers
                .Where(s => s.Status == "Active")
                .OrderBy(s => s.Email)
                .ToListAsync();
        }

        public async Task<EmailSubscriber> GetSubscriberByEmailAsync(string email)
        {
            return await _context.EmailSubscribers
                .FirstOrDefaultAsync(s => s.Email == email);
        }

        public async Task<EmailSubscriber> SubscribeAsync(string email, string firstName = null, string lastName = null, string source = null)
        {
            var existing = await GetSubscriberByEmailAsync(email);
            if (existing != null)
            {
                if (existing.Status == "Unsubscribed")
                {
                    existing.Status = "Active";
                    existing.SubscribedAt = DateTime.Now;
                    await _context.SaveChangesAsync();
                }
                return existing;
            }

            var subscriber = new EmailSubscriber
            {
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                Source = source ?? "Website",
                Status = "Active"
            };

            _context.EmailSubscribers.Add(subscriber);
            await _context.SaveChangesAsync();

            // Send welcome email
            await SendWelcomeEmailAsync(email, firstName);

            return subscriber;
        }

        public async Task<bool> UnsubscribeAsync(string email)
        {
            var subscriber = await GetSubscriberByEmailAsync(email);
            if (subscriber != null)
            {
                subscriber.Status = "Unsubscribed";
                subscriber.UnsubscribedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> UpdateSubscriberPreferencesAsync(string email, string preferences)
        {
            var subscriber = await GetSubscriberByEmailAsync(email);
            if (subscriber != null)
            {
                subscriber.Preferences = preferences;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        #endregion

        #region Targeting and Segmentation

        public async Task<List<string>> GetRecipientsAsync(CampaignTargetCriteria criteria)
        {
            var emails = new List<string>();

            // Get from subscribers
            if (criteria.UserTypes.Contains("Subscriber") || !criteria.UserTypes.Any())
            {
                var subscriberQuery = _context.EmailSubscribers.Where(s => s.Status == "Active");
                
                if (criteria.Tags.Any())
                {
                    subscriberQuery = subscriberQuery.Where(s => criteria.Tags.Any(tag => s.Tags.Contains(tag)));
                }

                var subscriberEmails = await subscriberQuery.Select(s => s.Email).ToListAsync();
                emails.AddRange(subscriberEmails);
            }

            // Get from users
            if (criteria.UserTypes.Contains("Customer") || !criteria.UserTypes.Any())
            {
                var userQuery = _context.Users.AsQueryable();

                if (criteria.RegisteredAfter.HasValue)
                {
                    userQuery = userQuery.Where(u => u.CreatedAt >= criteria.RegisteredAfter.Value);
                }

                if (criteria.RegisteredBefore.HasValue)
                {
                    userQuery = userQuery.Where(u => u.CreatedAt <= criteria.RegisteredBefore.Value);
                }

                if (criteria.HasBookings.HasValue)
                {
                    if (criteria.HasBookings.Value)
                    {
                        userQuery = userQuery.Where(u => _context.Bookings.Any(b => b.UserId == u.Id));
                    }
                    else
                    {
                        userQuery = userQuery.Where(u => !_context.Bookings.Any(b => b.UserId == u.Id));
                    }
                }

                var userEmails = await userQuery.Select(u => u.Email).ToListAsync();
                emails.AddRange(userEmails);
            }

            return emails.Distinct().ToList();
        }

        public async Task<int> GetRecipientCountAsync(CampaignTargetCriteria criteria)
        {
            var recipients = await GetRecipientsAsync(criteria);
            return recipients.Count;
        }

        #endregion

        #region Analytics and Tracking

        public async Task<EmailStats> GetEmailStatsAsync()
        {
            var totalCampaigns = await _context.EmailCampaigns.CountAsync();
            var activeSubscribers = await _context.EmailSubscribers.CountAsync(s => s.Status == "Active");
            var totalEmailsSent = await _context.EmailLogs.CountAsync(l => l.Status == "Sent");
            
            var campaigns = await _context.EmailCampaigns
                .Where(c => c.Status == "Sent" && c.SentCount > 0)
                .ToListAsync();

            var avgOpenRate = campaigns.Any() ? campaigns.Average(c => c.SentCount > 0 ? (double)c.OpenedCount / c.SentCount * 100 : 0) : 0;
            var avgClickRate = campaigns.Any() ? campaigns.Average(c => c.SentCount > 0 ? (double)c.ClickedCount / c.SentCount * 100 : 0) : 0;
            var unsubscribeRate = campaigns.Any() ? campaigns.Average(c => c.SentCount > 0 ? (double)c.UnsubscribedCount / c.SentCount * 100 : 0) : 0;

            var recentCampaigns = campaigns
                .OrderByDescending(c => c.SentDate)
                .Take(5)
                .Select(c => new CampaignPerformance
                {
                    CampaignId = c.Id,
                    CampaignName = c.Name,
                    SentDate = c.SentDate ?? DateTime.Now,
                    Recipients = c.SentCount,
                    Opens = c.OpenedCount,
                    Clicks = c.ClickedCount,
                    OpenRate = c.SentCount > 0 ? (double)c.OpenedCount / c.SentCount * 100 : 0,
                    ClickRate = c.SentCount > 0 ? (double)c.ClickedCount / c.SentCount * 100 : 0
                })
                .ToList();

            return new EmailStats
            {
                TotalCampaigns = totalCampaigns,
                ActiveSubscribers = activeSubscribers,
                TotalEmailsSent = totalEmailsSent,
                AverageOpenRate = avgOpenRate,
                AverageClickRate = avgClickRate,
                UnsubscribeRate = unsubscribeRate,
                RecentCampaigns = recentCampaigns
            };
        }

        public async Task<CampaignPerformance> GetCampaignPerformanceAsync(int campaignId)
        {
            var campaign = await GetCampaignByIdAsync(campaignId);
            if (campaign == null) return null;

            return new CampaignPerformance
            {
                CampaignId = campaign.Id,
                CampaignName = campaign.Name,
                SentDate = campaign.SentDate ?? DateTime.Now,
                Recipients = campaign.SentCount,
                Opens = campaign.OpenedCount,
                Clicks = campaign.ClickedCount,
                OpenRate = campaign.SentCount > 0 ? (double)campaign.OpenedCount / campaign.SentCount * 100 : 0,
                ClickRate = campaign.SentCount > 0 ? (double)campaign.ClickedCount / campaign.SentCount * 100 : 0
            };
        }

        public async Task TrackEmailOpenAsync(string trackingToken)
        {
            var log = await _context.EmailLogs
                .Include(l => l.Campaign)
                .FirstOrDefaultAsync(l => l.TrackingToken == trackingToken);

            if (log != null && log.OpenedAt == null)
            {
                log.OpenedAt = DateTime.Now;
                log.Campaign.OpenedCount++;
                await _context.SaveChangesAsync();
            }
        }

        public async Task TrackEmailClickAsync(string trackingToken, string url)
        {
            var log = await _context.EmailLogs
                .Include(l => l.Campaign)
                .FirstOrDefaultAsync(l => l.TrackingToken == trackingToken);

            if (log != null && log.ClickedAt == null)
            {
                log.ClickedAt = DateTime.Now;
                log.Campaign.ClickedCount++;
                await _context.SaveChangesAsync();
            }
        }

        #endregion

        #region Automation

        public async Task SendWelcomeEmailAsync(string email, string firstName = null)
        {
            var template = await _context.EmailTemplates
                .FirstOrDefaultAsync(t => t.TemplateType == "Welcome" && t.IsActive);

            if (template != null)
            {
                var content = PersonalizeContent(template.HtmlContent, email, firstName);
                await SendEmailAsync(email, template.Subject, content);
            }
        }

        public async Task SendBookingConfirmationAsync(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Tour)
                .Include(b => b.Hotel)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking?.User != null)
            {
                var template = await _context.EmailTemplates
                    .FirstOrDefaultAsync(t => t.TemplateType == "Booking_Confirmation" && t.IsActive);

                if (template != null)
                {
                    var content = PersonalizeBookingContent(template.HtmlContent, booking);
                    await SendEmailAsync(booking.User.Email, template.Subject, content);
                }
            }
        }

        public async Task SendBookingReminderAsync(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Tour)
                .Include(b => b.Hotel)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking?.User != null)
            {
                var template = await _context.EmailTemplates
                    .FirstOrDefaultAsync(t => t.TemplateType == "Reminder" && t.IsActive);

                if (template != null)
                {
                    var content = PersonalizeBookingContent(template.HtmlContent, booking);
                    await SendEmailAsync(booking.User.Email, template.Subject, content);
                }
            }
        }

        public async Task SendPromotionalEmailAsync(string email, string promoCode)
        {
            var template = await _context.EmailTemplates
                .FirstOrDefaultAsync(t => t.TemplateType == "Promotion" && t.IsActive);

            if (template != null)
            {
                var content = template.HtmlContent.Replace("{{PROMO_CODE}}", promoCode);
                content = PersonalizeContent(content, email);
                await SendEmailAsync(email, template.Subject, content);
            }
        }

        #endregion

        #region Private Methods

        private async Task SendEmailAsync(string toEmail, string subject, string content, string trackingToken = null)
        {
            var smtpHost = _configuration["Email:SmtpHost"];
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            var smtpUser = _configuration["Email:SmtpUser"];
            var smtpPass = _configuration["Email:SmtpPass"];
            var fromEmail = _configuration["Email:FromEmail"];
            var fromName = _configuration["Email:FromName"];

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            var message = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = AddTrackingPixel(content, trackingToken),
                IsBodyHtml = true
            };

            message.To.Add(toEmail);

            await client.SendMailAsync(message);
        }

        private string PersonalizeContent(string content, string email, string firstName = null)
        {
            content = content.Replace("{{EMAIL}}", email);
            content = content.Replace("{{FIRST_NAME}}", firstName ?? "Valued Customer");
            content = content.Replace("{{UNSUBSCRIBE_URL}}", $"{_configuration["BaseUrl"]}/EmailMarketing/Unsubscribe?email={email}");
            
            return content;
        }

        private string PersonalizeBookingContent(string content, Booking booking)
        {
            content = content.Replace("{{CUSTOMER_NAME}}", booking.User.Name ?? booking.User.Email);
            content = content.Replace("{{BOOKING_ID}}", booking.Id.ToString());
            content = content.Replace("{{START_DATE}}", booking.StartDate.ToString("dd/MM/yyyy"));
            content = content.Replace("{{END_DATE}}", booking.EndDate?.ToString("dd/MM/yyyy") ?? "N/A");
            content = content.Replace("{{TOTAL_PRICE}}", booking.TotalPrice.ToString("N0"));
            
            if (booking.Tour != null)
            {
                content = content.Replace("{{TOUR_NAME}}", booking.Tour.Name);
            }
            
            if (booking.Hotel != null)
            {
                content = content.Replace("{{HOTEL_NAME}}", booking.Hotel.Name);
            }

            return content;
        }

        private string AddTrackingPixel(string content, string trackingToken)
        {
            if (string.IsNullOrEmpty(trackingToken)) return content;

            var trackingPixel = $"<img src=\"{_configuration["BaseUrl"]}/EmailMarketing/Track?token={trackingToken}\" width=\"1\" height=\"1\" style=\"display:none;\" />";
            return content + trackingPixel;
        }

        #endregion
    }
}