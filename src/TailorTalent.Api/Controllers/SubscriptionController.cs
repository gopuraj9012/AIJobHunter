using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TailorTalent.Api.Data;
using TailorTalent.Api.Models.Subscription;
using TailorTalent.Api.Services;

namespace TailorTalent.Api.Controllers;

[ApiController]
[Route("api/subscription")]
[Authorize]
public class SubscriptionController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly TailorTalentDbContext _db;

    public SubscriptionController(ISubscriptionService subscriptionService, TailorTalentDbContext db)
    {
        _subscriptionService = subscriptionService;
        _db = db;
    }

    /// <summary>
    /// Get the subscription status and credit balance for the authenticated user.
    /// </summary>
    [HttpGet("status")]
    public async Task<ActionResult<SubscriptionStatusDto>> GetStatus()
    {
        var userId = User.GetUserId()!;
        var status = await _subscriptionService.GetStatusAsync(userId);
        return Ok(status);
    }

    /// <summary>
    /// Initialize the authenticated user with default free-tier subscription and credits.
    /// </summary>
    [HttpPost("init")]
    public async Task<ActionResult<SubscriptionStatusDto>> InitializeUser()
    {
        var userId = User.GetUserId()!;
        await _subscriptionService.InitializeUserAsync(userId);
        var status = await _subscriptionService.GetStatusAsync(userId);
        return Ok(status);
    }

    /// <summary>
    /// Purchase additional credits for the authenticated user (simulated).
    /// </summary>
    [HttpPost("purchase-credits")]
    public async Task<ActionResult<object>> PurchaseCredits([FromQuery] int amount = 10)
    {
        var userId = User.GetUserId()!;
        var credits = await _subscriptionService.AddCreditsAsync(userId, amount, $"Purchased {amount} credits");
        return Ok(new { userId, creditsRemaining = credits.CreditsRemaining, amountPurchased = amount });
    }

    /// <summary>
    /// Upgrade the authenticated user's subscription plan (simulated).
    /// </summary>
    [HttpPost("upgrade")]
    public async Task<ActionResult<object>> Upgrade([FromQuery] string plan = "Premium")
    {
        var userId = User.GetUserId()!;

        if (!Enum.TryParse<SubscriptionPlan>(plan, true, out var parsedPlan))
            return BadRequest($"Invalid plan. Use: {string.Join(", ", Enum.GetNames<SubscriptionPlan>())}");

        var sub = await _subscriptionService.UpgradeSubscriptionAsync(userId, parsedPlan);
        return Ok(new { userId, plan = sub.Plan.ToString(), isActive = sub.IsActive, expiresAt = sub.EndDate });
    }

    /// <summary>
    /// Get the transaction history for the authenticated user.
    /// </summary>
    [HttpGet("transactions")]
    public async Task<ActionResult> GetTransactions()
    {
        var userId = User.GetUserId()!;
        var transactions = await _db.CreditTransactions
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Take(50)
            .ToListAsync();

        return Ok(transactions);
    }
}