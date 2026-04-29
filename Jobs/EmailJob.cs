using Hangfire;

namespace WEBDULICH.Jobs
{
    public class EmailJob
    {
        private readonly ILogger<EmailJob> _logger;

        public EmailJob(ILogger<EmailJob> logger)
        {
            _logger = logger;
        }

        [AutomaticRetry(Attempts = 3)]
        public async Task SendWelcomeEmail(string email, string userName)
        {
            _logger.LogInformation($"Sending welcome email to {email}");
            
            // Simulate email sending
            await Task.Delay(1000);
            
            _logger.LogInformation($"Welcome email sent successfully to {email}");
        }

        [AutomaticRetry(Attempts = 5)]
        public async Task SendBookingConfirmation(int bookingId, string email)
        {
            _logger.LogInformation($"Sending booking confirmation for booking #{bookingId} to {email}");
            
            // Simulate email sending
            await Task.Delay(1000);
            
            _logger.LogInformation($"Booking confirmation sent successfully");
        }

        [AutomaticRetry(Attempts = 3)]
        public async Task SendPasswordResetEmail(string email, string resetToken)
        {
            _logger.LogInformation($"Sending password reset email to {email}");
            
            // Simulate email sending
            await Task.Delay(1000);
            
            _logger.LogInformation($"Password reset email sent successfully");
        }

        [DisableConcurrentExecution(timeoutInSeconds: 10 * 60)]
        public async Task SendDailyNewsletters()
        {
            _logger.LogInformation("Starting daily newsletter job");
            
            // Simulate newsletter sending
            await Task.Delay(5000);
            
            _logger.LogInformation("Daily newsletters sent successfully");
        }
    }
}
