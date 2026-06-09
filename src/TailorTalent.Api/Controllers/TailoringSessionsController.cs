using Microsoft.AspNetCore.Mvc;
using TailorTalent.Api.DTOs;
using TailorTalent.Api.Services;

namespace TailorTalent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TailoringSessionsController : ControllerBase
{
    private readonly ITailoringSessionService _sessionService;

    public TailoringSessionsController(ITailoringSessionService sessionService)
    {
        _sessionService = sessionService;
    }

    /// <summary>
    /// Get all tailoring sessions for a user.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TailoringSessionDto>>> GetAll([FromQuery] string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest("userId query parameter is required.");

        var sessions = await _sessionService.GetAllAsync(userId);
        return Ok(sessions);
    }

    /// <summary>
    /// Get a specific tailoring session by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TailoringSessionDto>> GetById(Guid id)
    {
        var session = await _sessionService.GetByIdAsync(id);
        if (session is null)
            return NotFound();

        return Ok(session);
    }

    /// <summary>
    /// Create a new tailoring session linking a resume and job description.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<TailoringSessionDto>> Create([FromBody] CreateTailoringSessionDto dto)
    {
        var session = await _sessionService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = session.Id }, session);
    }

    /// <summary>
    /// Update an existing tailoring session (e.g., set tailored content, cover letter, ATS score, status).
    /// </summary>
    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<TailoringSessionDto>> Update(Guid id, [FromBody] UpdateTailoringSessionDto dto)
    {
        var session = await _sessionService.UpdateAsync(id, dto);
        if (session is null)
            return NotFound();

        return Ok(session);
    }

    /// <summary>
    /// Delete a tailoring session.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var deleted = await _sessionService.DeleteAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}