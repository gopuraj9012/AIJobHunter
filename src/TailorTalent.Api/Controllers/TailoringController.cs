using Microsoft.AspNetCore.Mvc;
using TailorTalent.Api.DTOs;
using TailorTalent.Api.Models;
using TailorTalent.Api.Services;

namespace TailorTalent.Api.Controllers;

[ApiController]
[Route("api/tailoring")]
public class TailoringController : ControllerBase
{
    private readonly IAiIntegrationService _aiService;
    private readonly IResumeService _resumeService;
    private readonly IJobDescriptionService _jobDescriptionService;
    private readonly ITailoringSessionService _sessionService;
    private readonly ISubscriptionService _subscriptionService;

    public TailoringController(
        IAiIntegrationService aiService,
        IResumeService resumeService,
        IJobDescriptionService jobDescriptionService,
        ITailoringSessionService sessionService,
        ISubscriptionService subscriptionService)
        ITailoringSessionService sessionService)
    {
        _aiService = aiService;
        _resumeService = resumeService;
        _jobDescriptionService = jobDescriptionService;
        _sessionService = sessionService;
        _subscriptionService = subscriptionService;
    }

    /// <summary>
    /// Analyze a job description text to extract keywords and skills.
    /// </summary>
    [HttpPost("analyze")]
    public async Task<ActionResult<AnalyzeJobResponse>> AnalyzeJob([FromBody] AnalyzeJobRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RawContent))
            return BadRequest("Job description text is required.");

        var analysis = await _aiService.AnalyzeJobAsync(request.RawContent);

        return Ok(new AnalyzeJobResponse(
            analysis.Keywords,
            analysis.RequiredSkills,
            analysis.PreferredSkills,
            analysis.CoreResponsibilities
        ));
    }

    /// <summary>
    /// Full tailoring workflow: analyzes the job, tailors the resume, and saves to a session.
    /// Creates or updates a TailoringSession with the AI results.
    /// </summary>
    [HttpPost("tailor")]
    public async Task<ActionResult<TailorResumeResponse>> TailorResume([FromBody] TailorResumeRequest request)
    {
        // 1. Fetch the resume and job description
        var resume = await _resumeService.GetByIdAsync(request.ResumeId);
        if (resume is null)
            return NotFound($"Resume with ID {request.ResumeId} not found.");

        var jobDescription = await _jobDescriptionService.GetByIdAsync(request.JobDescriptionId);
        if (jobDescription is null)
            return NotFound($"JobDescription with ID {request.JobDescriptionId} not found.");

        // 2. Check credits - ensure user has sufficient balance
        var status = await _subscriptionService.GetStatusAsync(resume.UserId);
        if (!status.CanTailor)
            return PaymentRequired($"Insufficient credits. User {resume.UserId} has {status.CreditsRemaining} credits remaining and is on the {status.PlanName} plan. Please upgrade or purchase more credits.");

        // 3. Deduct a credit
        var deducted = await _subscriptionService.DeductCreditAsync(resume.UserId, "Tailored resume");
        if (!deducted)
            return PaymentRequired("Failed to deduct credit. Please check your balance.");

        // 2. Analyze the job description
        var analysis = await _aiService.AnalyzeJobAsync(jobDescription.RawContent);

        // 3. Store the analysis results on the job description entity
        var parsedJson = System.Text.Json.JsonSerializer.Serialize(analysis);
        await _jobDescriptionService.UpdateAsync(request.JobDescriptionId, new UpdateJobDescriptionDto(
            null, null, null, parsedJson));

        // 4. Tailor the resume
        var tailoringResult = await _aiService.TailorResumeAsync(resume.RawContent, new Models.AiService.JobAnalysis
        {
            Keywords = analysis.Keywords,
            RequiredSkills = analysis.RequiredSkills,
            PreferredSkills = analysis.PreferredSkills,
            CoreResponsibilities = analysis.CoreResponsibilities
        });

        // 5. Create or update a tailoring session
        var session = await _sessionService.CreateAsync(new CreateTailoringSessionDto(
            resume.UserId, request.ResumeId, request.JobDescriptionId));

        await _sessionService.UpdateAsync(session.Id, new UpdateTailoringSessionDto(
            tailoringResult.ImprovementSuggestions.Count > 0
                ? tailoringResult.ImprovementSuggestions[0].SuggestedRewrite
                : resume.RawContent,
            null,
            tailoringResult.MatchScore,
            TailoringSessionStatus.Completed));

        var suggestions = tailoringResult.ImprovementSuggestions
            .Select(s => new ImprovementSuggestion(s.Section, s.Feedback, s.SuggestedRewrite))
            .ToList();

        return Ok(new TailorResumeResponse(
            session.Id,
            tailoringResult.ImprovementSuggestions.Count > 0
                ? tailoringResult.ImprovementSuggestions[0].SuggestedRewrite
                : resume.RawContent,
            tailoringResult.MatchScore,
            tailoringResult.MissingKeywords,
            tailoringResult.Strengths,
            tailoringResult.Weaknesses,
            suggestions
        ));
    }

    /// <summary>
    /// Generate a cover letter for an existing tailoring session.
    /// </summary>
    [HttpPost("cover-letter")]
    public async Task<ActionResult<CoverLetterGenerateResponse>> GenerateCoverLetter([FromBody] CoverLetterGenerateRequest request)
    {
        var session = await _sessionService.GetByIdAsync(request.SessionId);
        if (session is null)
            return NotFound($"TailoringSession with ID {request.SessionId} not found.");

        var resume = await _resumeService.GetByIdAsync(session.ResumeId);
        if (resume is null)
            return NotFound($"Resume with ID {session.ResumeId} not found.");

        var jobDescription = await _jobDescriptionService.GetByIdAsync(session.JobDescriptionId);
        if (jobDescription is null)
            return NotFound($"JobDescription with ID {session.JobDescriptionId} not found.");

        var coverLetter = await _aiService.GenerateCoverLetterAsync(
            resume.RawContent, jobDescription.RawContent, request.Tone);

        // Save the cover letter to the session
        await _sessionService.UpdateAsync(request.SessionId, new UpdateTailoringSessionDto(
            null, coverLetter.Content, null, null));

        return Ok(new CoverLetterGenerateResponse(
            coverLetter.Content,
            coverLetter.KeyPointsAddressed,
            coverLetter.TailoringNotes
        ));
    }

    private ObjectResult PaymentRequired(string message)
    {
        return StatusCode(402, new { error = message });
    }
}