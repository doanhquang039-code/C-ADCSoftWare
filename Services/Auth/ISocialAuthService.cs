namespace WEBDULICH.Services.Auth
{
    public interface ISocialAuthService
    {
        Task<SocialAuthResult> AuthenticateWithGoogleAsync(string idToken);
        Task<SocialAuthResult> AuthenticateWithFacebookAsync(string accessToken);
        Task<SocialAuthResult> AuthenticateWithAppleAsync(string identityToken);
        Task<bool> LinkSocialAccountAsync(int userId, SocialProvider provider, string socialId);
        Task<bool> UnlinkSocialAccountAsync(int userId, SocialProvider provider);
    }

    public class SocialAuthResult
    {
        public bool Success { get; set; }
        public string? UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? ProfilePicture { get; set; }
        public SocialProvider Provider { get; set; }
        public string SocialId { get; set; } = string.Empty;
        public bool IsNewUser { get; set; }
        public string? ErrorMessage { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }

    public enum SocialProvider
    {
        Google,
        Facebook,
        Apple,
        Microsoft
    }

    public class SocialUserInfo
    {
        public string SocialId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ProfilePicture { get; set; }
        public bool EmailVerified { get; set; }
        public SocialProvider Provider { get; set; }
    }
}
