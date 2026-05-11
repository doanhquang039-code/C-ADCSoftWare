using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WEBDULICH.Services.Blockchain;

namespace WEBDULICH.Controllers
{
    /// <summary>
    /// Blockchain Controller for Secure Booking & Payment Verification
    /// API endpoints cho hệ thống blockchain
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class BlockchainController : ControllerBase
    {
        private readonly IBlockchainService _blockchainService;
        private readonly ILogger<BlockchainController> _logger;

        public BlockchainController(
            IBlockchainService blockchainService,
            ILogger<BlockchainController> logger)
        {
            _blockchainService = blockchainService;
            _logger = logger;
        }

        /// <summary>
        /// Create a new booking block in the blockchain
        /// </summary>
        /// <param name="request">Booking block request</param>
        /// <returns>Created block</returns>
        [HttpPost("blocks/booking")]
        [Authorize]
        [ProducesResponseType(typeof(Block), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CreateBookingBlock([FromBody] CreateBookingBlockRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { message = "Request body is required" });
                }

                if (request.BookingId <= 0)
                {
                    return BadRequest(new { message = "Invalid booking ID" });
                }

                if (request.Amount <= 0)
                {
                    return BadRequest(new { message = "Amount must be greater than 0" });
                }

                if (string.IsNullOrWhiteSpace(request.Details))
                {
                    return BadRequest(new { message = "Details are required" });
                }

                _logger.LogInformation($"Creating booking block for booking {request.BookingId}");

                var block = await _blockchainService.CreateBookingBlockAsync(
                    request.BookingId,
                    request.Amount,
                    request.Details
                );

                return CreatedAtAction(
                    nameof(GetBlockByHash),
                    new { hash = block.Hash },
                    new
                    {
                        success = true,
                        block = block,
                        message = "Booking block created successfully"
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating booking block for booking {request?.BookingId}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get the entire blockchain
        /// </summary>
        /// <returns>List of all blocks</returns>
        [HttpGet("blocks")]
        [ProducesResponseType(typeof(List<Block>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetBlockchain()
        {
            try
            {
                _logger.LogInformation("Getting entire blockchain");

                var blockchain = await _blockchainService.GetBlockchainAsync();

                return Ok(new
                {
                    success = true,
                    totalBlocks = blockchain.Count,
                    blockchain = blockchain,
                    message = "Blockchain retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting blockchain");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get a specific block by hash
        /// </summary>
        /// <param name="hash">Block hash</param>
        /// <returns>Block details</returns>
        [HttpGet("blocks/{hash}")]
        [ProducesResponseType(typeof(Block), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetBlockByHash(string hash)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(hash))
                {
                    return BadRequest(new { message = "Hash is required" });
                }

                _logger.LogInformation($"Getting block with hash {hash}");

                var block = await _blockchainService.GetBlockByHashAsync(hash);

                if (block == null)
                {
                    return NotFound(new { message = $"Block with hash {hash} not found" });
                }

                return Ok(new
                {
                    success = true,
                    block = block,
                    message = "Block retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting block with hash {hash}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Verify blockchain integrity
        /// </summary>
        /// <returns>Verification result</returns>
        [HttpGet("verify")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> VerifyBlockchain()
        {
            try
            {
                _logger.LogInformation("Verifying blockchain integrity");

                var isValid = await _blockchainService.VerifyBlockchainIntegrityAsync();

                return Ok(new
                {
                    success = true,
                    isValid = isValid,
                    message = isValid 
                        ? "Blockchain integrity verified successfully" 
                        : "Blockchain integrity verification failed",
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying blockchain");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get blockchain statistics
        /// </summary>
        /// <returns>Statistics object</returns>
        [HttpGet("stats")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                _logger.LogInformation("Getting blockchain statistics");

                var stats = await _blockchainService.GetBlockchainStatsAsync();

                return Ok(new
                {
                    success = true,
                    stats = stats,
                    message = "Blockchain statistics retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting blockchain statistics");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get blocks by booking ID
        /// </summary>
        /// <param name="bookingId">Booking ID</param>
        /// <returns>List of blocks for the booking</returns>
        [HttpGet("blocks/booking/{bookingId}")]
        [ProducesResponseType(typeof(List<Block>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetBlocksByBookingId(int bookingId)
        {
            try
            {
                if (bookingId <= 0)
                {
                    return BadRequest(new { message = "Invalid booking ID" });
                }

                _logger.LogInformation($"Getting blocks for booking {bookingId}");

                var blockchain = await _blockchainService.GetBlockchainAsync();
                var bookingBlocks = blockchain
                    .Where(b => b.BookingId == bookingId)
                    .OrderBy(b => b.Index)
                    .ToList();

                return Ok(new
                {
                    success = true,
                    bookingId = bookingId,
                    totalBlocks = bookingBlocks.Count,
                    blocks = bookingBlocks,
                    message = $"Found {bookingBlocks.Count} blocks for booking {bookingId}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting blocks for booking {bookingId}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get recent blocks
        /// </summary>
        /// <param name="count">Number of recent blocks (default: 10)</param>
        /// <returns>List of recent blocks</returns>
        [HttpGet("blocks/recent")]
        [ProducesResponseType(typeof(List<Block>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetRecentBlocks([FromQuery] int count = 10)
        {
            try
            {
                if (count <= 0 || count > 100)
                {
                    return BadRequest(new { message = "Count must be between 1 and 100" });
                }

                _logger.LogInformation($"Getting {count} recent blocks");

                var blockchain = await _blockchainService.GetBlockchainAsync();
                var recentBlocks = blockchain
                    .OrderByDescending(b => b.Index)
                    .Take(count)
                    .ToList();

                return Ok(new
                {
                    success = true,
                    count = recentBlocks.Count,
                    blocks = recentBlocks,
                    message = $"Retrieved {recentBlocks.Count} recent blocks"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent blocks");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get blockchain health status
        /// </summary>
        /// <returns>Health status</returns>
        [HttpGet("health")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetHealth()
        {
            try
            {
                _logger.LogInformation("Checking blockchain health");

                var isValid = await _blockchainService.VerifyBlockchainIntegrityAsync();
                var stats = await _blockchainService.GetBlockchainStatsAsync();

                return Ok(new
                {
                    success = true,
                    healthy = isValid,
                    totalBlocks = stats["totalBlocks"],
                    lastBlockHash = stats["lastBlockHash"],
                    isValid = stats["isValid"],
                    difficulty = stats["difficulty"],
                    message = isValid ? "Blockchain is healthy" : "Blockchain integrity issues detected",
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking blockchain health");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }

    /// <summary>
    /// Request model for creating booking block
    /// </summary>
    public class CreateBookingBlockRequest
    {
        public int BookingId { get; set; }
        public decimal Amount { get; set; }
        public string Details { get; set; }
    }
}
