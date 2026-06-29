using System.ComponentModel.DataAnnotations;

namespace TailorTalent.Api.DTOs;

public record RegisterRequest(
    [Required][EmailAddress] string Email,
    [Required][MinLength(6)] string Password,
    string FullName
);

public record LoginRequest(
    [Required][EmailAddress] string Email,
    [Required] string Password
);

public record AuthResponse(
    string Token,
    string UserId,
    string Email,
    string FullName
);

public record UserProfileResponse(
    string Id,
    string Email,
    string FullName,
    DateTime CreatedAt
);