using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TailorTalent.Api.Data;
using TailorTalent.Api.Models;

namespace TailorTalent.Api.Services;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(string email, string password, string fullName);
    Task<AuthResult> LoginAsync(string email, string password);
    Task<UserProfileDto?> GetProfileAsync(string userId);
}

public class AuthResult
{
    public bool Success { get; set; }
    public string Token { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
}

public class UserProfileDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class AuthService : IAuthService
{
    private readonly TailorTalentDbContext _db;
    private readonly IConfiguration _config;
    private readonly ISubscriptionService _subscriptionService;

    public AuthService(TailorTalentDbContext db, IConfiguration config, ISubscriptionService subscriptionService)
    {
        _db = db;
        _config = config;
        _subscriptionService = subscriptionService;
    }

    public async Task<AuthResult> RegisterAsync(string email, string password, string fullName)
    {
        var existing = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (existing != null)
            return new AuthResult { Success = false, Error = "Email already registered." };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = BCryptPasswordHasher.Hash(password),
            FullName = fullName,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // Auto-initialize subscription and credits
        await _subscriptionService.InitializeUserAsync(user.Id.ToString());

        var token = GenerateJwtToken(user);
        return new AuthResult
        {
            Success = true,
            Token = token,
            UserId = user.Id.ToString(),
            Email = user.Email,
            FullName = user.FullName
        };
    }

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null || !BCryptPasswordHasher.Verify(password, user.PasswordHash))
            return new AuthResult { Success = false, Error = "Invalid email or password." };

        var token = GenerateJwtToken(user);
        return new AuthResult
        {
            Success = true,
            Token = token,
            UserId = user.Id.ToString(),
            Email = user.Email,
            FullName = user.FullName
        };
    }

    public async Task<UserProfileDto?> GetProfileAsync(string userId)
    {
        if (!Guid.TryParse(userId, out var guid)) return null;
        var user = await _db.Users.FindAsync(guid);
        if (user == null) return null;

        return new UserProfileDto
        {
            Id = user.Id.ToString(),
            Email = user.Email,
            FullName = user.FullName,
            CreatedAt = user.CreatedAt
        };
    }

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? "TailorTalentSuperSecretKeyThatIsAtLeast32Bytes!"));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName)
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"] ?? "TailorTalent",
            audience: _config["Jwt:Audience"] ?? "TailorTalent",
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

/// <summary>
/// Simple BCrypt-like password hasher (for demo purposes).
/// In production, use a real BCrypt/Argon2 library.
/// </summary>
public static class BCryptPasswordHasher
{
    public static string Hash(string password)
    {
        return Convert.ToBase64String(
            System.Security.Cryptography.SHA256.HashData(
                Encoding.UTF8.GetBytes(password + "TailorTalentSalt")));
    }

    public static bool Verify(string password, string hash)
    {
        return Hash(password) == hash;
    }
}