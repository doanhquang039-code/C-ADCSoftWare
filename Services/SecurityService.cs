using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using WEBDULICH.Models;

namespace WEBDULICH.Services
{
    public class SecurityService : ISecurityService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SecurityService> _logger;

        public SecurityService(ApplicationDbContext context, ILogger<SecurityService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var salt = GenerateSecureToken();
            var saltedPassword = password + salt;
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
            return Convert.ToBase64String(hashedBytes) + ":" + salt;
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                var parts = hashedPassword.Split(':');
                if (parts.Length != 2) return false;

                var hash = parts[0];
                var salt = parts[1];

                using var sha256 = SHA256.Create();
                var saltedPassword = password + salt;
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
                var computedHash = Convert.ToBase64String(hashedBytes);

                return hash == computedHash;
            }
            catch
            {
                return false;
            }
        }

        public string GenerateSecureToken()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[32];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        public bool ValidatePasswordStrength(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 8)
                return false;

            // At least one uppercase, one lowercase, one digit, one special character
            var hasUpper = Regex.IsMatch(password, @"[A-Z]");
            var hasLower = Regex.IsMatch(password, @"[a-z]");
            var hasDigit = Regex.IsMatch(password, @"\d");
            var hasSpecial = Regex.IsMatch(password, @"[!@#$%^&*(),.?""':;{}|<>]");

            return hasUpper && hasLower && hasDigit && hasSpecial;
        }

        public async Task<bool> IsAccountLockedAsync(string email)
        {
            var lockRecord = await _context.SecurityLogs
                .Where(s => s.Email == email && s.EventType == "AccountLocked" && s.ExpiresAt > DateTime.Now)
                .FirstOrDefaultAsync();

            return lockRecord != null;
        }

        public async Task LockAccountAsync(string email, TimeSpan lockDuration)
        {
            var lockRecord = new SecurityLog
            {
                Email = email,
                EventType = "AccountLocked",
                Details = $"Account locked for {lockDuration.TotalMinutes} minutes",
                IpAddress = "System",
                CreatedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.Add(lockDuration)
            };

            _context.SecurityLogs.Add(lockRecord);
            await _context.SaveChangesAsync();

            _logger.LogWarning($"Account {email} has been locked for {lockDuration.TotalMinutes} minutes");
        }

        public async Task UnlockAccountAsync(string email)
        {
            var lockRecords = await _context.SecurityLogs
                .Where(s => s.Email == email && s.EventType == "AccountLocked")
                .ToListAsync();

            foreach (var record in lockRecords)
            {
                record.ExpiresAt = DateTime.Now.AddMinutes(-1);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation($"Account {email} has been unlocked");
        }

        public async Task RecordLoginAttemptAsync(string email, bool success, string ipAddress)
        {
            var loginAttempt = new SecurityLog
            {
                Email = email,
                EventType = success ? "LoginSuccess" : "LoginFailed",
                Details = success ? "Successful login" : "Failed login attempt",
                IpAddress = ipAddress,
                CreatedAt = DateTime.Now
            };

            _context.SecurityLogs.Add(loginAttempt);

            // Check for multiple failed attempts
            if (!success)
            {
                var recentFailures = await _context.SecurityLogs
                    .Where(s => s.Email == email && s.EventType == "LoginFailed" && 
                               s.CreatedAt > DateTime.Now.AddMinutes(-15))
                    .CountAsync();

                if (recentFailures >= 5)
                {
                    await LockAccountAsync(email, TimeSpan.FromMinutes(30));
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsIpAddressBlockedAsync(string ipAddress)
        {
            var blockRecord = await _context.SecurityLogs
                .Where(s => s.IpAddress == ipAddress && s.EventType == "IpBlocked" && s.ExpiresAt > DateTime.Now)
                .FirstOrDefaultAsync();

            return blockRecord != null;
        }

        public async Task<string> GenerateTwoFactorCodeAsync(int userId)
        {
            var code = new Random().Next(100000, 999999).ToString();
            
            var twoFactorRecord = new SecurityLog
            {
                UserId = userId,
                EventType = "TwoFactorCode",
                Details = code,
                CreatedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddMinutes(5)
            };

            _context.SecurityLogs.Add(twoFactorRecord);
            await _context.SaveChangesAsync();

            return code;
        }

        public async Task<bool> ValidateTwoFactorCodeAsync(int userId, string code)
        {
            var record = await _context.SecurityLogs
                .Where(s => s.UserId == userId && s.EventType == "TwoFactorCode" && 
                           s.Details == code && s.ExpiresAt > DateTime.Now)
                .FirstOrDefaultAsync();

            if (record != null)
            {
                record.ExpiresAt = DateTime.Now.AddMinutes(-1); // Invalidate the code
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task LogSecurityEventAsync(string eventType, int? userId, string details, string ipAddress)
        {
            var securityLog = new SecurityLog
            {
                UserId = userId,
                EventType = eventType,
                Details = details,
                IpAddress = ipAddress,
                CreatedAt = DateTime.Now
            };

            _context.SecurityLogs.Add(securityLog);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Security event logged: {eventType} - {details}");
        }
    }
}