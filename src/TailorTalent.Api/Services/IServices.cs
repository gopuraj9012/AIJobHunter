using TailorTalent.Api.DTOs;

namespace TailorTalent.Api.Services;

public interface IResumeService
{
    Task<IEnumerable<ResumeDto>> GetAllAsync(string userId);
    Task<ResumeDto?> GetByIdAsync(Guid id);
    Task<ResumeDto> CreateAsync(CreateResumeDto dto);
    Task<ResumeDto?> UpdateAsync(Guid id, UpdateResumeDto dto);
    Task<bool> DeleteAsync(Guid id);
}

public interface IJobDescriptionService
{
    Task<IEnumerable<JobDescriptionDto>> GetAllAsync(string userId);
    Task<JobDescriptionDto?> GetByIdAsync(Guid id);
    Task<JobDescriptionDto> CreateAsync(CreateJobDescriptionDto dto);
    Task<JobDescriptionDto?> UpdateAsync(Guid id, UpdateJobDescriptionDto dto);
    Task<bool> DeleteAsync(Guid id);
}

public interface ITailoringSessionService
{
    Task<IEnumerable<TailoringSessionDto>> GetAllAsync(string userId);
    Task<TailoringSessionDto?> GetByIdAsync(Guid id);
    Task<TailoringSessionDto> CreateAsync(CreateTailoringSessionDto dto);
    Task<TailoringSessionDto?> UpdateAsync(Guid id, UpdateTailoringSessionDto dto);
    Task<bool> DeleteAsync(Guid id);
}