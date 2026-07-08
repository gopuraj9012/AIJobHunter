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

public record TailorResumeResponse(
    Guid SessionId,
    string TailoredContent,
    int AtsScore,
    List<string> MissingKeywords,
    List<string> Strengths,
    List<string> Weaknesses,
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

public record ParseResumeRequestDto(string RawContent);

public record ParseResumeResponse(
    PersonalInfoDto? PersonalInfo,
    string? Summary,
    List<ExperienceItemDto>? Experience,
    List<EducationItemDto>? Education,
    List<string>? Skills
);

public record PersonalInfoDto(
    string? Name,
    string? Email,
    string? Phone,
    string? Location,
    string? Linkedin,
    string? Website
);

public record ExperienceItemDto(
    string? Company,
    string? Title,
    string? Location,
    string? StartDate,
    string? EndDate,
    string? Description,
    List<string>? Highlights
);

public record EducationItemDto(
    string? School,
    string? Degree,
    string? Location,
    string? GraduationDate,
    string? Description
);