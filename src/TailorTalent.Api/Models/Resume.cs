using System.ComponentModel.DataAnnotations;

namespace TailorTalent.Api.Models;

public class Resume
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(128)]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Original raw resume content (e.g., plain text or markdown).
    /// </summary>
    public string RawContent { get; set; } = string.Empty;

    /// <summary>
    /// Parsed resume sections stored as JSON (e.g., contact, summary, experience, education, skills).
    /// </summary>
    public string ParsedSectionsJson { get; set; } = "{}";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public ICollection<TailoringSession> TailoringSessions { get; set; } = new List<TailoringSession>();
}