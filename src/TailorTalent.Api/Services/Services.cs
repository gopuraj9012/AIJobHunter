using Microsoft.EntityFrameworkCore;
using TailorTalent.Api.Data;
using TailorTalent.Api.DTOs;
using TailorTalent.Api.Models;

namespace TailorTalent.Api.Services;

public class ResumeService : IResumeService
{
    private readonly TailorTalentDbContext _db;

    public ResumeService(TailorTalentDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<ResumeDto>> GetAllAsync(string userId)
    {
        return await _db.Resumes
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.UpdatedAt)
            .Select(r => ToDto(r))
            .ToListAsync();
    }

    public async Task<ResumeDto?> GetByIdAsync(Guid id)
    {
        var resume = await _db.Resumes.FindAsync(id);
        return resume is null ? null : ToDto(resume);
    }

    public async Task<ResumeDto> CreateAsync(CreateResumeDto dto)
    {
        var resume = new Resume
        {
            Id = Guid.NewGuid(),
            UserId = dto.UserId,
            Title = dto.Title,
            RawContent = dto.RawContent,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Resumes.Add(resume);
        await _db.SaveChangesAsync();

        return ToDto(resume);
    }

    public async Task<ResumeDto?> UpdateAsync(Guid id, UpdateResumeDto dto)
    {
        var resume = await _db.Resumes.FindAsync(id);
        if (resume is null) return null;

        if (dto.Title is not null)
            resume.Title = dto.Title;
        if (dto.RawContent is not null)
            resume.RawContent = dto.RawContent;
        if (dto.ParsedSectionsJson is not null)
            resume.ParsedSectionsJson = dto.ParsedSectionsJson;

        resume.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return ToDto(resume);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var resume = await _db.Resumes.FindAsync(id);
        if (resume is null) return false;

        _db.Resumes.Remove(resume);
        await _db.SaveChangesAsync();
        return true;
    }

    private static ResumeDto ToDto(Resume r) => new(
        r.Id, r.UserId, r.Title, r.RawContent,
        r.ParsedSectionsJson, r.CreatedAt, r.UpdatedAt
    );
}

public class JobDescriptionService : IJobDescriptionService
{
    private readonly TailorTalentDbContext _db;

    public JobDescriptionService(TailorTalentDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<JobDescriptionDto>> GetAllAsync(string userId)
    {
        return await _db.JobDescriptions
            .Where(j => j.UserId == userId)
            .OrderByDescending(j => j.UpdatedAt)
            .Select(j => ToDto(j))
            .ToListAsync();
    }

    public async Task<JobDescriptionDto?> GetByIdAsync(Guid id)
    {
        var jd = await _db.JobDescriptions.FindAsync(id);
        return jd is null ? null : ToDto(jd);
    }

    public async Task<JobDescriptionDto> CreateAsync(CreateJobDescriptionDto dto)
    {
        var jd = new JobDescription
        {
            Id = Guid.NewGuid(),
            UserId = dto.UserId,
            Title = dto.Title,
            Company = dto.Company,
            RawContent = dto.RawContent,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.JobDescriptions.Add(jd);
        await _db.SaveChangesAsync();

        return ToDto(jd);
    }

    public async Task<JobDescriptionDto?> UpdateAsync(Guid id, UpdateJobDescriptionDto dto)
    {
        var jd = await _db.JobDescriptions.FindAsync(id);
        if (jd is null) return null;

        if (dto.Title is not null)
            jd.Title = dto.Title;
        if (dto.Company is not null)
            jd.Company = dto.Company;
        if (dto.RawContent is not null)
            jd.RawContent = dto.RawContent;
        if (dto.ParsedRequirementsJson is not null)
            jd.ParsedRequirementsJson = dto.ParsedRequirementsJson;

        jd.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return ToDto(jd);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var jd = await _db.JobDescriptions.FindAsync(id);
        if (jd is null) return false;

        _db.JobDescriptions.Remove(jd);
        await _db.SaveChangesAsync();
        return true;
    }

    private static JobDescriptionDto ToDto(JobDescription j) => new(
        j.Id, j.UserId, j.Title, j.Company,
        j.RawContent, j.ParsedRequirementsJson, j.CreatedAt, j.UpdatedAt
    );
}

public class TailoringSessionService : ITailoringSessionService
{
    private readonly TailorTalentDbContext _db;

    public TailoringSessionService(TailorTalentDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<TailoringSessionDto>> GetAllAsync(string userId)
    {
        return await _db.TailoringSessions
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.UpdatedAt)
            .Select(t => ToDto(t))
            .ToListAsync();
    }

    public async Task<TailoringSessionDto?> GetByIdAsync(Guid id)
    {
        var session = await _db.TailoringSessions.FindAsync(id);
        return session is null ? null : ToDto(session);
    }

    public async Task<TailoringSessionDto> CreateAsync(CreateTailoringSessionDto dto)
    {
        var session = new TailoringSession
        {
            Id = Guid.NewGuid(),
            UserId = dto.UserId,
            ResumeId = dto.ResumeId,
            JobDescriptionId = dto.JobDescriptionId,
            Status = TailoringSessionStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.TailoringSessions.Add(session);
        await _db.SaveChangesAsync();

        return ToDto(session);
    }

    public async Task<TailoringSessionDto?> UpdateAsync(Guid id, UpdateTailoringSessionDto dto)
    {
        var session = await _db.TailoringSessions.FindAsync(id);
        if (session is null) return null;

        if (dto.TailoredContent is not null)
            session.TailoredContent = dto.TailoredContent;
        if (dto.CoverLetter is not null)
            session.CoverLetter = dto.CoverLetter;
        if (dto.AtsScore.HasValue)
            session.AtsScore = dto.AtsScore;
        if (dto.Status.HasValue)
            session.Status = dto.Status.Value;

        session.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return ToDto(session);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var session = await _db.TailoringSessions.FindAsync(id);
        if (session is null) return false;

        _db.TailoringSessions.Remove(session);
        await _db.SaveChangesAsync();
        return true;
    }

    private static TailoringSessionDto ToDto(TailoringSession t) => new(
        t.Id, t.UserId, t.ResumeId, t.JobDescriptionId,
        t.TailoredContent, t.CoverLetter, t.AtsScore,
        t.Status, t.CreatedAt, t.UpdatedAt
    );
}