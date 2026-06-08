namespace TailorTalent.Api.DTOs;

public record JobDescriptionDto(
    Guid Id,
    string UserId,
    string Title,
    string Company,
    string RawContent,
    string ParsedRequirementsJson,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateJobDescriptionDto(
    string UserId,
    string Title,
    string Company,
    string RawContent
);

public record UpdateJobDescriptionDto(
    string? Title,
    string? Company,
    string? RawContent,
    string? ParsedRequirementsJson
);