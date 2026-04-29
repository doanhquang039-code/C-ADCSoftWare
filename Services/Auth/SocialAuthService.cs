using Microsoft.EntityFrameworkCore;
using WEBDULICH.Models;
using System.Net.Http;
using System.Text.Json;

namespace WEBDULICH.Services.Auth
{
    public class SocialAuthService : ISocialAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<SocialAuthService> _logger;

        public SocialAuthService(
            ApplicationDbContext context,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<SocialAuthService> logger)
        {
            _context = context;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<SocialAuthResult> AuthenticateWithGoogleAsync(string idToken)
        {
            try
            {
                // Verify Google ID token
                var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.GetAsync($"https://oauth2.googleapis.com/tokeninfo?id_token={idToken}");
                
                if (!response.IsSuccessStatusCode)
                {
                    return new SocialAuthResult
                    {
                        Success = false,
                        ErrorMessage = "Invalid Google token"
                    };
                }

                var content = await response.Content.ReadAsStringAsync();
                var tokenInfo = JsonSerializer.Deserialize<GoogleTokenInfo>(content);

                if (tokenInfo == null || string.IsNullOrEmpty(tokenInfo.Email))
                {
                    return new SocialAuthResult
                    {
                        Success = false,
                        ErrorMessage = "Unable to retrieve user information from Google"
                    };
                }

                // Verify audience (client ID)
                var clientId = _configuration["GoogleAuth:ClientId"];
                if (tokenInfo.Aud != clientId)
                {
                    return new SocialAuthResult
                    {
                        Success = false,
                        ErrorMessage = "Invalid token audience"
                    };
                }

                var userInfo = new SocialUserInfo
                {
                    SocialId = tokenInfo.Sub,
                    Email = tokenInfo.Email,
                    Name = tokenInfo.Name ?? "",
                    ProfilePicture = tokenInfo.Picture,
                    EmailVerified = tokenInfo.EmailVerified,
                    Provider = SocialProvider.Google
                };

                return await ProcessSocialAuthAsync(userInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error authenticating with Google");
                return new SocialAuthResult
                {
                    Success = false,
                    ErrorMessage = "Authentication failed"
                };
            }
        }

        public async Task<SocialAuthResult> AuthenticateWithFacebookAsync(string accessToken)
        {
            try
            {
                // Verify Facebook access token
                var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.GetAsync(
                    $"https://graph.facebook.com/me?fields=id,name,email,picture&access_token={accessToken}");
                
                if (!response.IsSuccessStatusCode)
                {
                    return new SocialAuthResult
                    {
                        Success = false,
                        ErrorMessage = "Invalid Facebook token"
                    };
                }

                var content = await response.Content.ReadAsStringAsync();
                var fbUser = JsonSerializer.Deserialize<FacebookUserInfo>(content);

                if (fbUser == null || string.IsNullOrEmpty(fbUser.Email))
                {
                    return new SocialAuthResult
                    {
                        Success = false,
                        ErrorMessage = "Unable to retrieve user information from Facebook"
                    };
                }

                var userInfo = new SocialUserInfo
                {
                    SocialId = fbUser.Id,
                    Email = fbUser.Email,
                    Name = fbUser.Name ?? "",
                    ProfilePicture = fbUser.Picture?.Data?.Url,
                    EmailVerified = true, // Facebook emails are verified
                    Provider = SocialProvider.Facebook
                };

                return await ProcessSocialAuthAsync(userInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error authenticating with Facebook");
                return new SocialAuthResult
                {
                    Success = false,
                    ErrorMessage = "Authentication failed"
                };
            }
        }

        public async Task<SocialAuthResult> AuthenticateWithAppleAsync(string identityToken)
        {
            try
            {
                // Apple Sign-In verification is more complex
                // For now, return a placeholder implementation
                // In production, verify the JWT token with Apple's public keys
                
                _logger.LogWarning("Apple Sign-In not fully implemented yet");
                
                return new SocialAuthResult
                {
                    Success = false,
                    ErrorMessage = "Apple Sign-In not available yet"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error authenticating with Apple");
                return new SocialAuthResult
                {
                    Success = false,
                    ErrorMessage = "Authentication failed"
                };
            }
        }

        public async Task<bool> LinkSocialAccountAsync(int userId, SocialProvider provider, string socialId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return false;
                }

                // Check if social account is already linked to another user
                var existingLink = await _context.Users
                    .AnyAsync(u => u.Id != userId && 
                                   GetSocialId(u, provider) == socialId);

                if (existingLink)
                {
                    _logger.LogWarning($"Social account {provider}:{socialId} already linked to another user");
                    return false;
                }

                // Link the social account
                SetSocialId(user, provider, socialId);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Linked {provider} account to user {userId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error linking social account for user {userId}");
                return false;
            }
        }

        public async Task<bool> UnlinkSocialAccountAsync(int userId, SocialProvider provider)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return false;
                }

                // Unlink the social account
                SetSocialId(user, provider, null);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Unlinked {provider} account from user {userId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error unlinking social account for user {userId}");
                return false;
            }
        }

        private async Task<SocialAuthResult> ProcessSocialAuthAsync(SocialUserInfo userInfo)
        {
            // Check if user exists by email
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == userInfo.Email);

            bool isNewUser = false;

            if (user == null)
            {
                // Create new user
                user = new User
                {
                    Email = userInfo.Email,
                    FullName = userInfo.Name,
                    IsEmailVerified = userInfo.EmailVerified,
                    CreatedAt = DateTime.UtcNow,
                    Role = "Customer"
                };

                SetSocialId(user, userInfo.Provider, userInfo.SocialId);

                if (!string.IsNullOrEmpty(userInfo.ProfilePicture))
                {
                    user.ProfilePicture = userInfo.ProfilePicture;
                }

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                isNewUser = true;
                _logger.LogInformation($"Created new user from {userInfo.Provider}: {user.Email}");
            }
            else
            {
                // Update social ID if not set
                var existingSocialId = GetSocialId(user, userInfo.Provider);
                if (string.IsNullOrEmpty(existingSocialId))
                {
                    SetSocialId(user, userInfo.Provider, userInfo.SocialId);
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation($"User logged in with {userInfo.Provider}: {user.Email}");
            }

            return new SocialAuthResult
            {
                Success = true,
                UserId = user.Id.ToString(),
                Email = user.Email,
                Name = user.FullName,
                ProfilePicture = user.ProfilePicture,
                Provider = userInfo.Provider,
                SocialId = userInfo.SocialId,
                IsNewUser = isNewUser
            };
        }

        private string? GetSocialId(User user, SocialProvider provider)
        {
            return provider switch
            {
                SocialProvider.Google => user.GoogleId,
                SocialProvider.Facebook => user.FacebookId,
                SocialProvider.Apple => user.AppleId,
                _ => null
            };
        }

        private void SetSocialId(User user, SocialProvider provider, string? socialId)
        {
            switch (provider)
            {
                case SocialProvider.Google:
                    user.GoogleId = socialId;
                    break;
                case SocialProvider.Facebook:
                    user.FacebookId = socialId;
                    break;
                case SocialProvider.Apple:
                    user.AppleId = socialId;
                    break;
            }
        }

        // Helper classes for deserializing provider responses
        private class GoogleTokenInfo
        {
            public string Sub { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string? Name { get; set; }
            public string? Picture { get; set; }
            public bool EmailVerified { get; set; }
            public string Aud { get; set; } = string.Empty;
        }

        private class FacebookUserInfo
        {
            public string Id { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string? Name { get; set; }
            public FacebookPicture? Picture { get; set; }
        }

        private class FacebookPicture
        {
            public FacebookPictureData? Data { get; set; }
        }

        private class FacebookPictureData
        {
            public string? Url { get; set; }
        }
    }
}
