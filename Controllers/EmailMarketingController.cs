using Microsoft.AspNetCore.Mvc;
using WEBDULICH.Models;
using WEBDULICH.Services;

namespace WEBDULICH.Controllers
{
    public class EmailMarketingController : Controller
    {
        private readonly IEmailMarketingService _emailMarketingService;

        public EmailMarketingController(IEmailMarketingService emailMarketingService)
        {
            _emailMarketingService = emailMarketingService;
        }

        public async Task<IActionResult> Index()
        {
            var stats = await _emailMarketingService.GetEmailStatsAsync();
            return View(stats);
        }

        public async Task<IActionResult> Campaigns()
        {
            var campaigns = await _emailMarketingService.GetCampaignsAsync();
            return View(campaigns);
        }

        public IActionResult CreateCampaign()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateCampaign(EmailCampaign campaign)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _emailMarketingService.CreateCampaignAsync(campaign);
                    TempData["Success"] = "Campaign created successfully!";
                    return RedirectToAction("Campaigns");
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error creating campaign: {ex.Message}";
                }
            }

            return View(campaign);
        }

        public async Task<IActionResult> Subscribers()
        {
            var subscribers = await _emailMarketingService.GetSubscribersAsync();
            return View(subscribers);
        }

        [HttpPost]
        public async Task<IActionResult> Subscribe(string email, string firstName = null, string lastName = null)
        {
            try
            {
                await _emailMarketingService.SubscribeAsync(email, firstName, lastName, "Website");
                return Json(new { success = true, message = "Subscribed successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SendCampaign(int id)
        {
            try
            {
                var result = await _emailMarketingService.SendCampaignAsync(id);
                if (result)
                {
                    TempData["Success"] = "Campaign sent successfully!";
                }
                else
                {
                    TempData["Error"] = "Failed to send campaign.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error sending campaign: {ex.Message}";
            }

            return RedirectToAction("Campaigns");
        }

        public async Task<IActionResult> Track(string token)
        {
            await _emailMarketingService.TrackEmailOpenAsync(token);
            
            // Return 1x1 transparent pixel
            var pixel = Convert.FromBase64String("R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7");
            return File(pixel, "image/gif");
        }
    }
}