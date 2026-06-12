using Microsoft.EntityFrameworkCore;
using TailorTalent.Api.Data;
using TailorTalent.Api.Models.Subscription;

namespace TailorTalent.Api.Services;

public interface ISubscriptionService
{
    Task<SubscriptionStatusDto> GetStatusAsync(string userId);
    Task<UserCredits> InitializeUserAsync(string userId);
    Task<bool> DeductCreditAsync(string userId, string description, Guid? sessionId = null);
    Task<UserCredits> AddCreditsAsync(string userId, int amount, string description);
    Task<UserSubscription> UpgradeSubscriptionAsync(string userId, SubscriptionPlan plan);
}

public class SubscriptionStatusDto
{
    public string UserId { get; set; } = string.Empty;
    public SubscriptionPlan Plan { get; set; }
    public bool IsActive { get; set; }
    public int CreditsRemaining { get; set; }
    public bool CanTailor { get; set; }
    public string PlanName { get; set; } = string.Empty;
}

public class SubscriptionService : ISubscriptionService
{
    private readonly TailorTalentDbContext _db;
    private readonly ILogger<SubscriptionService> _logger;

    public SubscriptionService(TailorTalentDbContext db, ILogger<SubscriptionService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<SubscriptionStatusDto> GetStatusAsync(string userId)
    {
        var sub = await _db.UserSubscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId);

        var credits = await _db.UserCredits
            .FirstOrDefaultAsync(c => c.UserId == userId);

        var plan = sub?.Plan ?? SubscriptionPlan.Free;
        var isActive = sub?.IsActive ?? true;
        var remaining = credits?.CreditsRemaining ?? 0;

        bool canTailor = plan switch
        {
            SubscriptionPlan.Premium => isActive,
            SubscriptionPlan.PayPerTailor => remaining > 0,
            _ => false // Free tier cannot tailor
        };

        return new SubscriptionStatusDto
        {
            UserId = userId,
            Plan = plan,
            IsActive = isActive,
            CreditsRemaining = remaining,
            CanTailor = canTailor,
            PlanName = plan.ToString()
        };
    }

    public async Task<UserCredits> InitializeUserAsync(string userId)
    {
        // Check if already exists
        var existing = await _db.UserCredits.FirstOrDefaultAsync(c => c.UserId == userId);
        if (existing != null) return existing;

        var sub = await _db.UserSubscriptions.FirstOrDefaultAsync(s => s.UserId == userId);
        if (sub == null)
        {
            _db.UserSubscriptions.Add(new UserSubscription
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Plan = SubscriptionPlan.Free,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        var credits = new UserCredits
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreditsRemaining = 3, // Free tier gets 3 credits
            TotalCreditsPurchased = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.UserCredits.Add(credits);

        _db.CreditTransactions.Add(new CreditTransaction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Amount = 3,
            Description = "Free tier welcome credits",
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return credits;
    }

    public async Task<bool> DeductCreditAsync(string userId, string description, Guid? sessionId = null)
    {
        var credits = await _db.UserCredits.FirstOrDefaultAsync(c => c.UserId == userId);
        if (credits == null)
        {
            credits = await InitializeUserAsync(userId);
        }

        var sub = await _db.UserSubscriptions.FirstOrDefaultAsync(s => s.UserId == userId);
        if (sub?.Plan == SubscriptionPlan.Premium && sub.IsActive)
        {
            // Premium users have unlimited access, no deduction needed
            return true;
        }

        if (credits.CreditsRemaining <= 0)
            return false;

        credits.CreditsRemaining--;
        credits.UpdatedAt = DateTime.UtcNow;

        _db.CreditTransactions.Add(new CreditTransaction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Amount = -1,
            Description = description,
            TailoringSessionId = sessionId,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<UserCredits> AddCreditsAsync(string userId, int amount, string description)
    {
        var credits = await _db.UserCredits.FirstOrDefaultAsync(c => c.UserId == userId);
        if (credits == null)
        {
            credits = await InitializeUserAsync(userId);
        }

        credits.CreditsRemaining += amount;
        credits.TotalCreditsPurchased += amount;
        credits.UpdatedAt = DateTime.UtcNow;

        _db.CreditTransactions.Add(new CreditTransaction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Amount = amount,
            Description = description,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return credits;
    }

    public async Task<UserSubscription> UpgradeSubscriptionAsync(string userId, SubscriptionPlan plan)
    {
        var sub = await _db.UserSubscriptions.FirstOrDefaultAsync(s => s.UserId == userId);
        if (sub == null)
        {
            sub = new UserSubscription
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Plan = plan,
                IsActive = true,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _db.UserSubscriptions.Add(sub);
        }
        else
        {
            sub.Plan = plan;
            sub.IsActive = true;
            sub.StartDate = DateTime.UtcNow;
            sub.EndDate = DateTime.UtcNow.AddMonths(1);
            sub.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return sub;
    }
}