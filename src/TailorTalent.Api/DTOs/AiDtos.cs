namespace TailorTalent.Api.DTOs;

public record AnalyzeJobRequest(string RawContent);

public record AnalyzeJobResponse(
    List<string> Keywords,
    List<string> RequiredSkills,
    List<string> PreferredSkills,
    List<string> CoreResponsibilities
);

public record TailorResumeRequest(
    Guid ResumeId,
    Guid JobDescriptionId,
    string? Tone
);

public record ScoreBreakdownDto(
    int Skills,
    int Experience,
    int Education
);

public record TailorResumeResponse(
    Guid SessionId,
    string TailoredContent,
    int AtsScore,
    ScoreBreakdownDto AtsScoreBreakdown,
    List<string> MissingKeywords,
    List<string> HighImpactMissingKeywords,
    List<string> Strengths,
    List<string> Weaknesses,
    List<string> ExperienceBulletSuggestions,
    List<ImprovementSuggestion> ImprovementSuggestions
);

public record ImprovementSuggestion(
    string Section,
    string Feedback,
    string SuggestedRewrite
);

public record CoverLetterGenerateRequest(
    Guid SessionId,
    string Tone
);

public record CoverLetterGenerateResponse(
    string Content,
    List<string> KeyPointsAddressed,
    string TailoringNotes
);
