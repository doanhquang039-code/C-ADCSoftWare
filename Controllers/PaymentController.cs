using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WEBDULICH.Services.PaymentGateway;

namespace WEBDULICH.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly VNPayService _vnpayService;
        private readonly MoMoService _momoService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            VNPayService vnpayService,
            MoMoService momoService,
            ILogger<PaymentController> logger)
        {
            _vnpayService = vnpayService;
            _momoService = momoService;
            _logger = logger;
        }

        [HttpPost("vnpay/create")]
        [Authorize]
        public async Task<IActionResult> CreateVNPayPayment([FromBody] PaymentRequest request)
        {
            try
            {
                var result = await _vnpayService.CreatePaymentAsync(request);
                
                if (result.Success)
                {
                    return Ok(new { success = true, paymentUrl = result.PaymentUrl, transactionId = result.TransactionId });
                }

                return BadRequest(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating VNPay payment");
                return StatusCode(500, new { success = false, message = "Payment creation failed" });
            }
        }

        [HttpGet("vnpay/return")]
        public async Task<IActionResult> VNPayReturn([FromQuery] Dictionary<string, string> queryParams)
        {
            try
            {
                var transactionId = queryParams.GetValueOrDefault("vnp_TxnRef", "");
                var result = await _vnpayService.VerifyPaymentAsync(transactionId, queryParams);
                
                if (result.IsValid && result.IsSuccess)
                {
                    return Redirect($"/payment/success?orderId={result.OrderId}&amount={result.Amount}");
                }

                return Redirect($"/payment/failure?message={result.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing VNPay return");
                return Redirect("/payment/failure?message=Payment verification failed");
            }
        }

        [HttpPost("vnpay/ipn")]
        public async Task<IActionResult> VNPayIPN([FromQuery] Dictionary<string, string> queryParams)
        {
            try
            {
                var transactionId = queryParams.GetValueOrDefault("vnp_TxnRef", "");
                var result = await _vnpayService.VerifyPaymentAsync(transactionId, queryParams);
                
                if (result.IsValid && result.IsSuccess)
                {
                    // Update order status in database
                    // ... your business logic here ...
                    
                    return Ok(new { RspCode = "00", Message = "Confirm Success" });
                }

                return Ok(new { RspCode = "97", Message = "Invalid Signature" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing VNPay IPN");
                return Ok(new { RspCode = "99", Message = "Unknown error" });
            }
        }

        [HttpPost("momo/create")]
        [Authorize]
        public async Task<IActionResult> CreateMoMoPayment([FromBody] PaymentRequest request)
        {
            try
            {
                var result = await _momoService.CreatePaymentAsync(request);
                
                if (result.Success)
                {
                    return Ok(new 
                    { 
                        success = true, 
                        paymentUrl = result.PaymentUrl, 
                        transactionId = result.TransactionId
                    });
                }

                return BadRequest(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating MoMo payment");
                return StatusCode(500, new { success = false, message = "Payment creation failed" });
            }
        }

        [HttpGet("momo/return")]
        public async Task<IActionResult> MoMoReturn([FromQuery] Dictionary<string, string> queryParams)
        {
            try
            {
                var transactionId = queryParams.GetValueOrDefault("orderId", "");
                var result = await _momoService.VerifyPaymentAsync(transactionId, queryParams);
                
                if (result.IsValid && result.IsSuccess)
                {
                    return Redirect($"/payment/success?orderId={result.OrderId}&amount={result.Amount}");
                }

                return Redirect($"/payment/failure?message={result.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing MoMo return");
                return Redirect("/payment/failure?message=Payment verification failed");
            }
        }

        [HttpPost("momo/ipn")]
        public async Task<IActionResult> MoMoIPN([FromBody] Dictionary<string, string> data)
        {
            try
            {
                var transactionId = data.GetValueOrDefault("orderId", "");
                var result = await _momoService.VerifyPaymentAsync(transactionId, data);
                
                if (result.IsValid && result.IsSuccess)
                {
                    // Update order status in database
                    // ... your business logic here ...
                    
                    return Ok(new { resultCode = 0, message = "Success" });
                }

                return Ok(new { resultCode = 97, message = "Invalid Signature" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing MoMo IPN");
                return Ok(new { resultCode = 99, message = "Unknown error" });
            }
        }

        [HttpPost("vnpay/refund")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RefundVNPay([FromBody] RefundRequestDto request)
        {
            try
            {
                var result = await _vnpayService.RefundPaymentAsync(request.TransactionId, request.Amount);
                
                if (result.Success)
                {
                    return Ok(new { success = true, message = "Refund successful", refundId = result.RefundId });
                }

                return BadRequest(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing VNPay refund");
                return StatusCode(500, new { success = false, message = "Refund failed" });
            }
        }

        [HttpPost("momo/refund")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RefundMoMo([FromBody] RefundRequestDto request)
        {
            try
            {
                var result = await _momoService.RefundPaymentAsync(request.TransactionId, request.Amount);
                
                if (result.Success)
                {
                    return Ok(new { success = true, message = "Refund successful", refundId = result.RefundId });
                }

                return BadRequest(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing MoMo refund");
                return StatusCode(500, new { success = false, message = "Refund failed" });
            }
        }

        [HttpGet("vnpay/status/{transactionId}")]
        [Authorize]
        public async Task<IActionResult> GetVNPayStatus(string transactionId)
        {
            try
            {
                var result = await _vnpayService.GetPaymentStatusAsync(transactionId);
                
                return Ok(new { success = true, status = result.Status, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying VNPay status");
                return StatusCode(500, new { success = false, message = "Status query failed" });
            }
        }

        [HttpGet("momo/status/{transactionId}")]
        [Authorize]
        public async Task<IActionResult> GetMoMoStatus(string transactionId)
        {
            try
            {
                var result = await _momoService.GetPaymentStatusAsync(transactionId);
                
                return Ok(new { success = true, status = result.Status, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying MoMo status");
                return StatusCode(500, new { success = false, message = "Status query failed" });
            }
        }
    }
}

    public class RefundRequestDto
    {
        public string TransactionId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
