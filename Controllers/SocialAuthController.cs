using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WEBDULICH.Services.Auth;

namespace WEBDULICH.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SocialAuthController : ControllerBase
    {
        private readonly ISocialAuthService _socialAuthService;
        private readonly ILogger<SocialAuthController> _logger;

        public SocialAuthController(
            ISocialAuthService socialAuthService,
            ILogger<SocialAuthController> logger)
        {
            _socialAuthService = socialAuthService;
            _logger = logger;
        }

        [HttpPost("google")]
        public async Task<IActionResult> GoogleSignIn([FromBody] GoogleSignInRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.IdToken))
                {
                    return BadRequest(new { success = false, message = "ID token is required" });
                }

                var result = await _socialAuthService.AuthenticateWithGoogleAsync(request.IdToken);
                
                if (result.Success)
                {
                    return Ok(new 
                    { 
                        success = true, 
                        data = new
                        {
                            userId = result.UserId,
                            email = result.Email,
                            name = result.Name,
                            profilePicture = result.ProfilePicture,
                            isNewUser = result.IsNewUser
                        }
                    });
                }

                return BadRequest(new { success = false, message = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Google sign-in");
                return StatusCode(500, new { success = false, message = "Authentication failed" });
            }
        }

        [HttpPost("facebook")]
        public async Task<IActionResult> FacebookLogin([FromBody] FacebookLoginRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.AccessToken))
                {
                    return BadRequest(new { success = false, message = "Access token is required" });
                }

                var result = await _socialAuthService.AuthenticateWithFacebookAsync(request.AccessToken);
                
                if (result.Success)
                {
                    return Ok(new 
                    { 
                        success = true, 
                        data = new
                        {
                            userId = result.UserId,
                            email = result.Email,
                            name = result.Name,
                            profilePicture = result.ProfilePicture,
                            isNewUser = result.IsNewUser
                        }
                    });
                }

                return BadRequest(new { success = false, message = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Facebook login");
                return StatusCode(500, new { success = false, message = "Authentication failed" });
            }
        }

        [HttpPost("apple")]
        public async Task<IActionResult> AppleSignIn([FromBody] AppleSignInRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.IdentityToken))
                {
                    return BadRequest(new { success = false, message = "Identity token is required" });
                }

                var result = await _socialAuthService.AuthenticateWithAppleAsync(request.IdentityToken);
                
                if (result.Success)
                {
                    return Ok(new 
                    { 
                        success = true, 
                        data = new
                        {
                            userId = result.UserId,
                            email = result.Email,
                            name = result.Name,
                            isNewUser = result.IsNewUser
                        }
                    });
                }

                return BadRequest(new { success = false, message = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Apple sign-in");
                return StatusCode(500, new { success = false, message = "Authentication failed" });
            }
        }

        [HttpPost("link")]
        [Authorize]
        public async Task<IActionResult> LinkSocialAccount([FromBody] LinkAccountRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { success = false, message = "Invalid user" });
                }

                var success = await _socialAuthService.LinkSocialAccountAsync(
                    userId,
                    request.Provider,
                    request.SocialId
                );

                if (success)
                {
                    return Ok(new { success = true, message = "Account linked successfully" });
                }

                return BadRequest(new { success = false, message = "Failed to link account" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error linking social account");
                return StatusCode(500, new { success = false, message = "Failed to link account" });
            }
        }

        [HttpDelete("unlink/{provider}")]
        [Authorize]
        public async Task<IActionResult> UnlinkSocialAccount(string provider)
        {
            try
            {
                var userIdClaim = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { success = false, message = "Invalid user" });
                }

                if (!Enum.TryParse<SocialProvider>(provider, true, out var socialProvider))
                {
                    return BadRequest(new { success = false, message = "Invalid provider" });
                }

                var success = await _socialAuthService.UnlinkSocialAccountAsync(userId, socialProvider);

                if (success)
                {
                    return Ok(new { success = true, message = "Account unlinked successfully" });
                }

                return BadRequest(new { success = false, message = "Failed to unlink account" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unlinking social account");
                return StatusCode(500, new { success = false, message = "Failed to unlink account" });
            }
        }
    }

    public class GoogleSignInRequest
    {
        public string IdToken { get; set; } = string.Empty;
    }

    public class FacebookLoginRequest
    {
        public string AccessToken { get; set; } = string.Empty;
    }

    public class AppleSignInRequest
    {
        public string IdentityToken { get; set; } = string.Empty;
    }

    public class LinkAccountRequest
    {
        public SocialProvider Provider { get; set; }
        public string SocialId { get; set; } = string.Empty;
    }
}
