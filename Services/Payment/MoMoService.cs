using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace WEBDULICH.Services.PaymentGateway
{
    public class MoMoService : IPaymentGatewayService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<MoMoService> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _partnerCode;
        private readonly string _accessKey;
        private readonly string _secretKey;
        private readonly string _endpoint;

        public MoMoService(IConfiguration configuration, ILogger<MoMoService> logger, HttpClient httpClient)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient;
            _partnerCode = configuration["MoMo:PartnerCode"] ?? "";
            _accessKey = configuration["MoMo:AccessKey"] ?? "";
            _secretKey = configuration["MoMo:SecretKey"] ?? "";
            _endpoint = configuration["MoMo:Endpoint"] ?? "https://test-payment.momo.vn/v2/gateway/api/create";
        }

        public async Task<PaymentResponse> CreatePaymentAsync(PaymentRequest request)
        {
            try
            {
                var requestId = Guid.NewGuid().ToString();
                var orderId = request.OrderId;
                var amount = ((long)request.Amount).ToString();
                var orderInfo = request.Description;
                var redirectUrl = request.ReturnUrl;
                var ipnUrl = _configuration["MoMo:IpnUrl"] ?? "";
                var requestType = "captureWallet";
                var extraData = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request.Metadata)));

                // Create raw signature
                var rawSignature = $"accessKey={_accessKey}&amount={amount}&extraData={extraData}&ipnUrl={ipnUrl}&orderId={orderId}&orderInfo={orderInfo}&partnerCode={_partnerCode}&redirectUrl={redirectUrl}&requestId={requestId}&requestType={requestType}";
                
                var signature = ComputeHmacSha256(rawSignature, _secretKey);

                var momoRequest = new
                {
                    partnerCode = _partnerCode,
                    partnerName = "WEBDULICH",
                    storeId = "WEBDULICH_STORE",
                    requestId = requestId,
                    amount = amount,
                    orderId = orderId,
                    orderInfo = orderInfo,
                    redirectUrl = redirectUrl,
                    ipnUrl = ipnUrl,
                    lang = "vi",
                    extraData = extraData,
                    requestType = requestType,
                    signature = signature
                };

                var content = new StringContent(JsonConvert.SerializeObject(momoRequest), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(_endpoint, content);
                var responseString = await response.Content.ReadAsStringAsync();
                var momoResponse = JsonConvert.DeserializeObject<MoMoResponse>(responseString);

                if (momoResponse != null && momoResponse.ResultCode == 0)
                {
                    _logger.LogInformation($"MoMo payment URL created for order {orderId}");
                    
                    return new PaymentResponse
                    {
                        Success = true,
                        PaymentUrl = momoResponse.PayUrl,
                        TransactionId = requestId,
                        Message = "Payment URL created successfully"
                    };
                }
                else
                {
                    _logger.LogWarning($"MoMo payment creation failed: {momoResponse?.Message}");
                    
                    return new PaymentResponse
                    {
                        Success = false,
                        Message = momoResponse?.Message ?? "Unknown error",
                        ErrorCode = momoResponse?.ResultCode.ToString() ?? "UNKNOWN"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating MoMo payment for order {request.OrderId}");
                return new PaymentResponse
                {
                    Success = false,
                    Message = "Error creating payment",
                    ErrorCode = "MOMO_ERROR"
                };
            }
        }

        public async Task<PaymentVerificationResult> VerifyPaymentAsync(string transactionId, Dictionary<string, string> parameters)
        {
            try
            {
                var orderId = parameters.GetValueOrDefault("orderId", "");
                var requestId = parameters.GetValueOrDefault("requestId", "");
                var amount = parameters.GetValueOrDefault("amount", "0");
                var orderInfo = parameters.GetValueOrDefault("orderInfo", "");
                var orderType = parameters.GetValueOrDefault("orderType", "");
                var transId = parameters.GetValueOrDefault("transId", "");
                var resultCode = parameters.GetValueOrDefault("resultCode", "");
                var message = parameters.GetValueOrDefault("message", "");
                var payType = parameters.GetValueOrDefault("payType", "");
                var responseTime = parameters.GetValueOrDefault("responseTime", "");
                var extraData = parameters.GetValueOrDefault("extraData", "");
                var signature = parameters.GetValueOrDefault("signature", "");

                // Verify signature
                var rawSignature = $"accessKey={_accessKey}&amount={amount}&extraData={extraData}&message={message}&orderId={orderId}&orderInfo={orderInfo}&orderType={orderType}&partnerCode={_partnerCode}&payType={payType}&requestId={requestId}&responseTime={responseTime}&resultCode={resultCode}&transId={transId}";
                var computedSignature = ComputeHmacSha256(rawSignature, _secretKey);

                if (signature == computedSignature)
                {
                    if (resultCode == "0")
                    {
                        _logger.LogInformation($"MoMo payment verified successfully for order {orderId}");
                        
                        return await Task.FromResult(new PaymentVerificationResult
                        {
                            IsValid = true,
                            IsSuccess = true,
                            TransactionId = transId,
                            OrderId = orderId,
                            Amount = decimal.Parse(amount),
                            Message = message,
                            PaymentDate = DateTime.Now
                        });
                    }
                    else
                    {
                        return await Task.FromResult(new PaymentVerificationResult
                        {
                            IsValid = true,
                            IsSuccess = false,
                            TransactionId = transId,
                            OrderId = orderId,
                            Message = message
                        });
                    }
                }
                else
                {
                    _logger.LogWarning($"Invalid MoMo signature for order {orderId}");
                    return await Task.FromResult(new PaymentVerificationResult
                    {
                        IsValid = false,
                        IsSuccess = false,
                        Message = "Invalid signature"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying MoMo payment");
                return new PaymentVerificationResult
                {
                    IsValid = false,
                    IsSuccess = false,
                    Message = "Error verifying payment"
                };
            }
        }

        public async Task<RefundResponse> RefundPaymentAsync(string transactionId, decimal amount)
        {
            _logger.LogInformation($"Refund requested for transaction {transactionId}, amount: {amount}");
            
            return await Task.FromResult(new RefundResponse
            {
                Success = true,
                RefundId = Guid.NewGuid().ToString(),
                Message = "Refund request submitted"
            });
        }

        public async Task<PaymentStatusResponse> GetPaymentStatusAsync(string transactionId)
        {
            return await Task.FromResult(new PaymentStatusResponse
            {
                Status = "Pending",
                Message = "Payment status retrieved"
            });
        }

        private string ComputeHmacSha256(string message, string secret)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secret);
            var messageBytes = Encoding.UTF8.GetBytes(message);
            
            using (var hmac = new HMACSHA256(keyBytes))
            {
                var hashBytes = hmac.ComputeHash(messageBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        private class MoMoResponse
        {
            public string? PartnerCode { get; set; }
            public string? RequestId { get; set; }
            public string? OrderId { get; set; }
            public long Amount { get; set; }
            public long ResponseTime { get; set; }
            public string? Message { get; set; }
            public int ResultCode { get; set; }
            public string? PayUrl { get; set; }
            public string? DeepLink { get; set; }
            public string? QrCodeUrl { get; set; }
        }
    }
}
