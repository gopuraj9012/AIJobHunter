using Microsoft.Extensions.Logging.Abstractions;
using TailorTalent.Api.Models.Subscription;
using TailorTalent.Api.Services;
using Xunit;

namespace TailorTalent.Api.Tests.Unit;

public class SubscriptionServiceTests : IDisposable
{
    private readonly TestDb _db = new();
    private readonly SubscriptionService _service;
    private const string UserId = "user-1";

    public SubscriptionServiceTests()
    {
        _service = new SubscriptionService(_db.Context, NullLogger<SubscriptionService>.Instance);
    }

    [Fact]
    public async Task Initialize_GrantsThreeFreeCredits()
    {
        var credits = await _service.InitializeUserAsync(UserId);

        Assert.Equal(3, credits.CreditsRemaining);
        Assert.Equal(0, credits.TotalCreditsPurchased);

        var status = await _service.GetStatusAsync(UserId);
        Assert.Equal(SubscriptionPlan.Free, status.Plan);
    }

    [Fact]
    public async Task Initialize_IsIdempotent()
    {
        await _service.InitializeUserAsync(UserId);
        var second = await _service.InitializeUserAsync(UserId);

        Assert.Equal(3, second.CreditsRemaining);
        Assert.Single(_db.Context.UserCredits.Where(c => c.UserId == UserId));
    }

    [Fact]
    public async Task FreePlan_CannotTailor()
    {
        await _service.InitializeUserAsync(UserId);
        var status = await _service.GetStatusAsync(UserId);
        Assert.False(status.CanTailor);
    }

    [Fact]
    public async Task PayPerTailor_WithCredits_CanTailor()
    {
        await _service.InitializeUserAsync(UserId);
        await _service.UpgradeSubscriptionAsync(UserId, SubscriptionPlan.PayPerTailor);

        var status = await _service.GetStatusAsync(UserId);
        Assert.True(status.CanTailor);
    }

    [Fact]
    public async Task Deduct_DecrementsAndLogsTransaction()
    {
        await _service.InitializeUserAsync(UserId);

        var ok = await _service.DeductCreditAsync(UserId, "tailor run");

        Assert.True(ok);
        var status = await _service.GetStatusAsync(UserId);
        Assert.Equal(2, status.CreditsRemaining);
        Assert.Contains(_db.Context.CreditTransactions, t => t.UserId == UserId && t.Amount == -1);
    }

    [Fact]
    public async Task Deduct_AtZeroCredits_Fails()
    {
        await _service.InitializeUserAsync(UserId);
        await _service.DeductCreditAsync(UserId, "1");
        await _service.DeductCreditAsync(UserId, "2");
        await _service.DeductCreditAsync(UserId, "3");

        var fourth = await _service.DeductCreditAsync(UserId, "4");

        Assert.False(fourth);
        var status = await _service.GetStatusAsync(UserId);
        Assert.Equal(0, status.CreditsRemaining);
    }

    [Fact]
    public async Task Deduct_PremiumUser_DoesNotConsumeCredits()
    {
        await _service.InitializeUserAsync(UserId);
        await _service.UpgradeSubscriptionAsync(UserId, SubscriptionPlan.Premium);

        var ok = await _service.DeductCreditAsync(UserId, "tailor run");

        Assert.True(ok);
        var status = await _service.GetStatusAsync(UserId);
        Assert.Equal(3, status.CreditsRemaining);
    }

    [Fact]
    public async Task AddCredits_IncreasesBalanceAndPurchasedTotal()
    {
        await _service.InitializeUserAsync(UserId);
        var credits = await _service.AddCreditsAsync(UserId, 10, "purchase");

        Assert.Equal(13, credits.CreditsRemaining);
        Assert.Equal(10, credits.TotalCreditsPurchased);
    }

    [Fact]
    public async Task Upgrade_SetsPlanAndActiveWindow()
    {
        var sub = await _service.UpgradeSubscriptionAsync(UserId, SubscriptionPlan.Premium);

        Assert.Equal(SubscriptionPlan.Premium, sub.Plan);
        Assert.True(sub.IsActive);
        Assert.True(sub.EndDate > sub.StartDate);

        var status = await _service.GetStatusAsync(UserId);
        Assert.True(status.CanTailor);
    }

    public void Dispose() => _db.Dispose();
}
