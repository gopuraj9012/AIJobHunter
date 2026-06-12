using System.ComponentModel.DataAnnotations;

namespace TailorTalent.Api.Models.Subscription;

/// <summary>
/// Subscription plan types.
/// </summary>
public enum SubscriptionPlan
{
    Free = 0,
    Premium = 1,
    PayPerTailor = 2
}

/// <summary>
/// Tracks a user's subscription plan and renewal information.
/// </summary>
public class UserSubscription
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(128)]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// The user's current subscription plan.
    /// </summary>
    public SubscriptionPlan Plan { get; set; } = SubscriptionPlan.Free;

    /// <summary>
    /// When the subscription started.
    /// </summary>
    public DateTime StartDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the current subscription period ends (null for lifetime/free).
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Whether the subscription is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Tracks user's available credits for pay-per-tailor usage.
/// </summary>
public class UserCredits
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(128)]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Number of tailoring credits remaining.
    /// </summary>
    public int CreditsRemaining { get; set; } = 0;

    /// <summary>
    /// Total credits purchased/earned.
    /// </summary>
    public int TotalCreditsPurchased { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Record of each credit transaction (purchase or deduction).
/// </summary>
public class CreditTransaction
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(128)]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Amount changed (positive = added, negative = deducted).
    /// </summary>
    public int Amount { get; set; }

    /// <summary>
    /// Description of the transaction (e.g., "Purchase 10 credits", "Tailor resume").
    /// </summary>
    [MaxLength(512)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Optional reference to a TailoringSession.
    /// </summary>
    public Guid? TailoringSessionId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}