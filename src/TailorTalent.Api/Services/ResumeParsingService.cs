namespace TailorTalent.Api.Services;

/// <summary>
/// Service for parsing resume files (PDF, DOCX) into plain text.
/// </summary>
public interface IResumeParsingService
{
    /// <summary>
    /// Parses a PDF or DOCX file and extracts clean text content.
    /// </summary>
    /// <param name="fileName">Original file name (used to determine format).</param>
    /// <param name="stream">File content stream.</param>
    /// <returns>Extracted and cleaned text.</returns>
    Task<string> ParseAsync(string fileName, Stream stream);
}

public class ResumeParsingService : IResumeParsingService
{
    private readonly ILogger<ResumeParsingService> _logger;

    public ResumeParsingService(ILogger<ResumeParsingService> logger)
    {
        _logger = logger;
    }

    public async Task<string> ParseAsync(string fileName, Stream stream)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        string rawText = extension switch
        {
            ".pdf" => ParsePdf(stream),
            ".docx" => ParseDocx(stream),
            _ => throw new NotSupportedException($"File format '{extension}' is not supported. Use PDF or DOCX.")
        };

        return CleanText(rawText);
    }

    private string ParsePdf(Stream stream)
    {
        _logger.LogInformation("Parsing PDF resume...");

        using var document = UglyToad.PdfPig.PdfDocument.Open(stream);
        var text = new System.Text.StringBuilder();

        foreach (var page in document.GetPages())
        {
            text.AppendLine(page.Text);
        }

        return text.ToString();
    }

    private string ParseDocx(Stream stream)
    {
        _logger.LogInformation("Parsing DOCX resume...");

        using var doc = DocumentFormat.OpenXml.Packaging.WordprocessingDocument.Open(stream, false);
        var mainPart = doc.MainDocumentPart;
        if (mainPart?.Document?.Body is null) return string.Empty;
        var body = mainPart.Document.Body;

        var text = new System.Text.StringBuilder();

        foreach (var para in body.Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>())
        {
            text.AppendLine(para.InnerText);
        }

        return text.ToString();
    }

    /// <summary>
    /// Cleans extracted text: collapses multiple whitespace, trims, removes empty lines at start/end.
    /// </summary>
    private static string CleanText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // Split into lines, trim each, remove empty lines
        var lines = text
            .Split('\n')
            .Select(line => line.Trim())
            .Where(line => line.Length > 0);

        var cleaned = string.Join("\n", lines);

        // Collapse multiple spaces into one
        cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, @"[ ]{2,}", " ");

        return cleaned;
    }
}