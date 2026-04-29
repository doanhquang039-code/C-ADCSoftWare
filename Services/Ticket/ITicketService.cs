namespace WEBDULICH.Services.Ticket
{
    public interface ITicketService
    {
        Task<TicketGenerationResult> GenerateTicketAsync(int bookingId);
        Task<byte[]> GenerateTicketPdfAsync(int bookingId);
        Task<string> GenerateQRCodeAsync(string ticketCode);
        Task<TicketValidationResult> ValidateTicketAsync(string ticketCode);
        Task<bool> SendTicketEmailAsync(int bookingId, string email);
        Task<List<Models.Ticket>> GetTicketsByBookingAsync(int bookingId);
    }

    public class TicketGenerationResult
    {
        public bool Success { get; set; }
        public string TicketCode { get; set; } = string.Empty;
        public string QRCodeBase64 { get; set; } = string.Empty;
        public byte[]? PdfData { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime GeneratedAt { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidUntil { get; set; }
    }

    public class Ticket
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public string TicketCode { get; set; } = string.Empty;
        public string QRCode { get; set; } = string.Empty;
        public TicketStatus Status { get; set; }
        public DateTime GeneratedAt { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidUntil { get; set; }
        public DateTime? UsedAt { get; set; }
        public string? UsedBy { get; set; }
        public string PassengerName { get; set; } = string.Empty;
        public string PassengerEmail { get; set; } = string.Empty;
        public string PassengerPhone { get; set; } = string.Empty;
        public Dictionary<string, string> Metadata { get; set; } = new();
    }

    public class TicketValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
        public Models.Ticket? Ticket { get; set; }
        public ValidationError? Error { get; set; }
    }

    public class ValidationError
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public enum TicketStatus
    {
        Active,
        Used,
        Expired,
        Cancelled,
        Refunded
    }
}
