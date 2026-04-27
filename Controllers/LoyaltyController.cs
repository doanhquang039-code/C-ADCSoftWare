using Microsoft.AspNetCore.Mvc;
using WEBDULICH.Models;
using WEBDULICH.Services;

namespace WEBDULICH.Controllers
{
    public class LoyaltyController : Controller
    {
        private readonly ILoyaltyService _loyaltyService;
        private readonly ICurrentUserService _currentUserService;

        public LoyaltyController(ILoyaltyService loyaltyService, ICurrentUserService currentUserService)
        {
            _loyaltyService = loyaltyService;
            _currentUserService = currentUserService;
        }

        public async Task<IActionResult> Index()
        {
            var stats = await _loyaltyService.GetLoyaltyStatsAsync();
            return View(stats);
        }

        public async Task<IActionResult> MyAccount()
        {
            var currentUser = _currentUserService.GetCurrentUser();
            if (currentUser == null)
            {
                return RedirectToAction("Login", "User");
            }

            var account = await _loyaltyService.GetOrCreateAccountAsync(currentUser.Id);
            var transactions = await _loyaltyService.GetTransactionHistoryAsync(currentUser.Id);
            var redemptions = await _loyaltyService.GetUserRedemptionsAsync(currentUser.Id);
            var availableRewards = await _loyaltyService.GetAvailableRewardsAsync(currentUser.Id);

            ViewBag.Transactions = transactions;
            ViewBag.Redemptions = redemptions;
            ViewBag.AvailableRewards = availableRewards;

            return View(account);
        }

        public async Task<IActionResult> Rewards()
        {
            var currentUser = _currentUserService.GetCurrentUser();
            if (currentUser == null)
            {
                return RedirectToAction("Login", "User");
            }

            var rewards = await _loyaltyService.GetAvailableRewardsAsync(currentUser.Id);
            return View(rewards);
        }

        [HttpPost]
        public async Task<IActionResult> RedeemReward(int rewardId)
        {
            var currentUser = _currentUserService.GetCurrentUser();
            if (currentUser == null)
            {
                return Json(new { success = false, message = "Please login first" });
            }

            try
            {
                var redemption = await _loyaltyService.RedeemRewardAsync(currentUser.Id, rewardId);
                if (redemption != null)
                {
                    return Json(new { 
                        success = true, 
                        message = "Reward redeemed successfully!", 
                        redemptionCode = redemption.RedemptionCode 
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Unable to redeem reward" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Admin functions
        public async Task<IActionResult> ManageRewards()
        {
            var rewards = await _loyaltyService.GetAllRewardsAsync();
            return View(rewards);
        }

        public IActionResult CreateReward()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateReward(Reward reward)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _loyaltyService.CreateRewardAsync(reward);
                    TempData["Success"] = "Reward created successfully!";
                    return RedirectToAction("ManageRewards");
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error creating reward: {ex.Message}";
                }
            }

            return View(reward);
        }

        public async Task<IActionResult> Tiers()
        {
            var tiers = await _loyaltyService.GetTiersAsync();
            return View(tiers);
        }

        public async Task<IActionResult> Rules()
        {
            var rules = await _loyaltyService.GetPointsRulesAsync();
            return View(rules);
        }

        [HttpPost]
        public async Task<IActionResult> AwardPoints(int userId, int points, string description)
        {
            try
            {
                await _loyaltyService.AwardPointsAsync(userId, points, "Manual", description);
                TempData["Success"] = "Points awarded successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error awarding points: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
    }
}