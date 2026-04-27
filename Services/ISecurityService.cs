namespace WEBDULICH.Services
{
    public interface ISecurityService
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
        string GenerateSecureToken();
        bool ValidatePasswordStrength(string password);
        Task<bool> IsAccountLockedAsync(string email);
        Task LockAccountAsync(string email, TimeSpan lockDuration);
        Task UnlockAccountAsync(string email);
        Task RecordLoginAttemptAsync(string email, bool success, string ipAddress);
        Task<bool> IsIpAddressBlockedAsync(string ipAddress);
        Task<string> GenerateTwoFactorCodeAsync(int userId);
        Task<bool> ValidateTwoFactorCodeAsync(int userId, string code);
        Task LogSecurityEventAsync(string eventType, int? userId, string details, string ipAddress);
    }
}