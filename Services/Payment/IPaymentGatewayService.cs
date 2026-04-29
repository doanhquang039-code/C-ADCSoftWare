namespace WEBDULICH.Services.PaymentGateway
{
    public interface IPaymentGatewayService
    {
        Task<PaymentResponse> CreatePaymentAsync(PaymentRequest request);
        Task<PaymentVerificationResult> VerifyPaymentAsync(string transactionId, Dictionary<string, string> parameters);
        Task<RefundResponse> RefundPaymentAsync(string transactionId, decimal amount);
        Task<PaymentStatusResponse> GetPaymentStatusAsync(string transactionId);
    }

    public class PaymentRequest
    {
        public string OrderId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "VND";
        public string Description { get; set; } = string.Empty;
        public string ReturnUrl { get; set; } = string.Empty;
        public string CancelUrl { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public Dictionary<string, string> Metadata { get; set; } = new();
    }

    public class PaymentResponse
    {
        public bool Success { get; set; }
        public string PaymentUrl { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string ErrorCode { get; set; } = string.Empty;
    }

    public class PaymentVerificationResult
    {
        public bool IsValid { get; set; }
        public bool IsSuccess { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string OrderId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime? PaymentDate { get; set; }
    }

    public class RefundResponse
    {
        public bool Success { get; set; }
        public string RefundId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class PaymentStatusResponse
    {
        public string Status { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
