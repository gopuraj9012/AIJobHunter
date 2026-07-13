using TailorTalent.Api.Models;

namespace TailorTalent.Api.DTOs;

public record TailoringSessionDto(
    Guid Id,
    string UserId,
    Guid ResumeId,
    Guid JobDescriptionId,
    string TailoredContent,
    string CoverLetter,
    int? AtsScore,
    TailoringSessionStatus Status,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateTailoringSessionDto(
    string UserId,
    Guid ResumeId,
    Guid JobDescriptionId
);

public record UpdateTailoringSessionDto(
    string? TailoredContent,
    string? CoverLetter,
    int? AtsScore,
    TailoringSessionStatus? Status
);