using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TailorTalent.Api.DTOs;
using TailorTalent.Api.Services;

namespace TailorTalent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ResumesController : ControllerBase
{
    private readonly IResumeService _resumeService;

    public ResumesController(IResumeService resumeService)
    {
        _resumeService = resumeService;
    }

    /// <summary>
    /// Get all resumes for the authenticated user.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ResumeDto>>> GetAll()
    {
        var userId = User.GetUserId()!;
        var resumes = await _resumeService.GetAllAsync(userId);
        return Ok(resumes);
    }

    /// <summary>
    /// Get a specific resume by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ResumeDto>> GetById(Guid id)
    {
        var resume = await _resumeService.GetByIdAsync(id);
        if (resume is null)
            return NotFound();

        return Ok(resume);
    }

    /// <summary>
    /// Create a new resume for the authenticated user.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ResumeDto>> Create([FromBody] CreateResumeDto dto)
    {
        var userId = User.GetUserId()!;
        var resume = await _resumeService.CreateAsync(dto with { UserId = userId });
        return CreatedAtAction(nameof(GetById), new { id = resume.Id }, resume);
    }

    /// <summary>
    /// Update an existing resume.
    /// </summary>
    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<ResumeDto>> Update(Guid id, [FromBody] UpdateResumeDto dto)
    {
        var resume = await _resumeService.UpdateAsync(id, dto);
        if (resume is null)
            return NotFound();

        return Ok(resume);
    }

    /// <summary>
    /// Delete a resume.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var deleted = await _resumeService.DeleteAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}