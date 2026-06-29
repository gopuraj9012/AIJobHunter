using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TailorTalent.Api.DTOs;
using TailorTalent.Api.Services;

namespace TailorTalent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class JobDescriptionsController : ControllerBase
{
    private readonly IJobDescriptionService _jobDescriptionService;

    public JobDescriptionsController(IJobDescriptionService jobDescriptionService)
    {
        _jobDescriptionService = jobDescriptionService;
    }

    /// <summary>
    /// Get all job descriptions for the authenticated user.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<JobDescriptionDto>>> GetAll()
    {
        var userId = User.GetUserId()!;
        var jobDescriptions = await _jobDescriptionService.GetAllAsync(userId);
        return Ok(jobDescriptions);
    }

    /// <summary>
    /// Get a specific job description by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<JobDescriptionDto>> GetById(Guid id)
    {
        var jd = await _jobDescriptionService.GetByIdAsync(id);
        if (jd is null)
            return NotFound();

        return Ok(jd);
    }

    /// <summary>
    /// Create a new job description.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<JobDescriptionDto>> Create([FromBody] CreateJobDescriptionDto dto)
    {
        var userId = User.GetUserId()!;
        var jd = await _jobDescriptionService.CreateAsync(dto with { UserId = userId });
        return CreatedAtAction(nameof(GetById), new { id = jd.Id }, jd);
    }

    /// <summary>
    /// Update an existing job description.
    /// </summary>
    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<JobDescriptionDto>> Update(Guid id, [FromBody] UpdateJobDescriptionDto dto)
    {
        var jd = await _jobDescriptionService.UpdateAsync(id, dto);
        if (jd is null)
            return NotFound();

        return Ok(jd);
    }

    /// <summary>
    /// Delete a job description.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var deleted = await _jobDescriptionService.DeleteAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}