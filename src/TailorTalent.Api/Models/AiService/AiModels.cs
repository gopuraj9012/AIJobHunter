using System.Text.Json.Serialization;

namespace TailorTalent.Api.Models.AiService;

/// <summary>
/// Response from POST /analyze-job
/// </summary>
public class JobAnalysis
{
    [JsonPropertyName("keywords")]
    public List<string> Keywords { get; set; } = new();

    [JsonPropertyName("required_skills")]
    public List<string> RequiredSkills { get; set; } = new();

    [JsonPropertyName("preferred_skills")]
    public List<string> PreferredSkills { get; set; } = new();

    [JsonPropertyName("core_responsibilities")]
    public List<string> CoreResponsibilities { get; set; } = new();
}

/// <summary>
/// Request body for POST /tailor-resume
/// </summary>
public class TailoringRequest
{
    [JsonPropertyName("resume_text")]
    public string ResumeText { get; set; } = string.Empty;

    [JsonPropertyName("job_analysis")]
    public JobAnalysis JobAnalysis { get; set; } = new();
}

/// <summary>
/// Score breakdown by category.
/// </summary>
public class ScoreBreakdown
{
    [JsonPropertyName("skills")]
    public int Skills { get; set; }

    [JsonPropertyName("experience")]
    public int Experience { get; set; }

    [JsonPropertyName("education")]
    public int Education { get; set; }
}

/// <summary>
/// Response from POST /tailor-resume
/// </summary>
public class TailoringResult
{
    [JsonPropertyName("match_score")]
    public int MatchScore { get; set; }

    [JsonPropertyName("match_score_breakdown")]
    public ScoreBreakdown MatchScoreBreakdown { get; set; } = new();

    [JsonPropertyName("missing_keywords")]
    public List<string> MissingKeywords { get; set; } = new();

    [JsonPropertyName("high_impact_missing_keywords")]
    public List<string> HighImpactMissingKeywords { get; set; } = new();

    [JsonPropertyName("strengths")]
    public List<string> Strengths { get; set; } = new();

    [JsonPropertyName("weaknesses")]
    public List<string> Weaknesses { get; set; } = new();

    [JsonPropertyName("experience_bullet_suggestions")]
    public List<string> ExperienceBulletSuggestions { get; set; } = new();

    [JsonPropertyName("improvement_suggestions")]
    public List<Suggestion> ImprovementSuggestions { get; set; } = new();
}

/// <summary>
/// A single improvement suggestion from the AI.
/// </summary>
public class Suggestion
{
    [JsonPropertyName("section")]
    public string Section { get; set; } = string.Empty;

    [JsonPropertyName("feedback")]
    public string Feedback { get; set; } = string.Empty;

    [JsonPropertyName("suggested_rewrite")]
    public string SuggestedRewrite { get; set; } = string.Empty;
}

/// <summary>
/// Request body for POST /generate-cover-letter
/// </summary>
public class CoverLetterRequest
{
    [JsonPropertyName("resume_text")]
    public string ResumeText { get; set; } = string.Empty;

    [JsonPropertyName("job_description")]
    public string JobDescription { get; set; } = string.Empty;

    [JsonPropertyName("tone")]
    public string Tone { get; set; } = "professional";
}

/// <summary>
/// Response from POST /generate-cover-letter
/// </summary>
public class CoverLetterResponse
{
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("key_points_addressed")]
    public List<string> KeyPointsAddressed { get; set; } = new();

    [JsonPropertyName("tailoring_notes")]
    public string TailoringNotes { get; set; } = string.Empty;
}
