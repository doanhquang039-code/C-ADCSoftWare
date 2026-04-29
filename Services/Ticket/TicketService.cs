using Microsoft.EntityFrameworkCore;
using WEBDULICH.Models;
using System.Security.Cryptography;
using System.Text;

namespace WEBDULICH.Services.Ticket
{
    public class TicketService : ITicketService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TicketService> _logger;
        private readonly IEmailService _emailService;

        public TicketService(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<TicketService> logger,
            IEmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _emailService = emailService;
        }

        public async Task<TicketGenerationResult> GenerateTicketAsync(int bookingId)
        {
            try
            {
                var booking = await _context.Orders
                    .Include(o => o.User)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Tour)
                    .FirstOrDefaultAsync(o => o.Id == bookingId);

                if (booking == null)
                {
                    return new TicketGenerationResult
                    {
                        Success = false,
                        ErrorMessage = "Booking not found"
                    };
                }

                // Generate unique ticket code
                var ticketCode = GenerateTicketCode(bookingId);
                
                // Generate QR code
                var qrCodeBase64 = await GenerateQRCodeAsync(ticketCode);

                // Calculate validity
                var validityDays = _configuration.GetValue<int>("Ticket:ValidityDays", 365);
                var validFrom = DateTime.UtcNow;
                var validUntil = validFrom.AddDays(validityDays);

                // Create ticket record
                var ticket = new Models.Ticket
                {
                    BookingId = bookingId,
                    TicketCode = ticketCode,
                    QRCode = qrCodeBase64,
                    Status = TicketStatus.Active,
                    GeneratedAt = DateTime.UtcNow,
                    ValidFrom = validFrom,
                    ValidUntil = validUntil,
                    PassengerName = booking.User?.FullName ?? "Guest",
                    PassengerEmail = booking.User?.Email ?? "",
                    PassengerPhone = booking.User?.PhoneNumber ?? "",
                    Metadata = new Dictionary<string, string>
                    {
                        { "BookingId", bookingId.ToString() },
                        { "TotalAmount", booking.TotalAmount.ToString() },
                        { "OrderDate", booking.OrderDate.ToString("yyyy-MM-dd") }
                    }
                };

                _context.Tickets.Add(ticket);
                await _context.SaveChangesAsync();

                // Generate PDF
                var pdfData = await GenerateTicketPdfAsync(bookingId);

                _logger.LogInformation($"Generated ticket {ticketCode} for booking {bookingId}");

                return new TicketGenerationResult
                {
                    Success = true,
                    TicketCode = ticketCode,
                    QRCodeBase64 = qrCodeBase64,
                    PdfData = pdfData,
                    GeneratedAt = validFrom,
                    ValidFrom = validFrom,
                    ValidUntil = validUntil
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating ticket for booking {bookingId}");
                return new TicketGenerationResult
                {
                    Success = false,
                    ErrorMessage = "Failed to generate ticket"
                };
            }
        }

        public async Task<byte[]> GenerateTicketPdfAsync(int bookingId)
        {
            try
            {
                var booking = await _context.Orders
                    .Include(o => o.User)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Tour)
                    .FirstOrDefaultAsync(o => o.Id == bookingId);

                if (booking == null)
                {
                    throw new Exception("Booking not found");
                }

                var ticket = await _context.Tickets
                    .FirstOrDefaultAsync(t => t.BookingId == bookingId);

                if (ticket == null)
                {
                    throw new Exception("Ticket not found");
                }

                // Simple PDF generation (in production, use iTextSharp or similar)
                var pdfContent = GenerateSimplePdfContent(booking, ticket);
                return Encoding.UTF8.GetBytes(pdfContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating PDF for booking {bookingId}");
                throw;
            }
        }

        public async Task<string> GenerateQRCodeAsync(string ticketCode)
        {
            try
            {
                // Simple QR code generation (in production, use QRCoder library)
                // For now, return a placeholder base64 string
                var qrData = $"TICKET:{ticketCode}:{DateTime.UtcNow:yyyyMMddHHmmss}";
                var qrBytes = Encoding.UTF8.GetBytes(qrData);
                return Convert.ToBase64String(qrBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating QR code for ticket {ticketCode}");
                throw;
            }
        }

        public async Task<TicketValidationResult> ValidateTicketAsync(string ticketCode)
        {
            try
            {
                var ticket = await _context.Tickets
                    .Include(t => t.Booking)
                    .FirstOrDefaultAsync(t => t.TicketCode == ticketCode);

                if (ticket == null)
                {
                    return new TicketValidationResult
                    {
                        IsValid = false,
                        Message = "Ticket not found",
                        Error = new ValidationError
                        {
                            Code = "TICKET_NOT_FOUND",
                            Message = "The ticket code is invalid"
                        }
                    };
                }

                // Check if already used
                if (ticket.Status == TicketStatus.Used)
                {
                    return new TicketValidationResult
                    {
                        IsValid = false,
                        Message = "Ticket already used",
                        Ticket = ticket,
                        Error = new ValidationError
                        {
                            Code = "TICKET_ALREADY_USED",
                            Message = $"This ticket was used on {ticket.UsedAt:yyyy-MM-dd HH:mm}"
                        }
                    };
                }

                // Check if expired
                if (ticket.Status == TicketStatus.Expired || DateTime.UtcNow > ticket.ValidUntil)
                {
                    ticket.Status = TicketStatus.Expired;
                    await _context.SaveChangesAsync();

                    return new TicketValidationResult
                    {
                        IsValid = false,
                        Message = "Ticket expired",
                        Ticket = ticket,
                        Error = new ValidationError
                        {
                            Code = "TICKET_EXPIRED",
                            Message = $"This ticket expired on {ticket.ValidUntil:yyyy-MM-dd}"
                        }
                    };
                }

                // Check if cancelled
                if (ticket.Status == TicketStatus.Cancelled)
                {
                    return new TicketValidationResult
                    {
                        IsValid = false,
                        Message = "Ticket cancelled",
                        Ticket = ticket,
                        Error = new ValidationError
                        {
                            Code = "TICKET_CANCELLED",
                            Message = "This ticket has been cancelled"
                        }
                    };
                }

                // Check if not yet valid
                if (DateTime.UtcNow < ticket.ValidFrom)
                {
                    return new TicketValidationResult
                    {
                        IsValid = false,
                        Message = "Ticket not yet valid",
                        Ticket = ticket,
                        Error = new ValidationError
                        {
                            Code = "TICKET_NOT_YET_VALID",
                            Message = $"This ticket is valid from {ticket.ValidFrom:yyyy-MM-dd}"
                        }
                    };
                }

                // Ticket is valid
                return new TicketValidationResult
                {
                    IsValid = true,
                    Message = "Ticket is valid",
                    Ticket = ticket
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error validating ticket {ticketCode}");
                return new TicketValidationResult
                {
                    IsValid = false,
                    Message = "Validation error",
                    Error = new ValidationError
                    {
                        Code = "VALIDATION_ERROR",
                        Message = "An error occurred during validation"
                    }
                };
            }
        }

        public async Task<bool> SendTicketEmailAsync(int bookingId, string email)
        {
            try
            {
                var ticket = await _context.Tickets
                    .Include(t => t.Booking)
                        .ThenInclude(b => b.OrderDetails)
                            .ThenInclude(od => od.Tour)
                    .FirstOrDefaultAsync(t => t.BookingId == bookingId);

                if (ticket == null)
                {
                    _logger.LogWarning($"Ticket not found for booking {bookingId}");
                    return false;
                }

                // Generate PDF
                var pdfData = await GenerateTicketPdfAsync(bookingId);

                // Send email with ticket attachment
                var subject = $"Your E-Ticket - Booking #{bookingId}";
                var body = GenerateTicketEmailBody(ticket);

                await _emailService.SendEmailWithAttachmentAsync(
                    email,
                    subject,
                    body,
                    pdfData,
                    $"ticket-{ticket.TicketCode}.pdf"
                );

                _logger.LogInformation($"Sent ticket email for booking {bookingId} to {email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending ticket email for booking {bookingId}");
                return false;
            }
        }

        public async Task<List<Models.Ticket>> GetTicketsByBookingAsync(int bookingId)
        {
            return await _context.Tickets
                .Where(t => t.BookingId == bookingId)
                .OrderByDescending(t => t.GeneratedAt)
                .ToListAsync();
        }

        private string GenerateTicketCode(int bookingId)
        {
            // Generate unique ticket code: PREFIX-BOOKINGID-RANDOM-CHECKSUM
            var prefix = "TKT";
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var random = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
            var baseCode = $"{prefix}{bookingId:D6}{timestamp}{random}";
            
            // Add checksum
            var checksum = CalculateChecksum(baseCode);
            return $"{baseCode}{checksum}";
        }

        private string CalculateChecksum(string input)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(hash).Substring(0, 4);
        }

        private string GenerateSimplePdfContent(Orders booking, Models.Ticket ticket)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=".PadRight(60, '='));
            sb.AppendLine("E-TICKET - WEBDULICH");
            sb.AppendLine("=".PadRight(60, '='));
            sb.AppendLine();
            sb.AppendLine($"Ticket Code: {ticket.TicketCode}");
            sb.AppendLine($"Booking ID: #{booking.Id}");
            sb.AppendLine($"Passenger: {ticket.PassengerName}");
            sb.AppendLine($"Email: {ticket.PassengerEmail}");
            sb.AppendLine($"Phone: {ticket.PassengerPhone}");
            sb.AppendLine();
            sb.AppendLine("Tour Details:");
            sb.AppendLine("-".PadRight(60, '-'));
            
            foreach (var detail in booking.OrderDetails)
            {
                sb.AppendLine($"  {detail.Tour?.Name}");
                sb.AppendLine($"  Quantity: {detail.Quantity} x {detail.Price:N0} VND");
            }
            
            sb.AppendLine("-".PadRight(60, '-'));
            sb.AppendLine($"Total Amount: {booking.TotalAmount:N0} VND");
            sb.AppendLine();
            sb.AppendLine($"Valid From: {ticket.ValidFrom:yyyy-MM-dd HH:mm}");
            sb.AppendLine($"Valid Until: {ticket.ValidUntil:yyyy-MM-dd HH:mm}");
            sb.AppendLine($"Status: {ticket.Status}");
            sb.AppendLine();
            sb.AppendLine("QR Code: [QR CODE PLACEHOLDER]");
            sb.AppendLine();
            sb.AppendLine("=".PadRight(60, '='));
            sb.AppendLine("Thank you for choosing WEBDULICH!");
            sb.AppendLine("=".PadRight(60, '='));
            
            return sb.ToString();
        }

        private string GenerateTicketEmailBody(Models.Ticket ticket)
        {
            return $@"
                <html>
                <body>
                    <h2>Your E-Ticket is Ready!</h2>
                    <p>Dear {ticket.PassengerName},</p>
                    <p>Thank you for booking with WEBDULICH. Your e-ticket is attached to this email.</p>
                    <p><strong>Ticket Code:</strong> {ticket.TicketCode}</p>
                    <p><strong>Valid From:</strong> {ticket.ValidFrom:yyyy-MM-dd HH:mm}</p>
                    <p><strong>Valid Until:</strong> {ticket.ValidUntil:yyyy-MM-dd HH:mm}</p>
                    <p>Please present this ticket (printed or on your mobile device) at the check-in counter.</p>
                    <p>Have a great trip!</p>
                    <br>
                    <p>Best regards,<br>WEBDULICH Team</p>
                </body>
                </html>
            ";
        }
    }
}
