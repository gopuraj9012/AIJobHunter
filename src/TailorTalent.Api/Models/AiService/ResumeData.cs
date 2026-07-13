using System.Text.Json.Serialization;

namespace TailorTalent.Api.Models.AiService;

/// <summary>
/// Structured resume data returned by POST /parse-resume on the AI service.
/// </summary>
public class ResumeData
{
    [JsonPropertyName("personal_info")]
    public PersonalInfo? PersonalInfo { get; set; }

    [JsonPropertyName("summary")]
    public string? Summary { get; set; }

    [JsonPropertyName("experience")]
    public List<ExperienceItem>? Experience { get; set; }

    [JsonPropertyName("education")]
    public List<EducationItem>? Education { get; set; }

    [JsonPropertyName("skills")]
    public List<string>? Skills { get; set; }
}

public class PersonalInfo
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("linkedin")]
    public string? Linkedin { get; set; }

    [JsonPropertyName("website")]
    public string? Website { get; set; }
}

public class ExperienceItem
{
    [JsonPropertyName("company")]
    public string? Company { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("start_date")]
    public string? StartDate { get; set; }

    [JsonPropertyName("end_date")]
    public string? EndDate { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("highlights")]
    public List<string>? Highlights { get; set; }
}

public class EducationItem
{
    [JsonPropertyName("school")]
    public string? School { get; set; }

    [JsonPropertyName("degree")]
    public string? Degree { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("graduation_date")]
    public string? GraduationDate { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}
