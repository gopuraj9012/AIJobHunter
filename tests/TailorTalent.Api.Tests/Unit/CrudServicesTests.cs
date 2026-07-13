using TailorTalent.Api.DTOs;
using TailorTalent.Api.Models;
using TailorTalent.Api.Services;
using Xunit;

namespace TailorTalent.Api.Tests.Unit;

public class CrudServicesTests : IDisposable
{
    private readonly TestDb _db = new();
    private readonly ResumeService _resumes;
    private readonly JobDescriptionService _jds;
    private readonly TailoringSessionService _sessions;
    private const string UserId = "user-1";

    public CrudServicesTests()
    {
        _resumes = new ResumeService(_db.Context);
        _jds = new JobDescriptionService(_db.Context);
        _sessions = new TailoringSessionService(_db.Context);
    }

    [Fact]
    public async Task Resume_CreateReadUpdateDelete_Roundtrips()
    {
        var created = await _resumes.CreateAsync(new CreateResumeDto(UserId, "My CV", "raw text"));
        Assert.Equal("My CV", created.Title);

        var fetched = await _resumes.GetByIdAsync(created.Id);
        Assert.NotNull(fetched);
        Assert.Equal("raw text", fetched!.RawContent);

        var updated = await _resumes.UpdateAsync(created.Id, new UpdateResumeDto("New title", null, null));
        Assert.Equal("New title", updated!.Title);
        Assert.Equal("raw text", updated.RawContent); // untouched fields preserved

        Assert.True(await _resumes.DeleteAsync(created.Id));
        Assert.Null(await _resumes.GetByIdAsync(created.Id));
    }

    [Fact]
    public async Task Resume_GetAll_FiltersByUser()
    {
        await _resumes.CreateAsync(new CreateResumeDto(UserId, "Mine", "a"));
        await _resumes.CreateAsync(new CreateResumeDto("someone-else", "Theirs", "b"));

        var mine = (await _resumes.GetAllAsync(UserId)).ToList();

        Assert.Single(mine);
        Assert.Equal("Mine", mine[0].Title);
    }

    [Fact]
    public async Task JobDescription_CreateAndUpdate_Roundtrips()
    {
        var created = await _jds.CreateAsync(new CreateJobDescriptionDto(UserId, "Sr Dev", "Acme", "jd text"));
        Assert.Equal("Acme", created.Company);

        var updated = await _jds.UpdateAsync(created.Id, new UpdateJobDescriptionDto(null, "NewCo", null, null));
        Assert.Equal("NewCo", updated!.Company);
        Assert.Equal("Sr Dev", updated.Title);
    }

    [Fact]
    public async Task Session_Create_LinksResumeAndJd()
    {
        var resume = await _resumes.CreateAsync(new CreateResumeDto(UserId, "CV", "raw"));
        var jd = await _jds.CreateAsync(new CreateJobDescriptionDto(UserId, "Role", "Acme", "jd"));

        var session = await _sessions.CreateAsync(new CreateTailoringSessionDto(UserId, resume.Id, jd.Id));

        Assert.Equal(TailoringSessionStatus.Draft, session.Status);
        Assert.Equal(resume.Id, session.ResumeId);
        Assert.Equal(jd.Id, session.JobDescriptionId);
    }

    [Fact]
    public async Task Session_Update_SetsContentScoreAndStatus()
    {
        var resume = await _resumes.CreateAsync(new CreateResumeDto(UserId, "CV", "raw"));
        var jd = await _jds.CreateAsync(new CreateJobDescriptionDto(UserId, "Role", "Acme", "jd"));
        var session = await _sessions.CreateAsync(new CreateTailoringSessionDto(UserId, resume.Id, jd.Id));

        var updated = await _sessions.UpdateAsync(session.Id, new UpdateTailoringSessionDto(
            "tailored!", "cover letter", 82, TailoringSessionStatus.Completed));

        Assert.Equal("tailored!", updated!.TailoredContent);
        Assert.Equal("cover letter", updated.CoverLetter);
        Assert.Equal(82, updated.AtsScore);
        Assert.Equal(TailoringSessionStatus.Completed, updated.Status);
    }

    [Fact]
    public async Task Session_DeleteMissing_ReturnsFalse()
    {
        Assert.False(await _sessions.DeleteAsync(Guid.NewGuid()));
    }

    public void Dispose() => _db.Dispose();
}
