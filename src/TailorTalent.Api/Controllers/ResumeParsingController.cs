using Microsoft.AspNetCore.Mvc;
using TailorTalent.Api.DTOs;
using TailorTalent.Api.Services;

namespace TailorTalent.Api.Controllers;

[ApiController]
[Route("api/parsing")]
public class ResumeParsingController : ControllerBase
{
    private readonly IResumeParsingService _parsingService;
    private readonly IResumeService _resumeService;

    public ResumeParsingController(
        IResumeParsingService parsingService,
        IResumeService resumeService)
    {
        _parsingService = parsingService;
        _resumeService = resumeService;
    }

    /// <summary>
    /// Upload a resume file (PDF or DOCX) to extract text.
    /// Returns the extracted text along with a preview.
    /// </summary>
    [HttpPost("extract")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB limit
    public async Task<ActionResult<ExtractTextResponse>> ExtractText(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension != ".pdf" && extension != ".docx")
            return BadRequest("Only PDF and DOCX files are supported.");

        using var stream = file.OpenReadStream();
        var text = await _parsingService.ParseAsync(file.FileName, stream);

        return Ok(new ExtractTextResponse(
            file.FileName,
            text.Length,
            text.Length > 500 ? text[..500] + "..." : text
        ));
    }

    /// <summary>
    /// Upload a resume file (PDF or DOCX) and create a Resume entity.
    /// </summary>
    [HttpPost("upload")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB limit
    public async Task<ActionResult<ResumeDto>> UploadAndCreate(
        IFormFile file,
        [FromForm] string userId,
        [FromForm] string? title)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest("userId is required.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension != ".pdf" && extension != ".docx")
            return BadRequest("Only PDF and DOCX files are supported.");

        using var stream = file.OpenReadStream();
        var text = await _parsingService.ParseAsync(file.FileName, stream);

        var resume = await _resumeService.CreateAsync(new CreateResumeDto(
            userId,
            title ?? Path.GetFileNameWithoutExtension(file.FileName),
            text
        ));

        return CreatedAtAction("GetById", "Resumes", new { id = resume.Id }, resume);
    }
}

public record ExtractTextResponse(
    string FileName,
    int CharacterCount,
    string Preview
);