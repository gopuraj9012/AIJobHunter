using TailorTalent.Api.Models.AiService;

namespace TailorTalent.Api.Services;

/// <summary>
/// Service for calling the external AI FastAPI service.
/// </summary>
public interface IAiIntegrationService
{
    /// <summary>
    /// Analyzes a job description to extract keywords, skills, and responsibilities.
    /// </summary>
    Task<JobAnalysis> AnalyzeJobAsync(string jdText, CancellationToken ct = default);

    /// <summary>
    /// Tailors a resume against a job analysis.
    /// </summary>
    Task<TailoringResult> TailorResumeAsync(string resumeText, JobAnalysis analysis, CancellationToken ct = default);

    /// <summary>
    /// Generates a cover letter for a resume and job description.
    /// </summary>
    Task<CoverLetterResponse> GenerateCoverLetterAsync(string resumeText, string jobDescription, string tone = "professional", CancellationToken ct = default);

    /// <summary>
    /// Parses a raw resume text into structured data using the AI service.
    /// </summary>
    Task<ResumeData> ParseResumeAsync(string resumeText, CancellationToken ct = default);
}

public class AiIntegrationService : IAiIntegrationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AiIntegrationService> _logger;

    public AiIntegrationService(HttpClient httpClient, ILogger<AiIntegrationService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<JobAnalysis> AnalyzeJobAsync(string jdText, CancellationToken ct = default)
    {
        _logger.LogInformation("Calling AI service: POST /analyze-job");

        var response = await _httpClient.PostAsJsonAsync("/analyze-job", new { jd_text = jdText }, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<JobAnalysis>(cancellationToken: ct);
        return result ?? new JobAnalysis();
    }

    public async Task<TailoringResult> TailorResumeAsync(string resumeText, JobAnalysis analysis, CancellationToken ct = default)
    {
        var request = new TailoringRequest
        {
            ResumeText = resumeText,
            JobAnalysis = analysis
        };

        _logger.LogInformation("Calling AI service: POST /tailor-resume");

        var response = await _httpClient.PostAsJsonAsync("/tailor-resume", request, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<TailoringResult>(cancellationToken: ct);
        return result ?? new TailoringResult();
    }

    public async Task<CoverLetterResponse> GenerateCoverLetterAsync(string resumeText, string jobDescription, string tone = "professional", CancellationToken ct = default)
    {
        var request = new CoverLetterRequest
        {
            ResumeText = resumeText,
            JobDescription = jobDescription,
            Tone = tone
        };

        _logger.LogInformation("Calling AI service: POST /generate-cover-letter");

        var response = await _httpClient.PostAsJsonAsync("/generate-cover-letter", request, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CoverLetterResponse>(cancellationToken: ct);
        return result ?? new CoverLetterResponse();
    }

    public async Task<ResumeData> ParseResumeAsync(string resumeText, CancellationToken ct = default)
    {
        var request = new ParseResumeRequest { ResumeText = resumeText };
        _logger.LogInformation("Calling AI service: POST /parse-resume");

        var response = await _httpClient.PostAsJsonAsync("/parse-resume", request, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ResumeData>(cancellationToken: ct);
        return result ?? new ResumeData();
    }
}
