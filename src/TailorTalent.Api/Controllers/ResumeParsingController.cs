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
    private readonly IAiIntegrationService _aiService;

    public ResumeParsingController(
        IResumeParsingService parsingService,
        IResumeService resumeService,
        IAiIntegrationService aiService)
    {
        _parsingService = parsingService;
        _resumeService = resumeService;
        _aiService = aiService;
    }

    /// <summary>
    /// Upload a resume file (PDF or DOCX) to extract text.
    /// </summary>
    [HttpPost("extract")]
    [RequestSizeLimit(10 * 1024 * 1024)]
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
    [RequestSizeLimit(10 * 1024 * 1024)]
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

    /// <summary>
    /// Parse raw resume text into structured data using the AI service.
    /// Converts raw extracted file text into structured JSON for form pre-filling.
    /// </summary>
    [HttpPost("parse")]
    public async Task<ActionResult<ParseResumeResponse>> ParseResume([FromBody] ParseResumeRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.RawContent))
            return BadRequest("Raw resume text is required.");

        var resumeData = await _aiService.ParseResumeAsync(request.RawContent);

        return Ok(new ParseResumeResponse(
            resumeData.PersonalInfo is not null
                ? new PersonalInfoDto(
                    resumeData.PersonalInfo.Name,
                    resumeData.PersonalInfo.Email,
                    resumeData.PersonalInfo.Phone,
                    resumeData.PersonalInfo.Location,
                    resumeData.PersonalInfo.Linkedin,
                    resumeData.PersonalInfo.Website)
                : null,
            resumeData.Summary,
            resumeData.Experience?.Select(e => new ExperienceItemDto(
                e.Company, e.Title, e.Location, e.StartDate, e.EndDate, e.Description, e.Highlights
            )).ToList(),
            resumeData.Education?.Select(e => new EducationItemDto(
                e.School, e.Degree, e.Location, e.GraduationDate, e.Description
            )).ToList(),
            resumeData.Skills
        ));
    }
}

public record ExtractTextResponse(
    string FileName,
    int CharacterCount,
    string Preview
);