using System.ComponentModel.DataAnnotations;

namespace TailorTalent.Api.Models;

public class JobDescription
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(128)]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(256)]
    public string Company { get; set; } = string.Empty;

    /// <summary>
    /// Original job description text.
    /// </summary>
    public string RawContent { get; set; } = string.Empty;

    /// <summary>
    /// Parsed requirements and keywords stored as JSON.
    /// </summary>
    public string ParsedRequirementsJson { get; set; } = "{}";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public ICollection<TailoringSession> TailoringSessions { get; set; } = new List<TailoringSession>();
}