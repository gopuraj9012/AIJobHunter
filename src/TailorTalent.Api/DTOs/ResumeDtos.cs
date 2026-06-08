namespace TailorTalent.Api.DTOs;

public record ResumeDto(
    Guid Id,
    string UserId,
    string Title,
    string RawContent,
    string ParsedSectionsJson,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateResumeDto(
    string UserId,
    string Title,
    string RawContent
);

public record UpdateResumeDto(
    string? Title,
    string? RawContent,
    string? ParsedSectionsJson
);