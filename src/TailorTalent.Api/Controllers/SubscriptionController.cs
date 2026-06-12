using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TailorTalent.Api.Data;
using TailorTalent.Api.Models.Subscription;
using TailorTalent.Api.Services;

namespace TailorTalent.Api.Controllers;

[ApiController]
[Route("api/subscription")]
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
    /// Get the subscription status and credit balance for a user.
    /// </summary>
    [HttpGet("status")]
    public async Task<ActionResult<SubscriptionStatusDto>> GetStatus([FromQuery] string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest("userId is required.");

        var status = await _subscriptionService.GetStatusAsync(userId);
        return Ok(status);
    }

    /// <summary>
    /// Initialize a new user with default free-tier subscription and credits.
    /// </summary>
    [HttpPost("init")]
    public async Task<ActionResult<SubscriptionStatusDto>> InitializeUser([FromQuery] string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest("userId is required.");

        await _subscriptionService.InitializeUserAsync(userId);
        var status = await _subscriptionService.GetStatusAsync(userId);
        return Ok(status);
    }

    /// <summary>
    /// Purchase additional credits (simulated).
    /// </summary>
    [HttpPost("purchase-credits")]
    public async Task<ActionResult<object>> PurchaseCredits([FromQuery] string userId, [FromQuery] int amount = 10)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest("userId is required.");

        var credits = await _subscriptionService.AddCreditsAsync(userId, amount, $"Purchased {amount} credits");
        return Ok(new { userId, creditsRemaining = credits.CreditsRemaining, amountPurchased = amount });
    }

    /// <summary>
    /// Upgrade a user's subscription plan (simulated).
    /// </summary>
    [HttpPost("upgrade")]
    public async Task<ActionResult<object>> Upgrade([FromQuery] string userId, [FromQuery] string plan = "Premium")
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest("userId is required.");

        if (!Enum.TryParse<SubscriptionPlan>(plan, true, out var parsedPlan))
            return BadRequest($"Invalid plan. Use: {string.Join(", ", Enum.GetNames<SubscriptionPlan>())}");

        var sub = await _subscriptionService.UpgradeSubscriptionAsync(userId, parsedPlan);
        return Ok(new { userId, plan = sub.Plan.ToString(), isActive = sub.IsActive, expiresAt = sub.EndDate });
    }

    /// <summary>
    /// Get the transaction history for a user.
    /// </summary>
    [HttpGet("transactions")]
    public async Task<ActionResult> GetTransactions([FromQuery] string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest("userId is required.");

        var transactions = await _db.CreditTransactions
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Take(50)
            .ToListAsync();

        return Ok(transactions);
    }
}