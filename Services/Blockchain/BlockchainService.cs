using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace WEBDULICH.Services.Blockchain
{
    /// <summary>
    /// Blockchain Service for Secure Booking & Payment Verification
    /// Hệ thống blockchain để bảo mật booking và xác thực thanh toán
    /// </summary>
    public interface IBlockchainService
    {
        Task<Block> CreateBookingBlockAsync(int bookingId, decimal amount, string details);
        Task<bool> VerifyBlockchainIntegrityAsync();
        Task<List<Block>> GetBlockchainAsync();
        Task<Block> GetBlockByHashAsync(string hash);
        Task<Dictionary<string, object>> GetBlockchainStatsAsync();
    }

    public class Block
    {
        public int Index { get; set; }
        public DateTime Timestamp { get; set; }
        public string Data { get; set; }
        public string PreviousHash { get; set; }
        public string Hash { get; set; }
        public int Nonce { get; set; }
        public string BlockType { get; set; } // "BOOKING", "PAYMENT", "REFUND"
        public int? BookingId { get; set; }
        public decimal? Amount { get; set; }
    }

    public class BlockchainService : IBlockchainService
    {
        private readonly List<Block> _blockchain;
        private readonly ILogger<BlockchainService> _logger;
        private readonly int _difficulty = 4; // Mining difficulty

        public BlockchainService(ILogger<BlockchainService> logger)
        {
            _logger = logger;
            _blockchain = new List<Block>();
            
            // Create genesis block
            CreateGenesisBlock();
        }

        private void CreateGenesisBlock()
        {
            var genesisBlock = new Block
            {
                Index = 0,
                Timestamp = DateTime.Now,
                Data = "Genesis Block - WEBDULICH Blockchain",
                PreviousHash = "0",
                BlockType = "GENESIS",
                Nonce = 0
            };

            genesisBlock.Hash = CalculateHash(genesisBlock);
            _blockchain.Add(genesisBlock);

            _logger.LogInformation("Genesis block created");
        }

        public async Task<Block> CreateBookingBlockAsync(int bookingId, decimal amount, string details)
        {
            var previousBlock = _blockchain.Last();

            var newBlock = new Block
            {
                Index = _blockchain.Count,
                Timestamp = DateTime.Now,
                Data = JsonSerializer.Serialize(new
                {
                    bookingId = bookingId,
                    amount = amount,
                    details = details,
                    timestamp = DateTime.Now
                }),
                PreviousHash = previousBlock.Hash,
                BlockType = "BOOKING",
                BookingId = bookingId,
                Amount = amount,
                Nonce = 0
            };

            // Mine the block (Proof of Work)
            await MineBlockAsync(newBlock);

            _blockchain.Add(newBlock);

            _logger.LogInformation($"Block {newBlock.Index} created for booking {bookingId}");

            return newBlock;
        }

        private async Task MineBlockAsync(Block block)
        {
            var target = new string('0', _difficulty);

            while (!block.Hash?.StartsWith(target) ?? true)
            {
                block.Nonce++;
                block.Hash = CalculateHash(block);

                // Simulate async mining
                if (block.Nonce % 1000 == 0)
                {
                    await Task.Delay(1);
                }
            }

            _logger.LogInformation($"Block mined: {block.Hash} (Nonce: {block.Nonce})");
        }

        private string CalculateHash(Block block)
        {
            var data = $"{block.Index}{block.Timestamp}{block.Data}{block.PreviousHash}{block.Nonce}";
            
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
                return Convert.ToHexString(hashBytes).ToLower();
            }
        }

        public async Task<bool> VerifyBlockchainIntegrityAsync()
        {
            for (int i = 1; i < _blockchain.Count; i++)
            {
                var currentBlock = _blockchain[i];
                var previousBlock = _blockchain[i - 1];

                // Verify hash
                var calculatedHash = CalculateHash(currentBlock);
                if (currentBlock.Hash != calculatedHash)
                {
                    _logger.LogWarning($"Block {i} has invalid hash");
                    return false;
                }

                // Verify chain link
                if (currentBlock.PreviousHash != previousBlock.Hash)
                {
                    _logger.LogWarning($"Block {i} has invalid previous hash");
                    return false;
                }

                // Verify proof of work
                var target = new string('0', _difficulty);
                if (!currentBlock.Hash.StartsWith(target))
                {
                    _logger.LogWarning($"Block {i} has invalid proof of work");
                    return false;
                }
            }

            _logger.LogInformation("Blockchain integrity verified");
            return await Task.FromResult(true);
        }

        public async Task<List<Block>> GetBlockchainAsync()
        {
            return await Task.FromResult(_blockchain.ToList());
        }

        public async Task<Block> GetBlockByHashAsync(string hash)
        {
            return await Task.FromResult(_blockchain.FirstOrDefault(b => b.Hash == hash));
        }

        public async Task<Dictionary<string, object>> GetBlockchainStatsAsync()
        {
            var bookingBlocks = _blockchain.Where(b => b.BlockType == "BOOKING").ToList();
            var totalAmount = bookingBlocks.Sum(b => b.Amount ?? 0);

            return await Task.FromResult(new Dictionary<string, object>
            {
                ["totalBlocks"] = _blockchain.Count,
                ["bookingBlocks"] = bookingBlocks.Count,
                ["totalAmount"] = totalAmount,
                ["lastBlockHash"] = _blockchain.Last().Hash,
                ["isValid"] = await VerifyBlockchainIntegrityAsync(),
                ["difficulty"] = _difficulty
            });
        }
    }

    /// <summary>
    /// Smart Contract for Automated Refunds
    /// </summary>
    public class SmartContract
    {
        public int Id { get; set; }
        public string ContractType { get; set; } // "REFUND", "CANCELLATION", "INSURANCE"
        public Dictionary<string, object> Conditions { get; set; }
        public string Status { get; set; } // "ACTIVE", "EXECUTED", "EXPIRED"
        public DateTime CreatedAt { get; set; }
        public DateTime? ExecutedAt { get; set; }

        public bool EvaluateConditions(Dictionary<string, object> currentState)
        {
            // Evaluate smart contract conditions
            foreach (var condition in Conditions)
            {
                if (!currentState.ContainsKey(condition.Key))
                    return false;

                if (!currentState[condition.Key].Equals(condition.Value))
                    return false;
            }

            return true;
        }
    }
}
