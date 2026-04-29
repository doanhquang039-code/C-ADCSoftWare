using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WEBDULICH.Services.Ticket;

namespace WEBDULICH.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TicketController : ControllerBase
    {
        private readonly ITicketService _ticketService;
        private readonly ILogger<TicketController> _logger;

        public TicketController(
            ITicketService ticketService,
            ILogger<TicketController> logger)
        {
            _ticketService = ticketService;
            _logger = logger;
        }

        [HttpPost("generate/{bookingId}")]
        public async Task<IActionResult> GenerateTicket(int bookingId)
        {
            try
            {
                var result = await _ticketService.GenerateTicketAsync(bookingId);
                
                if (result.Success)
                {
                    return Ok(new 
                    { 
                        success = true, 
                        data = new
                        {
                            ticketCode = result.TicketCode,
                            qrCode = result.QRCodeBase64,
                            validFrom = result.ValidFrom,
                            validUntil = result.ValidUntil
                        }
                    });
                }

                return BadRequest(new { success = false, message = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating ticket for booking {bookingId}");
                return StatusCode(500, new { success = false, message = "Failed to generate ticket" });
            }
        }

        [HttpGet("pdf/{bookingId}")]
        public async Task<IActionResult> GetTicketPdf(int bookingId)
        {
            try
            {
                var pdfBytes = await _ticketService.GenerateTicketPdfAsync(bookingId);
                return File(pdfBytes, "application/pdf", $"ticket-{bookingId}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating PDF for booking {bookingId}");
                return StatusCode(500, new { success = false, message = "Failed to generate PDF" });
            }
        }

        [HttpGet("validate/{ticketCode}")]
        [AllowAnonymous]
        public async Task<IActionResult> ValidateTicket(string ticketCode)
        {
            try
            {
                var result = await _ticketService.ValidateTicketAsync(ticketCode);
                
                return Ok(new 
                { 
                    success = result.IsValid, 
                    message = result.Message,
                    ticket = result.Ticket,
                    error = result.Error
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error validating ticket {ticketCode}");
                return StatusCode(500, new { success = false, message = "Validation failed" });
            }
        }

        [HttpPost("send-email/{bookingId}")]
        public async Task<IActionResult> SendTicketEmail(int bookingId, [FromBody] SendTicketEmailRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    return BadRequest(new { success = false, message = "Email is required" });
                }

                var success = await _ticketService.SendTicketEmailAsync(bookingId, request.Email);
                
                if (success)
                {
                    return Ok(new { success = true, message = "Ticket sent successfully" });
                }

                return BadRequest(new { success = false, message = "Failed to send ticket" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending ticket email for booking {bookingId}");
                return StatusCode(500, new { success = false, message = "Failed to send email" });
            }
        }

        [HttpGet("booking/{bookingId}")]
        public async Task<IActionResult> GetTicketsByBooking(int bookingId)
        {
            try
            {
                var tickets = await _ticketService.GetTicketsByBookingAsync(bookingId);
                return Ok(new { success = true, data = tickets });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving tickets for booking {bookingId}");
                return StatusCode(500, new { success = false, message = "Failed to retrieve tickets" });
            }
        }

        [HttpGet("qrcode/{ticketCode}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetQRCode(string ticketCode)
        {
            try
            {
                var qrCodeBase64 = await _ticketService.GenerateQRCodeAsync(ticketCode);
                var qrCodeBytes = Convert.FromBase64String(qrCodeBase64);
                return File(qrCodeBytes, "image/png");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating QR code for ticket {ticketCode}");
                return StatusCode(500, new { success = false, message = "Failed to generate QR code" });
            }
        }
    }

    public class SendTicketEmailRequest
    {
        public string Email { get; set; } = string.Empty;
    }
}
