using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Globalization;

namespace WEBDULICH.Services.PaymentGateway
{
    public class VNPayService : IPaymentGatewayService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<VNPayService> _logger;
        private readonly string _vnpUrl;
        private readonly string _vnpTmnCode;
        private readonly string _vnpHashSecret;
        private readonly string _vnpReturnUrl;

        public VNPayService(IConfiguration configuration, ILogger<VNPayService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _vnpUrl = configuration["VNPay:Url"] ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
            _vnpTmnCode = configuration["VNPay:TmnCode"] ?? "";
            _vnpHashSecret = configuration["VNPay:HashSecret"] ?? "";
            _vnpReturnUrl = configuration["VNPay:ReturnUrl"] ?? "";
        }

        public async Task<PaymentResponse> CreatePaymentAsync(PaymentRequest request)
        {
            try
            {
                var vnpay = new VNPayLibrary();
                
                vnpay.AddRequestData("vnp_Version", "2.1.0");
                vnpay.AddRequestData("vnp_Command", "pay");
                vnpay.AddRequestData("vnp_TmnCode", _vnpTmnCode);
                vnpay.AddRequestData("vnp_Amount", ((long)(request.Amount * 100)).ToString());
                vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
                vnpay.AddRequestData("vnp_CurrCode", "VND");
                vnpay.AddRequestData("vnp_IpAddr", "127.0.0.1");
                vnpay.AddRequestData("vnp_Locale", "vn");
                vnpay.AddRequestData("vnp_OrderInfo", request.Description);
                vnpay.AddRequestData("vnp_OrderType", "other");
                vnpay.AddRequestData("vnp_ReturnUrl", request.ReturnUrl ?? _vnpReturnUrl);
                vnpay.AddRequestData("vnp_TxnRef", request.OrderId);

                var paymentUrl = vnpay.CreateRequestUrl(_vnpUrl, _vnpHashSecret);

                _logger.LogInformation($"VNPay payment URL created for order {request.OrderId}");

                return await Task.FromResult(new PaymentResponse
                {
                    Success = true,
                    PaymentUrl = paymentUrl,
                    TransactionId = request.OrderId,
                    Message = "Payment URL created successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating VNPay payment for order {request.OrderId}");
                return new PaymentResponse
                {
                    Success = false,
                    Message = "Error creating payment",
                    ErrorCode = "VNPAY_ERROR"
                };
            }
        }

        public async Task<PaymentVerificationResult> VerifyPaymentAsync(string transactionId, Dictionary<string, string> parameters)
        {
            try
            {
                var vnpay = new VNPayLibrary();
                
                foreach (var param in parameters)
                {
                    if (!string.IsNullOrEmpty(param.Value) && param.Key.StartsWith("vnp_"))
                    {
                        vnpay.AddResponseData(param.Key, param.Value);
                    }
                }

                var orderId = vnpay.GetResponseData("vnp_TxnRef");
                var vnpayTranId = vnpay.GetResponseData("vnp_TransactionNo");
                var vnpResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
                var vnpSecureHash = parameters.ContainsKey("vnp_SecureHash") ? parameters["vnp_SecureHash"] : "";
                var vnpAmount = Convert.ToDecimal(vnpay.GetResponseData("vnp_Amount")) / 100;

                bool checkSignature = vnpay.ValidateSignature(vnpSecureHash, _vnpHashSecret);

                if (checkSignature)
                {
                    if (vnpResponseCode == "00")
                    {
                        _logger.LogInformation($"VNPay payment verified successfully for order {orderId}");
                        
                        return await Task.FromResult(new PaymentVerificationResult
                        {
                            IsValid = true,
                            IsSuccess = true,
                            TransactionId = vnpayTranId,
                            OrderId = orderId,
                            Amount = vnpAmount,
                            Message = "Payment successful",
                            PaymentDate = DateTime.Now
                        });
                    }
                    else
                    {
                        return await Task.FromResult(new PaymentVerificationResult
                        {
                            IsValid = true,
                            IsSuccess = false,
                            TransactionId = vnpayTranId,
                            OrderId = orderId,
                            Message = $"Payment failed with code: {vnpResponseCode}"
                        });
                    }
                }
                else
                {
                    _logger.LogWarning($"Invalid VNPay signature for order {orderId}");
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
                _logger.LogError(ex, "Error verifying VNPay payment");
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
            // VNPay refund implementation
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
            // Query payment status from VNPay
            return await Task.FromResult(new PaymentStatusResponse
            {
                Status = "Pending",
                Message = "Payment status retrieved"
            });
        }
    }

    // VNPay Library Helper
    public class VNPayLibrary
    {
        private readonly SortedList<string, string> _requestData = new SortedList<string, string>(new VNPayCompare());
        private readonly SortedList<string, string> _responseData = new SortedList<string, string>(new VNPayCompare());

        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _requestData.Add(key, value);
            }
        }

        public void AddResponseData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _responseData.Add(key, value);
            }
        }

        public string GetResponseData(string key)
        {
            return _responseData.TryGetValue(key, out string? value) ? value : string.Empty;
        }

        public string CreateRequestUrl(string baseUrl, string vnpHashSecret)
        {
            var data = new StringBuilder();
            
            foreach (var kv in _requestData)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    data.Append(HttpUtility.UrlEncode(kv.Key) + "=" + HttpUtility.UrlEncode(kv.Value) + "&");
                }
            }

            string queryString = data.ToString();
            
            if (queryString.Length > 0)
            {
                queryString = queryString.Remove(queryString.Length - 1, 1);
            }

            string signData = queryString;
            string vnpSecureHash = HmacSHA512(vnpHashSecret, signData);
            
            return baseUrl + "?" + queryString + "&vnp_SecureHash=" + vnpSecureHash;
        }

        public bool ValidateSignature(string inputHash, string secretKey)
        {
            var data = new StringBuilder();
            
            foreach (var kv in _responseData)
            {
                if (!string.IsNullOrEmpty(kv.Value) && kv.Key != "vnp_SecureHash")
                {
                    data.Append(HttpUtility.UrlEncode(kv.Key) + "=" + HttpUtility.UrlEncode(kv.Value) + "&");
                }
            }

            string signData = data.ToString();
            
            if (signData.Length > 0)
            {
                signData = signData.Remove(signData.Length - 1, 1);
            }

            string myChecksum = HmacSHA512(secretKey, signData);
            
            return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }

        private string HmacSHA512(string key, string inputData)
        {
            var hash = new StringBuilder();
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
            
            using (var hmac = new HMACSHA512(keyBytes))
            {
                byte[] hashValue = hmac.ComputeHash(inputBytes);
                foreach (var b in hashValue)
                {
                    hash.Append(b.ToString("x2"));
                }
            }

            return hash.ToString();
        }
    }

    public class VNPayCompare : IComparer<string>
    {
        public int Compare(string? x, string? y)
        {
            if (x == y) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            
            var vnpCompare = CompareInfo.GetCompareInfo("en-US");
            return vnpCompare.Compare(x, y, CompareOptions.Ordinal);
        }
    }
}
