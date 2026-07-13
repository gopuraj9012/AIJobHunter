using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TailorTalent.Api.DTOs;
using TailorTalent.Api.Services;

namespace TailorTalent.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Register a new user account.
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request.Email, request.Password, request.FullName);
        if (!result.Success)
            return BadRequest(new { error = result.Error });

        return Ok(new AuthResponse(result.Token, result.UserId, result.Email, result.FullName));
    }

    /// <summary>
    /// Login with email and password to receive a JWT token.
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request.Email, request.Password);
        if (!result.Success)
            return Unauthorized(new { error = result.Error });

        return Ok(new AuthResponse(result.Token, result.UserId, result.Email, result.FullName));
    }

    /// <summary>
    /// Get the authenticated user's profile.
    /// </summary>
    [HttpGet("profile")]
    [Authorize]
    public async Task<ActionResult<UserProfileResponse>> GetProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var profile = await _authService.GetProfileAsync(userId);
        if (profile == null)
            return NotFound();

        return Ok(new UserProfileResponse(profile.Id, profile.Email, profile.FullName, profile.CreatedAt));
    }
}